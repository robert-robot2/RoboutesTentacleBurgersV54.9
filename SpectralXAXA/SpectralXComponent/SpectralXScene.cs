namespace SpectralXAXA.SpectralXComponent
{
    /// <summary>
    /// Manages all entities in the 3D scene
    /// Handles spawning, updating, and querying entities
    /// </summary>
    public class SpectralXScene
    {
        private readonly List<SpectralXEntity> entities = new();
        private readonly Dictionary<string, SpectralXEntity> entitiesById = new();

        /// <summary>
        /// All entities in the scene
        /// </summary>
        public IReadOnlyList<SpectralXEntity> Entities => entities;

        /// <summary>
        /// Number of entities in scene
        /// </summary>
        public int EntityCount => entities.Count;

        /// <summary>
        /// Number of active entities
        /// </summary>
        public int ActiveEntityCount => entities.Count(e => e.IsActive);

        /// <summary>
        /// Number of visible entities (for rendering)
        /// </summary>
        public int VisibleEntityCount => entities.Count(e => e.IsActive && e.IsVisible);

        public SpectralXScene()
        {
        }

        /// <summary>
        /// Add entity to scene
        /// </summary>
        public void AddEntity(SpectralXEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entitiesById.ContainsKey(entity.Id))
            {
                Console.WriteLine($"[SpectralXScene] Entity with ID {entity.Id} already exists!");
                return;
            }

            entities.Add(entity);
            entitiesById[entity.Id] = entity;
        }

        /// <summary>
        /// Remove entity from scene
        /// </summary>
        public bool RemoveEntity(SpectralXEntity entity)
        {
            if (entity == null) return false;

            entitiesById.Remove(entity.Id);
            return entities.Remove(entity);
        }

        /// <summary>
        /// Remove entity by ID
        /// </summary>
        public bool RemoveEntity(string id)
        {
            if (entitiesById.TryGetValue(id, out var entity))
            {
                entities.Remove(entity);
                entitiesById.Remove(id);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get entity by ID
        /// </summary>
        public SpectralXEntity? GetEntity(string id)
        {
            entitiesById.TryGetValue(id, out var entity);
            return entity;
        }

        /// <summary>
        /// Get all entities with a specific tag
        /// </summary>
        public List<SpectralXEntity> GetEntitiesByTag(string tag)
        {
            return entities.Where(e => e.Tag == tag).ToList();
        }

        /// <summary>
        /// Clear all entities from scene
        /// </summary>
        public void Clear()
        {
            entities.Clear();
            entitiesById.Clear();
        }

        /// <summary>
        /// Update all active entities
        /// </summary>
        public void Update(float deltaTime)
        {
            foreach (var entity in entities)
            {
                if (entity.IsActive)
                {
                    entity.Update(deltaTime);
                }
            }
        }

        /// <summary>
        /// Get all entities that should be rendered
        /// </summary>
        public IEnumerable<SpectralXEntity> GetVisibleEntities()
        {
            return entities.Where(e => e.IsActive && e.IsVisible);
        }

        /// <summary>
        /// Spawn a new entity from a mesh at a position
        /// </summary>
        public SpectralXEntity Spawn(IMesh mesh, Vector3 position, string? name = null)
        {
            var entity = new SpectralXEntity(mesh, position: position);
            if (!string.IsNullOrEmpty(name))
                entity.Name = name;

            AddEntity(entity);
            return entity;
        }

        /// <summary>
        /// Spawn multiple entities in a grid pattern (useful for testing)
        /// </summary>
        public List<SpectralXEntity> SpawnGrid(IMesh mesh, int countX, int countZ, float spacing)
        {
            var spawned = new List<SpectralXEntity>();

            for (int x = 0; x < countX; x++)
            {
                for (int z = 0; z < countZ; z++)
                {
                    var pos = new Vector3(x * spacing, 0, z * spacing);
                    var entity = Spawn(mesh, pos, $"{mesh.Name}_Grid_{x}_{z}");
                    spawned.Add(entity);
                }
            }

            return spawned;
        }
    }
}