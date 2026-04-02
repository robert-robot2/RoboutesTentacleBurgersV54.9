

namespace SpectralXAXA.SpectralXComponent
{
    /// <summary>
    /// Minimal test scene with a single cube
    /// </summary>
    public class SpectralXScene2
    {
        public SpectralXScene Scene { get; private set; }
        public SpectralXMeshLibrary MeshLibrary { get; private set; }

        public SpectralXScene2()
        {
            MeshLibrary = new SpectralXMeshLibrary();
            Scene = new SpectralXScene();

            // Get built-in cube mesh
            var cubeMesh = MeshLibrary.GetMesh("PrimCube");
            if (cubeMesh != null)
            {
                // Spawn cube at origin
                Scene.Spawn(cubeMesh, new Vector3(0, 0, 0), "TestCube");
            }
        }

        public void Update(float deltaTime)
        {
            Scene.Update(deltaTime);
        }

        /// <summary>
        /// Renders the scene as HTML markup for the C# viewport.
        /// Each entity is a div with inline CSS transforms.
        /// </summary>
        public MarkupString Render()
        {
            var sb = new System.Text.StringBuilder();

            foreach (var entity in Scene.GetVisibleEntities())
            {
                var pos = entity.Position;
                var scale = entity.Scale;
                var rot = entity.Rotation;

                // Basic 3D → CSS transform approximation
                var transform = $"translate3d({pos.X * 50}px, {pos.Y * -50}px, {pos.Z * 50}px) " +
                                $"rotateX({rot.X}rad) rotateY({rot.Y}rad) rotateZ({rot.Z}rad) " +
                                $"scale3d({scale.X},{scale.Y},{scale.Z})";

                sb.AppendLine($@"
<div style='
    width:50px;
    height:50px;
    background-color:rgba(100,150,255,0.7);
    position:absolute;
    transform-style:preserve-3d;
    transform:{transform};
    border:1px solid cyan;
    box-sizing:border-box;
'>
    {entity.Name}
</div>
");
            }

            return new MarkupString(sb.ToString());
        }
    }
}
