﻿using System.Collections.Generic;
using U3DSpace.Primitives;
using U3DSpace.Primitives.MaterialPrimitives;
using U3DSpace.Primitives.MeshPrimitives;
using U3DSpace.Primitives.NodePrimitives;
using U3DSpace.Primitives.TexturePrimitives;

namespace U3DSpace
{
    public class U3DDocument
    {
        #region Constructors

        public U3DDocument()
        {
            Shaders = new Dictionary<string, Shader>();
            Materials = new Dictionary<string, Material>();
            Meshes = new Dictionary<string, Mesh>();
            Nodes = new Dictionary<string, Node>();
            Textures = new Dictionary<string, Texture>();
        }

        #endregion Constructors

        #region Properties

        public Dictionary<string, Material> Materials { get; internal set; }
        public Dictionary<string, Mesh> Meshes { get; internal set; }
        public Dictionary<string, Node> Nodes { get; internal set; }
        public Dictionary<string, Shader> Shaders { get; internal set; }
        public Dictionary<string, Texture> Textures { get; internal set; }

        #endregion Properties

        #region Methods

        public bool TryAddNode(Node node)
        {
            if ((node == null) || string.IsNullOrEmpty(node.Name) || Nodes.ContainsKey(node.Name))
            {
                return false;
            }
            if (!string.IsNullOrEmpty(node.Mesh) && !Meshes.ContainsKey(node.Mesh))
            {
                return false;
            }
            if (!string.IsNullOrEmpty(node.Parent) && !Nodes.ContainsKey(node.Parent))
            {
                return false;
            }
            Nodes.Add(node.Name, node);
            return true;
        }

        public bool TryAddMesh(Mesh mesh)
        {
            if ((mesh == null) || string.IsNullOrEmpty(mesh.Name) || Meshes.ContainsKey(mesh.Name))
            {
                return false;
            }
            if (string.IsNullOrEmpty(mesh.Shader) || !Shaders.ContainsKey(mesh.Shader))
            {
                return false;
            }
            if (!mesh.IsTrianglesCorrect())
            {
                return false;
            }
            Meshes.Add(mesh.Name, mesh);
            return true;
        }

        public bool TryAddMaterial(Material material)
        {
            if ((material == null) || string.IsNullOrEmpty(material.Name) || Materials.ContainsKey(material.Name))
            {
                return false;
            }
            Materials.Add(material.Name, material);
            return true;
        }

        public bool TryAddShader(Shader shader)
        {
            if ((shader == null) || string.IsNullOrEmpty(shader.Name) || Shaders.ContainsKey(shader.Name))
            {
                return false;
            }
            if (string.IsNullOrEmpty(shader.Material) || !Materials.ContainsKey(shader.Material))
            {
                return false;
            }
            if (!string.IsNullOrEmpty(shader.Texture) && !Textures.ContainsKey(shader.Texture))
            {
                return false;
            }
            Shaders.Add(shader.Name, shader);
            return true;
        }

        public bool TryAddTexture(Texture texture)
        {
            if ((texture == null) || string.IsNullOrEmpty(texture.Name) || Textures.ContainsKey(texture.Name))
            {
                return false;
            }
            if (texture.Image.Length == 0)
            {
                return false;
            }
            if (texture.ImageFormat == ImageFormat.Invalid)
            {
                return false;
            }
            Textures.Add(texture.Name, texture);
            return true;
        }

        public byte[] Save()
        {
            throw new System.NotImplementedException();
        }

        public void SaveToFile(string filePath)
        {
            throw new System.NotImplementedException();
        }

        public byte[] SavePDF()
        {
            throw new System.NotImplementedException();
        }

        public void SaveToFilePDF(string filePath)
        {
            throw new System.NotImplementedException();
        }

        public void Read(byte[] data)
        {
            throw new System.NotImplementedException();
        }

        public void ReadFromFile(string filePath)
        {
            throw new System.NotImplementedException();
        }

        #endregion Methods
    }
}