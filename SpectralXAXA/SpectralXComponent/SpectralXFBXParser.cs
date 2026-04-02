using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace SpectralXAXA.SpectralXComponent
{

    /// <summary>
    /// Pure C# FBX Parser - Works in Blazor WASM
    /// Parses binary FBX format and extracts mesh data
    /// Converts directly to System.Numerics types
    /// </summary>
    public class SpectralXFBXParser
    {
        private const int FBX_BINARY_MAGIC = 0x58424B;  // "KBX" in little-endian
        private const string FBX_ASCII_HEADER = "; FBX";

        /// <summary>
        /// Parse FBX from byte array and return mesh
        /// </summary>
        public static SpectralXMesh Parse(byte[] data, string meshName = "FBXMesh")
        {
            try
            {
                Console.WriteLine($"[FBXParser] Starting parse of {data.Length} bytes");

                // Detect format
                bool isBinary = IsBinaryFBX(data);
                Console.WriteLine($"[FBXParser] Format detected: {(isBinary ? "Binary" : "ASCII")}");

                if (isBinary)
                {
                    return ParseBinaryFBX(data, meshName);
                }
                else
                {
                    return ParseASCIIFBX(data, meshName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FBXParser] Parse failed: {ex.Message}");
                Console.WriteLine($"[FBXParser] Stack: {ex.StackTrace}");
                return new SpectralXMesh(meshName);
            }
        }

        /// <summary>
        /// Check if FBX is binary format
        /// </summary>
        private static bool IsBinaryFBX(byte[] data)
        {
            if (data.Length < 23) return false;

            // Check for "Kaydara FBX Binary" header
            string header = Encoding.ASCII.GetString(data, 0, Math.Min(18, data.Length));
            return header.StartsWith("Kaydara FBX Binary");
        }

        /// <summary>
        /// Parse Binary FBX format
        /// </summary>
        private static SpectralXMesh ParseBinaryFBX(byte[] data, string meshName)
        {
            var mesh = new SpectralXMesh(meshName);

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                // Skip header (23 bytes: "Kaydara FBX Binary  \0" + version)
                reader.BaseStream.Seek(23, SeekOrigin.Begin);

                // Read version
                int version = reader.ReadInt32();
                Console.WriteLine($"[FBXParser] FBX Version: {version}");

                // Parse node tree
                var rootNodes = new List<FBXNode>();
                while (reader.BaseStream.Position < reader.BaseStream.Length - 160)
                {
                    try
                    {
                        var node = ReadNode(reader, version);
                        if (node == null) break;
                        rootNodes.Add(node);
                    }
                    catch
                    {
                        break;
                    }
                }

                Console.WriteLine($"[FBXParser] Found {rootNodes.Count} root nodes");

                // Extract mesh data from nodes
                ExtractMeshData(rootNodes, mesh);
            }

            return mesh;
        }

        /// <summary>
        /// Read a single FBX node
        /// </summary>
        private static FBXNode ReadNode(BinaryReader reader, int version)
        {
            long startPos = reader.BaseStream.Position;

            // Read node record header
            long endOffset = version >= 7500 ? reader.ReadInt64() : reader.ReadUInt32();
            long numProperties = version >= 7500 ? reader.ReadInt64() : reader.ReadUInt32();
            long propertyListLen = version >= 7500 ? reader.ReadInt64() : reader.ReadUInt32();

            byte nameLen = reader.ReadByte();
            string name = nameLen > 0 ? Encoding.ASCII.GetString(reader.ReadBytes(nameLen)) : "";

            // NULL record = end of node list
            if (endOffset == 0) return null;

            var node = new FBXNode { Name = name };

            // Read properties
            for (int i = 0; i < numProperties; i++)
            {
                var prop = ReadProperty(reader);
                node.Properties.Add(prop);
            }

            // Read child nodes
            while (reader.BaseStream.Position < endOffset)
            {
                var child = ReadNode(reader, version);
                if (child == null) break;
                node.Children.Add(child);
            }

            // Seek to end of node
            if (reader.BaseStream.Position < endOffset)
                reader.BaseStream.Seek(endOffset, SeekOrigin.Begin);

            return node;
        }

        /// <summary>
        /// Read a property value
        /// </summary>
        private static FBXProperty ReadProperty(BinaryReader reader)
        {
            char typeCode = (char)reader.ReadByte();
            var prop = new FBXProperty { TypeCode = typeCode };

            switch (typeCode)
            {
                case 'Y': // 16-bit int
                    prop.Value = reader.ReadInt16();
                    break;
                case 'C': // boolean
                    prop.Value = reader.ReadByte() != 0;
                    break;
                case 'I': // 32-bit int
                    prop.Value = reader.ReadInt32();
                    break;
                case 'F': // 32-bit float
                    prop.Value = reader.ReadSingle();
                    break;
                case 'D': // 64-bit double
                    prop.Value = reader.ReadDouble();
                    break;
                case 'L': // 64-bit long
                    prop.Value = reader.ReadInt64();
                    break;
                case 'S': // string
                    int len = reader.ReadInt32();
                    prop.Value = Encoding.ASCII.GetString(reader.ReadBytes(len));
                    break;
                case 'R': // raw binary
                    int rawLen = reader.ReadInt32();
                    prop.Value = reader.ReadBytes(rawLen);
                    break;
                case 'f': // float array
                    prop.Value = ReadArray(reader, r => r.ReadSingle());
                    break;
                case 'd': // double array
                    prop.Value = ReadArray(reader, r => r.ReadDouble());
                    break;
                case 'l': // long array
                    prop.Value = ReadArray(reader, r => r.ReadInt64());
                    break;
                case 'i': // int array
                    prop.Value = ReadArray(reader, r => r.ReadInt32());
                    break;
                case 'b': // boolean array
                    prop.Value = ReadArray(reader, r => r.ReadByte() != 0);
                    break;
                default:
                    Console.WriteLine($"[FBXParser] Unknown property type: {typeCode}");
                    break;
            }

            return prop;
        }

        /// <summary>
        /// Read array property (supports compression)
        /// </summary>
        private static T[] ReadArray<T>(BinaryReader reader, Func<BinaryReader, T> readFunc)
        {
            int arrayLength = reader.ReadInt32();
            int encoding = reader.ReadInt32();
            int compressedLength = reader.ReadInt32();

            // ========== CASE 1 — Uncompressed data ==========
            if (encoding == 0)
            {
                var arr = new T[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                    arr[i] = readFunc(reader);
                return arr;
            }

            // ========== CASE 2 — ZLIB COMPRESSED ARRAY ==========
            if (encoding == 1)
            {
                // Read compressed payload
                byte[] compressed = reader.ReadBytes(compressedLength);

                // The FBX spec uses *raw* DEFLATE blocks inside a zlib wrapper.
                // We must skip the first two bytes of the zlib header.
                int zlibHeader = 2;
                var ms = new MemoryStream(compressed, zlibHeader, compressedLength - zlibHeader);

                using var deflate = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Decompress);

                // Decompress into memory buffer
                using var uncompressedStream = new MemoryStream();
                deflate.CopyTo(uncompressedStream);

                byte[] uncompressed = uncompressedStream.ToArray();

                // Now read T[] from the uncompressed buffer
                using var ub = new BinaryReader(new MemoryStream(uncompressed));

                var arr = new T[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                    arr[i] = readFunc(ub);

                return arr;
            }

            Console.WriteLine($"[FBXParser] Unknown array encoding {encoding}");
            return new T[0];
        }


        /// <summary>
        /// Extract mesh data from FBX node tree
        /// </summary>
        private static void ExtractMeshData(List<FBXNode> nodes, SpectralXMesh mesh)
        {
            // Find Objects node
            var objectsNode = FindNode(nodes, "Objects");
            if (objectsNode == null)
            {
                Console.WriteLine("[FBXParser] No Objects node found");
                return;
            }

            // Find Geometry nodes (meshes)
            var geometryNodes = FindNodes(objectsNode.Children, "Geometry");
            Console.WriteLine($"[FBXParser] Found {geometryNodes.Count} geometry nodes");

            if (geometryNodes.Count == 0) return;

            // Use first geometry node
            var geomNode = geometryNodes[0];

            // Extract vertices
            var verticesNode = FindNode(geomNode.Children, "Vertices");
            if (verticesNode != null && verticesNode.Properties.Count > 0)
            {
                var vertArray = verticesNode.Properties[0].Value as double[];
                if (vertArray != null)
                {
                    for (int i = 0; i < vertArray.Length; i += 3)
                    {
                        mesh.Vertices.Add(new Vector3(
                            (float)vertArray[i],
                            (float)vertArray[i + 1],
                            (float)vertArray[i + 2]
                        ));
                    }
                    Console.WriteLine($"[FBXParser] Loaded {mesh.Vertices.Count} vertices");
                }
            }

            // Extract indices
            var indicesNode = FindNode(geomNode.Children, "PolygonVertexIndex");
            if (indicesNode != null && indicesNode.Properties.Count > 0)
            {
                var indexArray = indicesNode.Properties[0].Value as int[];
                if (indexArray != null)
                {
                    // FBX uses negative indices to mark polygon ends
                    // Convert to triangle list
                    var polygonIndices = new List<int>();
                    foreach (var idx in indexArray)
                    {
                        int actualIndex = idx < 0 ? ~idx : idx;
                        polygonIndices.Add(actualIndex);

                        // Negative index = end of polygon
                        if (idx < 0)
                        {
                            // Triangulate polygon if needed
                            if (polygonIndices.Count == 3)
                            {
                                mesh.Indices.AddRange(polygonIndices);
                            }
                            else if (polygonIndices.Count == 4)
                            {
                                // Quad to triangles
                                mesh.Indices.Add(polygonIndices[0]);
                                mesh.Indices.Add(polygonIndices[1]);
                                mesh.Indices.Add(polygonIndices[2]);
                                mesh.Indices.Add(polygonIndices[0]);
                                mesh.Indices.Add(polygonIndices[2]);
                                mesh.Indices.Add(polygonIndices[3]);
                            }
                            polygonIndices.Clear();
                        }
                    }
                    Console.WriteLine($"[FBXParser] Loaded {mesh.Indices.Count / 3} triangles");
                }
            }

            // Extract normals (optional)
            var normalsNode = FindNode(geomNode.Children, "LayerElementNormal");
            if (normalsNode != null)
            {
                var normalArrayNode = FindNode(normalsNode.Children, "Normals");
                if (normalArrayNode != null && normalArrayNode.Properties.Count > 0)
                {
                    var normalArray = normalArrayNode.Properties[0].Value as double[];
                    if (normalArray != null)
                    {
                        for (int i = 0; i < normalArray.Length; i += 3)
                        {
                            mesh.Normals.Add(new Vector3(
                                (float)normalArray[i],
                                (float)normalArray[i + 1],
                                (float)normalArray[i + 2]
                            ));
                        }
                        Console.WriteLine($"[FBXParser] Loaded {mesh.Normals.Count} normals");
                    }
                }
            }
        }

        /// <summary>
        /// Find first node with given name
        /// </summary>
        private static FBXNode FindNode(List<FBXNode> nodes, string name)
        {
            foreach (var node in nodes)
            {
                if (node.Name == name) return node;
            }
            return null;
        }

        /// <summary>
        /// Find all nodes with given name
        /// </summary>
        private static List<FBXNode> FindNodes(List<FBXNode> nodes, string name)
        {
            var results = new List<FBXNode>();
            foreach (var node in nodes)
            {
                if (node.Name == name) results.Add(node);
            }
            return results;
        }

        /// <summary>
        /// Parse ASCII FBX format (simpler, text-based)
        /// </summary>
        private static SpectralXMesh ParseASCIIFBX(byte[] data, string meshName)
        {
            var mesh = new SpectralXMesh(meshName);
            Console.WriteLine("[FBXParser] ASCII FBX parsing not yet implemented");
            // TODO: Implement ASCII parser if needed
            return mesh;
        }

        // ========== DATA STRUCTURES ==========

        private class FBXNode
        {
            public string Name { get; set; } = "";
            public List<FBXProperty> Properties { get; set; } = new();
            public List<FBXNode> Children { get; set; } = new();
        }

        private class FBXProperty
        {
            public char TypeCode { get; set; }
            public object Value { get; set; }
        }
    }
}