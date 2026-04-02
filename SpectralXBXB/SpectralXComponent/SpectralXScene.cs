
namespace SpectralXBXB.SpectralXComponent
{
    /// <summary>
    /// Represents a collection of meshes and lights in the world.
    /// No rendering, no viewport, no camera knowledge.
    /// </summary>
    public class SpectralXScene
    {
        private readonly List<IMesh> _meshes = new();
        private readonly List<SpectralXLight> _lights = new();

        public IReadOnlyList<IMesh> Meshes => _meshes;
        public IReadOnlyList<SpectralXLight> Lights => _lights;
       

        /// <summary>
        /// All entities in the scene
        /// </summary>
    

        public void AddMesh(IMesh mesh)
        {
            if (mesh == null)
                return;
            _meshes.Add(mesh);
        }

        public void RemoveMesh(IMesh mesh)
        {
            _meshes.Remove(mesh);
        }

        public void AddLight(SpectralXLight light)
        {
            if (light == null)
                return;
            _lights.Add(light);
        }

        public void RemoveLight(SpectralXLight light)
        {
            _lights.Remove(light);
        }

        public void Clear()
        {
            _meshes.Clear();
            _lights.Clear();
        }
    }
}