
namespace SpectralXBXB.SpectralXComponent
{
    /// <summary>
    /// Central repository for loading and storing mesh definitions.
    /// Meshes are world-space, static, and reusable.
    /// </summary>
    public class SpectralXMeshLibrary
    {
        private readonly Dictionary<string, IMesh> meshCache = new();

        public IReadOnlyDictionary<string, IMesh> Meshes => meshCache;
        public int MeshCount => meshCache.Count;

        public SpectralXMeshLibrary()
        {
            LoadBuiltInPrimitives();
        }

        public IMesh? GetMesh(string name)
        {
            meshCache.TryGetValue(name, out var mesh);
            return mesh;
        }

        public bool HasMesh(string name) => meshCache.ContainsKey(name);

    public void AddMesh(string name, IMesh mesh)
{
    mesh.Name = name;

    FinalizeMesh(mesh); // 🔥 NEW

    meshCache[name] = mesh;

    Console.WriteLine(
        $"[MeshLibrary] Added mesh: {mesh.Name} " +
        $"({mesh.Vertices.Count} verts, {mesh.Faces.Count} faces)"
    );
}


        public bool RemoveMesh(string name)
        {
            return meshCache.Remove(name);
        }

        private void LoadBuiltInPrimitives()
        {

            AddMesh("PrimSquare", CreateSquare());
            AddMesh("PrimTriangle", CreateTriangle());

            AddMesh("PrimCube", CreateCube());
            AddMesh("PrimPyramid", CreatePyramid());
        




        }

        // 2D Prim Factory

        private IMesh CreateSquare()
        {
            var mesh = new SpectralXMesh("PrimSquare");



            // Counter-clockwise winding (front-facing)
            Vector3 v0 = new Vector3(-1f, 1f, 0f); // top-left
            Vector3 v1 = new Vector3(1f, 1f, 0f); // top-right
            Vector3 v2 = new Vector3(1f, -1f, 0f); // bottom-right
            Vector3 v3 = new Vector3(-1f, -1f, 0f); // bottom-left

            mesh.AddQuadFace(v0, v1, v2, v3);

            return mesh;
        }

     
        private IMesh CreateTriangle()
        {
            var mesh = new SpectralXMesh("PrimTriangle");

            // Add triangle face (adds vertices AND face in one call)
            mesh.AddTriangleFace(
                new Vector3(0f, 1f, 0f),  // top
                new Vector3(-1f, -1f, 0f),  // bottom-left
                new Vector3(1f, -1f, 0f)   // bottom-right
            );

            return mesh;
        }
    

        // CreateBezierSphere












        // 3D Prim Factory




        /// <summary>
        /// Unit cube centered at origin.
        /// Winding is CCW when viewed from outside.
        /// </summary>
        public static IMesh CreateCube()
        {
            var mesh = new SpectralXMesh("PrimCube");

            // Front
            mesh.AddQuadFace(
                new Vector3(-1, 1, 1),
                new Vector3(1, 1, 1),
                new Vector3(1, -1, 1),
                new Vector3(-1, -1, 1)
            );

            // Back
            mesh.AddQuadFace(
                new Vector3(1, 1, -1),
                new Vector3(-1, 1, -1),
                new Vector3(-1, -1, -1),
                new Vector3(1, -1, -1)
            );

            // Left
            mesh.AddQuadFace(
                new Vector3(-1, 1, -1),
                new Vector3(-1, 1, 1),
                new Vector3(-1, -1, 1),
                new Vector3(-1, -1, -1)
            );

            // Right
            mesh.AddQuadFace(
                new Vector3(1, 1, 1),
                new Vector3(1, 1, -1),
                new Vector3(1, -1, -1),
                new Vector3(1, -1, 1)
            );

            // Top
            mesh.AddQuadFace(
                new Vector3(-1, 1, -1),
                new Vector3(1, 1, -1),
                new Vector3(1, 1, 1),
                new Vector3(-1, 1, 1)
            );

            // Bottom
            mesh.AddQuadFace(
                new Vector3(-1, -1, 1),
                new Vector3(1, -1, 1),
                new Vector3(1, -1, -1),
                new Vector3(-1, -1, -1)
            );

            return mesh;
        }


        private IMesh CreatePyramid()
        {
            var mesh = new SpectralXMesh("PrimPyramid");

            // Apex
            Vector3 apex = new Vector3(0f, 1f, 0f);

            // Base (square, Y = -1)
            Vector3 b0 = new Vector3(-1f, -1f, -1f); // back-left
            Vector3 b1 = new Vector3(1f, -1f, -1f); // back-right
            Vector3 b2 = new Vector3(1f, -1f, 1f); // front-right
            Vector3 b3 = new Vector3(-1f, -1f, 1f); // front-left

            // ---- Side faces (triangles, CCW) ----
            mesh.AddTriangleFace(apex, b0, b1); // back
            mesh.AddTriangleFace(apex, b1, b2); // right
            mesh.AddTriangleFace(apex, b2, b3); // front
            mesh.AddTriangleFace(apex, b3, b0); // left

            // ---- Base (quad, CCW when viewed from below) ----
            mesh.AddQuadFace(b0, b3, b2, b1);

            return mesh;
        }
















        // Load From FBX Parser

        public async Task<SpectralXMesh?> LoadFromFBXParserAsync(
            HttpClient http,
            string url,
            string name)
        {
            try
            {
                Console.WriteLine($"[MeshLibrary] Downloading FBX from: {url}");

                var bytes = await http.GetByteArrayAsync(url);
                if (bytes == null || bytes.Length == 0)
                    return null;

                var mesh = SpectralXFBXParser.Parse(bytes, name);
                if (mesh == null || !mesh.IsValid())
                    return null;

                AddMesh(name, mesh);
                return mesh;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MeshLibrary] FBX load error: {ex.Message}");
                return null;
            }
        }

        private void FinalizeMesh(IMesh mesh)
        {
            if (mesh is not SpectralXMesh sxMesh)
                return;

            int primitiveIndex = 0;

            for (int i = 0; i < sxMesh.Faces.Count; i++)
            {
                var face = sxMesh.Faces[i];
                face.PrimitiveIndex = primitiveIndex++;
                sxMesh.Faces[i] = face;
            }

            if (!mesh.IsValid())
            {
                throw new InvalidOperationException(
                    $"Mesh '{mesh.Name}' failed validation during finalization.");
            }
        }


        public void Clear()
        {
            meshCache.Clear();
            LoadBuiltInPrimitives();
        }



    }
}
