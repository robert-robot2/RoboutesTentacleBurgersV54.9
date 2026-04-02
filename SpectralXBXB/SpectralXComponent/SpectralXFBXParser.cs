using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SpectralXBXB.SpectralXComponent
{
    /// <summary>
    /// Pure C# FBX Parser - Works in Blazor WASM
    /// Parses binary FBX format and extracts mesh data including textures
    /// Converts directly to System.Numerics types
    /// </summary>
    public class SpectralXFBXParser
    {
        private const int FBX_BINARY_MAGIC = 0x58424B;
        private const string FBX_ASCII_HEADER = "; FBX";

        public static SpectralXMesh Parse(byte[] data, string meshName = "FBXMesh")
        {
            try
            {
                Console.WriteLine($"[FBXParser] Starting parse of {data.Length} bytes");

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

        private static bool IsBinaryFBX(byte[] data)
        {
            if (data.Length < 23) return false;
            string header = Encoding.ASCII.GetString(data, 0, Math.Min(18, data.Length));
            return header.StartsWith("Kaydara FBX Binary");
        }

        private static SpectralXMesh ParseBinaryFBX(byte[] data, string meshName)
        {
            var mesh = new SpectralXMesh(meshName);

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                reader.BaseStream.Seek(23, SeekOrigin.Begin);
                int version = reader.ReadInt32();
                Console.WriteLine($"[FBXParser] FBX Version: {version}");

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
                ExtractMeshData(rootNodes, mesh);
            }

            return mesh;
        }

        private static FBXNode ReadNode(BinaryReader reader, int version, int depth = 0)
        {
            if (depth > 64)
            {
                Console.WriteLine("[FBXParser] Max node depth exceeded, aborting");
                return null;
            }
            long startPos = reader.BaseStream.Position;

            long endOffset = version >= 7500 ? reader.ReadInt64() : reader.ReadUInt32();
            long numProperties = version >= 7500 ? reader.ReadInt64() : reader.ReadUInt32();
            long propertyListLen = version >= 7500 ? reader.ReadInt64() : reader.ReadUInt32();

            byte nameLen = reader.ReadByte();
            string name = nameLen > 0 ? Encoding.ASCII.GetString(reader.ReadBytes(nameLen)) : "";

            if (endOffset == 0) return null;

            var node = new FBXNode { Name = name };

            for (int i = 0; i < numProperties; i++)
            {
                var prop = ReadProperty(reader);
                node.Properties.Add(prop);
            }

            while (reader.BaseStream.Position < endOffset)
            {
                long before = reader.BaseStream.Position;

                var child = ReadNode(reader, version, depth + 1);
                if (child == null) break;

                if (reader.BaseStream.Position <= before)
                {
                    Console.WriteLine("[FBXParser] ReadNode did not advance stream, aborting");
                    break;
                }

                node.Children.Add(child);
            }


            if (reader.BaseStream.Position < endOffset)
                reader.BaseStream.Seek(endOffset, SeekOrigin.Begin);

            return node;
        }

        private static FBXProperty ReadProperty(BinaryReader reader)
        {
            char typeCode = (char)reader.ReadByte();
            var prop = new FBXProperty { TypeCode = typeCode };

            switch (typeCode)
            {
                case 'Y': prop.Value = reader.ReadInt16(); break;
                case 'C': prop.Value = reader.ReadByte() != 0; break;
                case 'I': prop.Value = reader.ReadInt32(); break;
                case 'F': prop.Value = reader.ReadSingle(); break;
                case 'D': prop.Value = reader.ReadDouble(); break;
                case 'L': prop.Value = reader.ReadInt64(); break;
                case 'S':
                    int len = reader.ReadInt32();
                    prop.Value = Encoding.ASCII.GetString(reader.ReadBytes(len));
                    break;
                case 'R':
                    int rawLen = reader.ReadInt32();
                    prop.Value = reader.ReadBytes(rawLen);
                    break;
                case 'f': prop.Value = ReadArray(reader, r => r.ReadSingle()); break;
                case 'd': prop.Value = ReadArray(reader, r => r.ReadDouble()); break;
                case 'l': prop.Value = ReadArray(reader, r => r.ReadInt64()); break;
                case 'i': prop.Value = ReadArray(reader, r => r.ReadInt32()); break;
                case 'b': prop.Value = ReadArray(reader, r => r.ReadByte() != 0); break;
                default:
                    Console.WriteLine($"[FBXParser] Unknown property type: {typeCode}");
                    break;
            }

            return prop;
        }

        private static T[] ReadArray<T>(BinaryReader reader, Func<BinaryReader, T> readFunc)
        {
            int arrayLength = reader.ReadInt32();
            int encoding = reader.ReadInt32();
            int compressedLength = reader.ReadInt32();

            if (encoding == 0)
            {
                var arr = new T[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                    arr[i] = readFunc(reader);
                return arr;
            }

            if (encoding == 1)
            {
                byte[] compressed = reader.ReadBytes(compressedLength);
                int zlibHeader = 2;
                var ms = new MemoryStream(compressed, zlibHeader, compressedLength - zlibHeader);

                using var deflate = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Decompress);
                using var uncompressedStream = new MemoryStream();
                deflate.CopyTo(uncompressedStream);

                byte[] uncompressed = uncompressedStream.ToArray();
                using var ub = new BinaryReader(new MemoryStream(uncompressed));

                var arr = new T[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                    arr[i] = readFunc(ub);

                return arr;
            }

            Console.WriteLine($"[FBXParser] Unknown array encoding {encoding}");
            return new T[0];
        }

        private static void ExtractMeshData(List<FBXNode> nodes, SpectralXMesh mesh)
        {
            var objectsNode = FindNode(nodes, "Objects");
            if (objectsNode == null)
            {
                Console.WriteLine("[FBXParser] No Objects node found");
                return;
            }

            var geometryNodes = FindNodes(objectsNode.Children, "Geometry");
            Console.WriteLine($"[FBXParser] Found {geometryNodes.Count} geometry nodes");

            if (geometryNodes.Count == 0) return;

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
                        float x = (float)vertArray[i];
                        float y = (float)vertArray[i + 1];
                        float z = (float)vertArray[i + 2];

                        mesh.Vertices.Add(new Vector3(x, z, -y));
                    }
                    Console.WriteLine($"[FBXParser] Loaded {mesh.Vertices.Count} vertices");
                }
            }

            // ========= NEW: Extract faces (Triangle / Quad) =========
            var indicesNode = FindNode(geomNode.Children, "PolygonVertexIndex");
            if (indicesNode != null && indicesNode.Properties.Count > 0)
            {
                var indexArray = indicesNode.Properties[0].Value as int[];
                if (indexArray != null)
                {
                    var polygon = new List<int>();

                    foreach (var idx in indexArray)
                    {
                        if (polygon.Count > 32)
                        {
                            Console.WriteLine("[FBXParser] Polygon overflow, aborting face");
                            polygon.Clear();
                            continue;
                        }
                        int actualIndex = idx < 0 ? ~idx : idx;
                        polygon.Add(actualIndex);

                        if (idx < 0) // end of polygon
                        {
                            if (polygon.Count == 3)
                            {
                                mesh.AddTriangleFace(
                                    polygon[0],
                                    polygon[1],
                                    polygon[2]
                                );
                            }
                            else if (polygon.Count == 4)
                            {
                                mesh.AddQuadFace(
                                    polygon[0],
                                    polygon[1],
                                    polygon[2],
                                    polygon[3]
                                );
                            }
                            else
                            {
                                // FAN TRIANGULATION FOR NGONS
                                int v0 = polygon[0];

                                for (int i = 1; i < polygon.Count - 1; i++)
                                {
                                    mesh.AddTriangleFace(
                                        v0,
                                        polygon[i],
                                        polygon[i + 1]
                                    );
                                }

                                Console.WriteLine(
                                    $"[FBXParser] Triangulated ngon ({polygon.Count} verts) into {polygon.Count - 2} triangles"
                                );
                            }


                            polygon.Clear();
                        }
                    }

                    Console.WriteLine(
                        $"[FBXParser] Loaded {mesh.TriangleFaceCount} triangle faces, {mesh.QuadFaceCount} quad faces"
                    );
                }
            }


            // Extract normals
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
                            float nx = (float)normalArray[i];
                            float ny = (float)normalArray[i + 1];
                            float nz = (float)normalArray[i + 2];

                            mesh.Normals.Add(new Vector3(nx, nz, -ny));
                        }
                        Console.WriteLine($"[FBXParser] Loaded {mesh.Normals.Count} normals");
                    }
                }
            }

            // ========== NEW: Extract UV coordinates ==========
            var uvNode = FindNode(geomNode.Children, "LayerElementUV");
            if (uvNode != null)
            {
                var uvArrayNode = FindNode(uvNode.Children, "UV");
                if (uvArrayNode != null && uvArrayNode.Properties.Count > 0)
                {
                    var uvArray = uvArrayNode.Properties[0].Value as double[];
                    if (uvArray != null)
                    {
                        for (int i = 0; i < uvArray.Length; i += 2)
                        {
                            float u = (float)uvArray[i];
                            float v = (float)uvArray[i + 1];
                            mesh.UVs.Add(new Vector2(u, 1.0f - v)); // Flip V for correct orientation
                        }
                        Console.WriteLine($"[FBXParser] Loaded {mesh.UVs.Count} UV coordinates");
                    }
                }
            }

            // ========== NEW: Extract texture path from materials ==========
            ExtractTextureData(objectsNode, mesh);
        }

        private static void ExtractTextureData(FBXNode objectsNode, SpectralXMesh mesh)
        {
            // Find Material nodes
            var materialNodes = FindNodes(objectsNode.Children, "Material");
            if (materialNodes.Count == 0)
            {
                Console.WriteLine("[FBXParser] No materials found");
                return;
            }

            // Find Texture nodes
            var textureNodes = FindNodes(objectsNode.Children, "Texture");
            if (textureNodes.Count == 0)
            {
                Console.WriteLine("[FBXParser] No textures found");
                return;
            }

            // Find Video nodes (contain actual file paths)
            var videoNodes = FindNodes(objectsNode.Children, "Video");
            if (videoNodes.Count == 0)
            {
                Console.WriteLine("[FBXParser] No video/image nodes found");
                return;
            }

            // Extract texture file path from first video node
            foreach (var videoNode in videoNodes)
            {
                // Check for embedded image data
                var contentNode = FindNode(videoNode.Children, "Content");
                if (contentNode != null && contentNode.Properties.Count > 0)
                {
                    var imageBytes = contentNode.Properties[0].Value as byte[];
                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        Console.WriteLine($"[FBXParser] Found embedded image: {imageBytes.Length} bytes");

                        try
                        {
                            using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(imageBytes);

                            mesh.TextureData = new Vector3[image.Width, image.Height];

                            for (int x = 0; x < image.Width; x++)
                            {
                                for (int y = 0; y < image.Height; y++)
                                {
                                    var pixel = image[x, y];
                                    mesh.TextureData[x, y] = new Vector3(
                                        pixel.R / 255f,
                                        pixel.G / 255f,
                                        pixel.B / 255f
                                    );
                                }
                            }

                            Console.WriteLine($"[FBXParser] ✓ Texture loaded: {image.Width}x{image.Height}");
                            // Convert to data URL once
                            // Convert to data URL once
                            using var ms = new MemoryStream();
                            image.SaveAsPng(ms);
                            mesh.TextureDataUrl = $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}";
                            // Don't set HasTexture - it's computed from TextureData being non-null

                            return;

                          
                            
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[FBXParser] Failed to load texture: {ex.Message}");
                        }
                    }
                }
            }
        }

        private static FBXNode FindNode(List<FBXNode> nodes, string name)
        {
            foreach (var node in nodes)
            {
                if (node.Name == name) return node;
            }
            return null;
        }

        private static List<FBXNode> FindNodes(List<FBXNode> nodes, string name)
        {
            var results = new List<FBXNode>();
            foreach (var node in nodes)
            {
                if (node.Name == name) results.Add(node);
            }
            return results;
        }

        private static SpectralXMesh ParseASCIIFBX(byte[] data, string meshName)
        {
            var mesh = new SpectralXMesh(meshName);
            Console.WriteLine("[FBXParser] ASCII FBX parsing not yet implemented");
            return mesh;
        }

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