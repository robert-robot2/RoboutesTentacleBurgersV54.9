

/*




namespace RoboutesTentacleBurgers.SpectralXComponent.SpectralXRender
{
    public class SpectralXCCube:IMesh
    {
        /// <summary>
        /// Unique identifier for this mesh
        /// </summary>
       public string Name { get; set; }

        /// <summary>
        /// List of vertex positions in local space
        /// </summary>
        public List<Vector3> Vertices { get; }

        /// <summary>
        /// Triangle indices (groups of 3)
        /// </summary>
        public List<int> Indices { get; }

        /// <summary>
        /// Optional: UV coordinates for texturing (future)
        /// </summary>
        public List<Vector2> UVs { get; }

        /// <summary>
        /// Optional: Vertex normals for lighting (future)
        /// </summary>
        public List<Vector3> Normals { get; }

        /// <summary>
        /// Number of vertices in this mesh
        /// </summary>
        public int VertexCount { get; }

        /// <summary>
        /// Number of triangles in this mesh
        /// </summary>
        public int TriangleCount { get; }

        /// <summary>
        /// Validate mesh data integrity
        /// </summary>
        public bool IsValid();

        /// <summary>
        /// Create a deep copy of this mesh
        /// </summary>
        public IMesh Clone();

        //add to imesh property
        public int MeshStyle { get; set; }




        public int CubeX{ get; set; }
        public int CubeY { get; set; }

        public int CubeWidth { get; set; }
        public int CubeHeight { get; set; }





        public string MeshStyle =>
         $"position:absolute; left:{CubeX}px; top:{CubeY}px; " +
         $"width:{WarriorWidth}px; height:{WarriorHeight}px; " +
         $"background-image:url('{CubeOutputperay}'); " +
         $"background-position:-{animationFrame * WarriorWidth}px 0px; " +
         $"background-repeat:no-repeat; " +
         $"opacity:0.95; user-select:none; touch-action:manipulation; z-index:{WarriorZIndex};";


        //then in .razor we cal div>@obj.Meshstyle







    }
}


*/
