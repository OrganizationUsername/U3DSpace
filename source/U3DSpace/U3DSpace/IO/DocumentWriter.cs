﻿using Spire.Pdf;
using Spire.Pdf.Annotations;
using Spire.Pdf.Graphics;
using System.IO;
using U3DSpace.IO.BlockIO;
using U3DSpace.Primitives;
using U3DSpace.Primitives.MeshPrimitives;
using U3DSpace.Primitives.NodePrimitives;

namespace U3DSpace.IO
{
    public static class DocumentWriter
    {
        #region PublicMethods

        public static void SavePDF(Stream pdfDocStream, U3DDocument u3dDoc)
        {
            var pdfDoc = new PdfDocument();
            PdfPageBase page = pdfDoc.Pages.Add(PdfPageSize.A4, new PdfMargins(30.0f), PdfPageRotateAngle.RotateAngle0, PdfPageOrientation.Landscape);
            var rectangle = new System.Drawing.Rectangle(0, 0, (int)(page.Size.Width - 60.0f), (int)(page.Size.Height - 60.0f));

            string u3dTempFileName = Path.GetTempFileName();
            using (var u3dDocStream = new FileStream(u3dTempFileName, FileMode.Create))
            {
                Save(u3dDocStream, u3dDoc);
            }
            Pdf3DAnnotation annotation = new Pdf3DAnnotation(rectangle, u3dTempFileName);
            File.Delete(u3dTempFileName);

            annotation.Activation = new Pdf3DActivation();
            annotation.Activation.ActivationMode = Pdf3DActivationMode.PageOpen;
            Pdf3DView View = new Pdf3DView();
            View.Background = new Pdf3DBackground(new PdfRGBColor(System.Drawing.Color.White));
            View.ViewNodeName = "DefaultView";
            View.RenderMode = new Pdf3DRendermode(Pdf3DRenderStyle.Solid);
            View.InternalName = "Default";
            View.LightingScheme = new Pdf3DLighting();
            View.LightingScheme.Style = Pdf3DLightingStyle.Day;
            annotation.Views.Add(View);
            page.AnnotationsWidget.Add(annotation);

            pdfDoc.SaveToStream(pdfDocStream, FileFormat.PDF);
        }

        public static void Save(Stream stream, U3DDocument doc)
        {
            using (var writer = new BinaryWriter(stream, doc.TextEncoding, true))
            {
                writer.Write(GetHeaderBlock(doc).ToArray());
                WriteNodes(writer, doc);
                WriteDeclarationsOfMeshes(writer, doc);
                WriteShaders(writer, doc);
                WriteMaterials(writer, doc);
                WriteDeclarationsOfTextures(writer, doc);
                WriteContinuationsOfMeshes(writer, doc);
                WriteContinuationsOfTextures(writer, doc);
            }
        }

        #endregion PublicMethods

        #region PrivateMethods

        private static void WriteNodes(BinaryWriter writer, U3DDocument doc)
        {
            foreach (Node node in doc.Nodes.Values)
            {
                if (string.IsNullOrEmpty(node.Mesh))
                {
                    writer.Write(GetGroupNodeModifierChain(node).ToArray());
                }
                else
                {
                    writer.Write(GetNodeModifierChain(node, doc).ToArray());
                }
            }
        }

        private static void WriteDeclarationsOfMeshes(BinaryWriter writer, U3DDocument doc)
        {
            foreach (Mesh mesh in doc.Meshes.Values)
            {
                writer.Write(GetModelResourceModifierChain(mesh).ToArray());
            }
        }

        private static void WriteContinuationsOfMeshes(BinaryWriter writer, U3DDocument doc)
        {
            foreach (Mesh mesh in doc.Meshes.Values)
            {
                writer.Write(GetMeshContinuationBlock(mesh).ToArray());
            }
        }

        private static void WriteShaders(BinaryWriter writer, U3DDocument doc)
        {
            foreach (Shader shader in doc.Shaders.Values)
            {
                writer.Write(GetLitTextureShaderBlock(shader).ToArray());
            }
        }

        private static void WriteMaterials(BinaryWriter writer, U3DDocument doc)
        {
            throw new System.NotImplementedException();
        }

        private static void WriteDeclarationsOfTextures(BinaryWriter writer, U3DDocument doc)
        {
            throw new System.NotImplementedException();
        }

        private static void WriteContinuationsOfTextures(BinaryWriter writer, U3DDocument doc)
        {
            throw new System.NotImplementedException();
        }

        private static Block GetHeaderBlock(U3DDocument doc)
        {
            var w = new BlockWriter();
            w.WriteI32(0x00000000); // version
            w.WriteU32(0x00000004); // profile identifier (0x00000004 - No compression mode)
            w.WriteU32(36); // declaration size
            w.WriteU64(732); // file size
            w.WriteU32(106); // character encoding: 106 = UTF-8
            //Meta data.
            w.WriteMetaU32(1); // Key/Value Pair Count
            w.WriteMetaU32(0); // Key/Value Pair Attributes; 0x00000000 - indicates the Value is formatted as a String
            w.WriteMetaString("{Created_by", doc.TextEncoding); // Key String
            w.WriteMetaString("GLTFtoU3D_converter}", doc.TextEncoding); // Value String
            return w.GetBlock(BlockType.Header);
        }

        private static Block GetGroupNodeBlock(Node node)
        {
            var w = new BlockWriter();
            w.WriteString(node.Name); // model node name
            w.WriteU32(1); // parent node count
            w.WriteString(node.Parent); // parent node name
            w.WriteArray(node.Transformation.ToArray()); // transformation
            return w.GetBlock(BlockType.GroupNode);
        }

        private static Block GetGroupNodeModifierChain(Node node)
        {
            var w = new BlockWriter();
            w.WriteString(node.Name); // modifier chain name
            w.WriteU32(0); // modifier chain type: 0 = node modifier chain
            w.WriteU32(0); // modifier chain attributes: 0 = neither bounding sphere nor
            // Bounding box info present.
            w.WritePadding();
            w.WriteU32(1); // modifier count in this chain
            w.WriteBlock(GetGroupNodeBlock(node));
            return w.GetBlock(BlockType.ModifierChain);
        }

        private static Block GetModelNodeBlock(Node node)
        {
            var w = new BlockWriter();
            w.WriteString(node.Name); // model node name
            w.WriteU32(1); // parent node count
            w.WriteString(node.Parent); // parent node name
            w.WriteArray(node.Transformation.ToArray()); // transformation
            w.WriteString(node.Mesh); // model resource name
            w.WriteU32(3); // visibility 3 = front and back
            return w.GetBlock(BlockType.ModelNode);
        }

        private static Block GetShadingModifierBlock(string nodeName, string shaderName)
        {
            var w = new BlockWriter();
            w.WriteString(nodeName); // shading modifier name
            w.WriteU32(1); // chain index
            w.WriteU32(1); // shading attributes
            w.WriteU32(1); // shading list count
            w.WriteU32(1); // shader count
            w.WriteString(shaderName); // shader name
            return w.GetBlock(BlockType.ShadingModifier);
        }

        private static Block GetNodeModifierChain(Node node, U3DDocument doc)
        {
            var w = new BlockWriter();
            w.WriteString(node.Name); // modifier chain name
            w.WriteU32(0); // modifier chain type: 0 = node modifier chain
            w.WriteU32(0); // modifier chain attributes: 0 = neither bounding sphere nor
            // Bounding box info present.
            w.WritePadding();
            w.WriteU32(2); // modifier count in this chain
            w.WriteBlock(GetModelNodeBlock(node));
            w.WriteBlock(GetShadingModifierBlock(node.Name, doc.Meshes[node.Mesh].Shader));
            return w.GetBlock(BlockType.ModifierChain);
        }

        private static Block GetMeshDeclarationBlock(Mesh mesh)
        {
            var w = new BlockWriter();
            w.WriteString(mesh.Name); // mesh name
            w.WriteU32(0); // chain index
            // Max mesh description.
            w.WriteU32(mesh.Normals.Count == 0 ? 1u : 0u); // mesh attributes: 1 = no normals
            w.WriteU32((uint)mesh.Triangles.Count); // face count
            w.WriteU32((uint)mesh.Positions.Count); // positions count
            w.WriteU32((uint)mesh.Normals.Count); // normal count
            w.WriteU32(0); // diffuse color count
            w.WriteU32(0); // specular color count
            w.WriteU32((uint)mesh.TextureCoordinates.Count); // texture coord count
            w.WriteU32(1); // shading count
            // Shading description.
            w.WriteU32(0); // shading attributes
            w.WriteU32(mesh.TextureCoordinates.Count == 0 ? 0u : 1u); // texture layer count
            if (mesh.TextureCoordinates.Count > 0)
            {
                w.WriteU32(2); // texture coord dimensions
            }
            w.WriteU32(0); // original shading id
            // Clod desc.
            w.WriteU32((uint)mesh.Positions.Count); // minimum resolution
            w.WriteU32((uint)mesh.Positions.Count); // maximum resolution
            // Resource Description.
            w.WriteU32(300); // position quality factor
            w.WriteU32(300); // normal quality factor
            w.WriteU32(300); // texture coord quality factor
            w.WriteF32(0.01f); // position inverse quant
            w.WriteF32(0.01f); // normal inverse quant
            w.WriteF32(0.01f); // texture coord inverse quant
            w.WriteF32(0.01f); // diffuse color inverse quant
            w.WriteF32(0.01f); // specular color inverse quant
            w.WriteF32(0.9f); // normal crease parameter
            w.WriteF32(0.5f); // normal update parameter
            w.WriteF32(0.985f); // normal tolerance parameter
            // Skeleton Description.
            w.WriteU32(0); // bone count
            return w.GetBlock(BlockType.MeshDeclaration);
        }

        private static Block GetModelResourceModifierChain(Mesh mesh)
        {
            var w = new BlockWriter();
            w.WriteString(mesh.Name); // modifier chain name, bonded to ModelNodeBlock.ModelResourceName
            w.WriteU32(1); // modifier chain type: 1 = model resource modifier chain
            w.WriteU32(0); // modifier chain attributes: 0 = neither bounding sphere nor
            // Bounding box info present. Padding.
            w.WritePadding();
            w.WriteU32(1); // modifier count in this chain
            w.WriteBlock(GetMeshDeclarationBlock(mesh));
            return w.GetBlock(BlockType.ModifierChain);
        }

        private static Block GetMeshContinuationBlock(Mesh mesh)
        {
            var w = new BlockWriter();
            w.WriteString(mesh.Name); // mesh name
            w.WriteU32(0); // chain index
            // Base Mesh Description.
            w.WriteU32((uint)mesh.Triangles.Count); // base face count
            w.WriteU32((uint)mesh.Positions.Count); // base position count
            w.WriteU32((uint)mesh.Normals.Count); // base normal count
            w.WriteU32(0); // base diffuse color count
            w.WriteU32(0); // base specular color count
            w.WriteU32((uint)mesh.TextureCoordinates.Count); // base texture coordinate count
            // Base Mesh Data.
            foreach (Vector3 position in mesh.Positions)
            {
                w.WriteF32(position.X);
                w.WriteF32(position.Y);
                w.WriteF32(position.Z);
            }
            foreach (Vector3 normal in mesh.Normals)
            {
                w.WriteF32(normal.X);
                w.WriteF32(normal.Y);
                w.WriteF32(normal.Z);
            }
            foreach (Vector2 textureCoordinate in mesh.TextureCoordinates)
            {
                w.WriteF32(textureCoordinate.X);
                w.WriteF32(textureCoordinate.Y);
                w.WriteF32(0);
                w.WriteF32(0);
            }
            // Base Face.
            foreach (Triangle triangle in mesh.Triangles)
            {
                w.WriteU32(0u); // shading id
                foreach (Corner corner in triangle)
                {
                    w.WriteU32((uint)corner.Position);
                    if (corner.Normal >= 0)
                    {
                        w.WriteU32((uint)corner.Normal);
                    }
                    if (corner.TextureCoordinate >= 0)
                    {
                        w.WriteU32((uint)corner.TextureCoordinate);
                    }
                }
            }
            return w.GetBlock(BlockType.MeshContinuation);
        }

        private static Block GetLitTextureShaderBlock(Shader shader)
        {
            var w = new BlockWriter();
            w.WriteString(shader.Name); // shader name
            w.WriteU32(1); // Lit texture shader attributes: 1 = Lights enabled
            w.WriteF32(0f); // Alpha Test Reference
            w.WriteU32(0x00000617); // Alpha Test Function: ALWAYS
            w.WriteU32(0x00000605); // Color Blend Function: FB_MULTIPLY
            w.WriteU32(1); // Render pass enabled flags
            w.WriteU32(string.IsNullOrEmpty(shader.Texture) ? 0u : 1u); // Shader channels (active texture count)
            w.WriteU32(0); // Alpha texture channels
            w.WriteString(shader.Material); // Material name
            // Texture information.
            if (!string.IsNullOrEmpty(shader.Texture))
            {
                w.WriteString(shader.Texture); // Texture name
                w.WriteF32(1f); // Texture Intensity
                w.WriteU8(0); // Blend function: 0 - Multiply
                w.WriteU8(1); // Blend Source, 1 - blending constant
                w.WriteF32(1f); // Blend Constant
                w.WriteU8(0x00); // Texture Mod; 0x00: TM_NONE; shader should use texture coordinates of the model
                w.WriteArray(Matrix4.GetIdentityMatrix().ToArray()); // Texture Transform Matrix Element
                w.WriteArray(Matrix4.GetIdentityMatrix().ToArray()); // Texture Wrap Transform Matrix Element
                w.WriteU8(0x03); // Texture Repeat
            }
            return w.GetBlock(BlockType.LitTextureShader);
        }

        #endregion PrivateMethods
    }
}