namespace SpectralXBXB.SpectralXComponent.SpectralXRender
{
    public enum FaceType
    {
        Triangle = 3,
        Quad = 4
    }

    public struct Face
    {
        public FaceType Type;

        // Vertex indices
        public int A;
        public int B;
        public int C;
        public int D;

        // 🔑 Stable per-mesh primitive identity
        public int PrimitiveIndex;
    }


    public readonly struct Edge
    {
        public int A { get; }
        public int B { get; }

        public Edge(int a, int b)
        {
            A = a;
            B = b;
        }
    }

    public class SpectralXMesh : IMesh
    {
        public string Name { get; set; } = string.Empty;
        public Vector4 Color { get; set; } = new Vector4(1f, 1f, 1f, 1f);
        public Vector3 Position { get; set; }
        public Vector3 Size { get; set; }

        public Vector3 Rotation { get; set; } // radians
        public List<Vector3> Vertices { get; set; } = new();
        public List<Vector2> UVs { get; set; } = new();
        public List<Vector3> Normals { get; set; } = new();
        public List<Face> Faces { get; } = new();

        private int _nextPrimitiveIndex = 0;
        public int PrimitiveCount => _nextPrimitiveIndex;

        public int VertexCount => Vertices.Count;
        public int UVCount => UVs.Count;
        public int NormalCount => Normals.Count;
        public int FaceCount => Faces.Count;

        public List<Edge> Edges { get; } = new();
        public int EdgeCount => Edges.Count;

        public bool HasRenderableFaces => Faces.Count > 0;

        public int TriangleFaceCount
        {
            get { RebuildTopologyCache(); return _triangleFaceCount; }
        }

        public int QuadFaceCount
        {
            get { RebuildTopologyCache(); return _quadFaceCount; }
        }

        public string? TexturePath { get; set; }
        public bool HasTexture => TextureData != null;
        public Vector3[,]? TextureData { get; set; }
        public string? TextureDataUrl { get; set; }

     
        private int _triangleFaceCount;
        private int _quadFaceCount;
        private bool _topologyDirty = true;

        public SpectralXMesh(string name)
        {
            Name = name;
        }

        public Matrix4x4 GetModelMatrix()
        {
            var scaleMatrix = Matrix4x4.CreateScale(Size);

            var rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(
                Rotation.Y,
                Rotation.X,
                Rotation.Z);

            var translationMatrix = Matrix4x4.CreateTranslation(Position);

            // Row-vector convention: scale → rotate → translate
            return scaleMatrix * rotationMatrix * translationMatrix;
        }


        public bool IsValid()
        {
            if (Vertices.Count == 0) return false;
            if (Faces.Count == 0) return false;

            foreach (var face in Faces)
            {
                // 🔑 Primitive identity must be valid
                if (face.PrimitiveIndex < 0)
                    return false;

                // Vertex index validation
                if (face.A < 0 || face.A >= Vertices.Count) return false;
                if (face.B < 0 || face.B >= Vertices.Count) return false;
                if (face.C < 0 || face.C >= Vertices.Count) return false;

                if (face.Type == FaceType.Quad)
                {
                    if (face.D < 0 || face.D >= Vertices.Count)
                        return false;
                }
            }

            return true;
        }


        public IMesh Clone()
        {
            return new SpectralXMesh(Name + "_Clone")
            {
                Vertices = new List<Vector3>(Vertices),
                UVs = new List<Vector2>(UVs),
                Normals = new List<Vector3>(Normals),
                Size = Size
            };
        }

        public (Vector3 min, Vector3 max) GetBounds()
        {
            if (Vertices.Count == 0)
                return (Vector3.Zero, Vector3.Zero);

            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);

            foreach (var v in Vertices)
            {
                min = Vector3.Min(min, v);
                max = Vector3.Max(max, v);
            }

            return (min, max);
        }

        public Vector3 GetCenter()
        {
            var (min, max) = GetBounds();
            return (min + max) * 0.5f;
        }

        public void AddTriangleFace(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            int startIdx = Vertices.Count;
            Vertices.Add(v0);
            Vertices.Add(v1);
            Vertices.Add(v2);

            AddTriangleFace(startIdx, startIdx + 1, startIdx + 2);
        }

        public void AddQuadFace(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int startIdx = Vertices.Count;
            Vertices.Add(v0);
            Vertices.Add(v1);
            Vertices.Add(v2);
            Vertices.Add(v3);

            AddQuadFace(startIdx, startIdx + 1, startIdx + 2, startIdx + 3);
        }

        public void AddTriangleFace(int a, int b, int c)
        {
            Faces.Add(new Face
            {
                Type = FaceType.Triangle,
                A = a,
                B = b,
                C = c,
                D = -1,
                PrimitiveIndex = _nextPrimitiveIndex++
            });

            Edges.Add(new Edge(a, b));
            Edges.Add(new Edge(b, c));
            Edges.Add(new Edge(c, a));

            _topologyDirty = true;
        }

        public void AddQuadFace(int a, int b, int c, int d)
        {
            Faces.Add(new Face
            {
                Type = FaceType.Quad,
                A = a,
                B = b,
                C = c,
                D = d,
                PrimitiveIndex = _nextPrimitiveIndex++
            });

            Edges.Add(new Edge(a, b));
            Edges.Add(new Edge(b, c));
            Edges.Add(new Edge(c, d));
            Edges.Add(new Edge(d, a));

            _topologyDirty = true;
        }


        private void RebuildTopologyCache()
        {
            if (!_topologyDirty) return;

            _triangleFaceCount = 0;
            _quadFaceCount = 0;

            foreach (var face in Faces)
            {
                if (face.Type == FaceType.Triangle) _triangleFaceCount++;
                else if (face.Type == FaceType.Quad) _quadFaceCount++;
            }

            _topologyDirty = false;
        }
    }
}
