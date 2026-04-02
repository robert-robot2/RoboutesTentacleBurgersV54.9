using System.Numerics;

namespace SpectralXGLX.SpectralXComponent
{
    // ── Tile Material Indices ────────────────────────────────────────────────
    public enum TileMaterial
    {
        Dirt = 0,
        Rock = 1,
        Grass = 2,
        Snow = 3,
        Water = 4,
        Ice = 5
    }

    // ── Paint Mode ───────────────────────────────────────────────────────────
    public enum TilePaintMode
    {
        PaintMaterial,
        RaiseTopology,
        LowerTopology,
        SmoothTopology,
        FlattenTopology
    }

    // ── Per-Tile Data ────────────────────────────────────────────────────────
    public struct TileData
    {
        // Primary material — which texture this tile shows
        public int MaterialIndex;

        // Secondary material for edge blending
        public int BlendMaterial;

        // Blend weight toward BlendMaterial (0 = pure primary, 1 = pure blend)
        public float BlendWeight;

        // Z height for topology sculpting
        public float Height;

        // Recalculated world-space normal — updated after any topology change
        public Vector3 Normal;

        public TileData()
        {
            MaterialIndex = (int)TileMaterial.Grass;
            BlendMaterial = (int)TileMaterial.Grass;
            BlendWeight = 0f;
            Height = 0f;
            Normal = Vector3.UnitZ; // Z = up in XYZ
        }
    }



    // ── Main TileMap Class ───────────────────────────────────────────────────
    public class SpectralXLandTileMap
    {
        private float[]? _cachedHeights;
        private float[]? _cachedNormals;
        private int[]? _cachedMaterials;
        private float[]? _cachedBlendWeights;
        private int[]? _cachedBlendMats;
        private float[] _smoothSnapshot = new float[TileCount];

        // Dirty region tracking — only send changed rectangle to JS
        private int _dirtyMinX = 0, _dirtyMinY = 0;
        private int _dirtyMaxX = GridSize - 1, _dirtyMaxY = GridSize - 1;
        private bool _fullUploadPending = true; // force full on first frame

        // Shared vertex corner heights — (GridSize+1)² corners, one per grid intersection
        private readonly float[] _vertexHeights = new float[(GridSize + 1) * (GridSize + 1)];
        private readonly float[] _vertexSnapshot = new float[(GridSize + 1) * (GridSize + 1)];

        // ── Constants ───────────────────────────────────────────────────────
        public const int GridSize = 512;
        public const int TileCount = GridSize * GridSize; // 1024
        public const float TileSize = 1.0f; // 1 metre per tile in world space

        // Origin offset — centers the grid at world origin
        // Grid runs from -16 to +16 on both X and Y
        private static readonly float GridOriginX = -(GridSize * TileSize) / 2f;
        private static readonly float GridOriginY = -(GridSize * TileSize) / 2f;

        // ── Tile Data Array ──────────────────────────────────────────────────
        private readonly TileData[] _tiles = new TileData[TileCount];

        // ── Dirty Tracking ───────────────────────────────────────────────────
        private bool _isDirty = true;

        // ── UI State ─────────────────────────────────────────────────────────
        public TileMaterial ActiveMaterial { get; set; } = TileMaterial.Grass;
        public TilePaintMode PaintMode { get; set; } = TilePaintMode.PaintMaterial;
        public int BrushSize { get; set; } = 1; // radius in tiles
        public float TopologyStrength { get; set; } = 0.25f;
        public float FlattenTargetHeight { get; set; } = 0f;
        public float BlendStrength { get; set; } = 0.4f; // 0 = hard edge, 1 = maximum blend zone
        public float BrushWorldX { get; set; } = 0f;
        public float BrushWorldY { get; set; } = 0f;
        public bool IsActive { get; set; } = true;



        // ── Texture Paths — sent to JS for GPU upload ────────────────────────
        public static readonly string[] TexturePaths = new[]
        {
            "/iAssets/DirtTile002.png",
            "/iAssets/RockTile002.png",
            "/iAssets/GrassTile004.png",
            "/iAssets/SnowTile002.png",
            "/iAssets/WaterTile002.png",
            "/iAssets/IceTile002.png",
        };

        // ── Material Display Names ───────────────────────────────────────────
        public static readonly string[] MaterialNames = new[]
        {
            "Dirt", "Rock", "Grass", "Snow", "Water", "Ice"
        };




        // ── Init ─────────────────────────────────────────────────────────────
        public void Init()
        {
            for (int i = 0; i < TileCount; i++)
            {
                _tiles[i] = new TileData
                {
                    MaterialIndex = (int)TileMaterial.Grass,
                    BlendMaterial = (int)TileMaterial.Grass,
                    BlendWeight = 0f,
                    Height = 0f,
                    Normal = Vector3.UnitZ
                };
            }

            Array.Clear(_vertexHeights, 0, _vertexHeights.Length);

            //   RecalculateNormals(); // full pass on init only
            _fullUploadPending = true;

            _isDirty = true;

        }

        // ── Tile Index Helpers ────────────────────────────────────────────────
        public static int TileIndex(int x, int y) => y * GridSize + x;

        public static (int x, int y) TileCoord(int index) =>
            (index % GridSize, index / GridSize);

        public static bool InBounds(int x, int y) =>
            x >= 0 && x < GridSize && y >= 0 && y < GridSize;
        public static int VertexIndex(int x, int y) => y * (GridSize + 1) + x;
        // ── World Position → Tile Coordinate ─────────────────────────────────
        // Call from engine when mouse ray hits Z=0 plane
        public (int tileX, int tileY, bool hit) WorldToTile(float worldX, float worldY)
        {
            float localX = worldX - GridOriginX;
            float localY = worldY - GridOriginY;

            int tileX = (int)MathF.Floor(localX / TileSize);
            int tileY = (int)MathF.Floor(localY / TileSize);

            if (!InBounds(tileX, tileY))
                return (0, 0, false);

            return (tileX, tileY, true);
        }

        // ── Tile World Position ───────────────────────────────────────────────
        public Vector3 TileWorldPosition(int x, int y)
        {
            float wx = GridOriginX + x * TileSize + TileSize * 0.5f;
            float wy = GridOriginY + y * TileSize + TileSize * 0.5f;
            float wz = _tiles[TileIndex(x, y)].Height;
            return new Vector3(wx, wy, wz);
        }

        // ── Paint Stroke — called on mouse drag ──────────────────────────────
        public void Paint(int centerX, int centerY)
        {
            if (!IsActive) return; 
            switch (PaintMode)
            {
                case TilePaintMode.PaintMaterial:
                    PaintMaterial(centerX, centerY);
                    break;
                case TilePaintMode.RaiseTopology:
                    AdjustHeight(centerX, centerY, +TopologyStrength);
                    break;
                case TilePaintMode.LowerTopology:
                    AdjustHeight(centerX, centerY, -TopologyStrength);
                    break;
                case TilePaintMode.SmoothTopology:
                    SmoothHeight(centerX, centerY);
                    break;
                case TilePaintMode.FlattenTopology:
                    FlattenHeight(centerX, centerY);
                    break;
            }

            RecalculateNormalsScoped(centerX, centerY);
            MarkDirtyRegion(centerX, centerY, BrushSize);
        }

        // ── Material Painting ─────────────────────────────────────────────────
        private void PaintMaterial(int cx, int cy)
        {
            ForEachInBrush(cx, cy, (x, y, falloff) =>
            {
                int idx = TileIndex(x, y);
                var tile = _tiles[idx];

                int newMat = (int)ActiveMaterial;

                if (tile.MaterialIndex == newMat)
                {
                    // Already this material — strengthen and clear blend
                    tile.BlendWeight = Math.Max(0f, tile.BlendWeight - falloff * 0.5f);
                    tile.BlendMaterial = newMat;
                }
                else
                {
                    // Different material — blend toward new material
                    float newWeight = tile.BlendWeight + falloff * BlendStrength;

                    if (newWeight >= 1.0f)
                    {
                        // Fully converted — swap primary
                        tile.MaterialIndex = newMat;
                        tile.BlendMaterial = newMat;
                        tile.BlendWeight = 0f;
                    }
                    else
                    {
                        // Partial blend — store old as secondary
                        tile.BlendMaterial = newMat;
                        tile.BlendWeight = newWeight;
                    }
                }

                _tiles[idx] = tile;
            });
        }

        // ── Height Adjustment ─────────────────────────────────────────────────
        private void AdjustHeight(int cx, int cy, float delta)
        {
            ForEachInBrush(cx, cy, (x, y, falloff) =>
            {
                // Write to all 4 corners of this tile
                int[] vx = { x, x + 1, x, x + 1 };
                int[] vy = { y, y, y + 1, y + 1 };
                for (int c = 0; c < 4; c++)
                {
                    int vi = VertexIndex(vx[c], vy[c]);
                    _vertexHeights[vi] = Math.Clamp(
                        _vertexHeights[vi] + delta * falloff, -5f, 10f);
                }
                SyncTileHeight(x, y);
            });
        }

        // ── Smooth Height ─────────────────────────────────────────────────────
        private void SmoothHeight(int cx, int cy)
        {
            // Snapshot vertex heights before smoothing
            Array.Copy(_vertexHeights, _vertexSnapshot, _vertexHeights.Length);

            ForEachInBrush(cx, cy, (x, y, falloff) =>
            {
                int[] vx = { x, x + 1, x, x + 1 };
                int[] vy = { y, y, y + 1, y + 1 };
                for (int c = 0; c < 4; c++)
                {
                    int pvx = vx[c], pvy = vy[c];
                    float sum = _vertexSnapshot[VertexIndex(pvx, pvy)];
                    int count = 1;
                    for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            int nx = pvx + dx, ny = pvy + dy;
                            if (nx < 0 || nx > GridSize || ny < 0 || ny > GridSize) continue;
                            sum += _vertexSnapshot[VertexIndex(nx, ny)];
                            count++;
                        }
                    float smoothed = sum / count;
                    int vi = VertexIndex(pvx, pvy);
                    _vertexHeights[vi] = Lerp(_vertexHeights[vi], smoothed, falloff * TopologyStrength);
                }
                SyncTileHeight(x, y);
            });
        }

        // ── Flatten Height ────────────────────────────────────────────────────
        private void FlattenHeight(int cx, int cy)
        {
            ForEachInBrush(cx, cy, (x, y, falloff) =>
            {
                int[] vx = { x, x + 1, x, x + 1 };
                int[] vy = { y, y, y + 1, y + 1 };
                for (int c = 0; c < 4; c++)
                {
                    int vi = VertexIndex(vx[c], vy[c]);
                    _vertexHeights[vi] = Lerp(
                        _vertexHeights[vi], FlattenTargetHeight,
                        falloff * TopologyStrength * 2f);
                }
                SyncTileHeight(x, y);
            });
        }

        private void SyncTileHeight(int x, int y)
        {
            int idx = TileIndex(x, y);
            var tile = _tiles[idx];
            tile.Height = (
                _vertexHeights[VertexIndex(x, y)] +
                _vertexHeights[VertexIndex(x + 1, y)] +
                _vertexHeights[VertexIndex(x, y + 1)] +
                _vertexHeights[VertexIndex(x + 1, y + 1)]) * 0.25f;
            _tiles[idx] = tile;
        }

        // ── Normal Recalculation ──────────────────────────────────────────────
        // Uses central difference across neighbours for smooth cross-tile normals
        private void RecalculateNormals()
        {
            for (int y = 0; y < GridSize; y++)
                for (int x = 0; x < GridSize; x++)
                {
                    // Sample heights of cardinal neighbours, clamp at borders
                    float hL = GetHeight(x - 1, y);
                    float hR = GetHeight(x + 1, y);
                    float hD = GetHeight(x, y - 1);
                    float hU = GetHeight(x, y + 1);

                    // Central difference gradient
                    float dX = (hR - hL) / (2f * TileSize);
                    float dY = (hU - hD) / (2f * TileSize);

                    // Normal from gradient — XYZ space, Z = up
                    var normal = Vector3.Normalize(new Vector3(-dX, -dY, 1f));

                    int idx = TileIndex(x, y);
                    var tile = _tiles[idx];
                    tile.Normal = normal;
                    _tiles[idx] = tile;
                }
        }

        private void RecalculateNormalsScoped(int cx, int cy)
        {
            // Only recalculate tiles within brush radius + 1 padding
            // The +1 ensures neighbours used in central difference are also updated
            int radius = BrushSize + 1;

            int minX = Math.Max(0, cx - radius);
            int maxX = Math.Min(GridSize - 1, cx + radius);
            int minY = Math.Max(0, cy - radius);
            int maxY = Math.Min(GridSize - 1, cy + radius);

            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                {
                    float hL = GetHeight(x - 1, y);
                    float hR = GetHeight(x + 1, y);
                    float hD = GetHeight(x, y - 1);
                    float hU = GetHeight(x, y + 1);

                    float dX = (hR - hL) / (2f * TileSize);
                    float dY = (hU - hD) / (2f * TileSize);

                    var normal = Vector3.Normalize(new Vector3(-dX, -dY, 1f));

                    int idx = TileIndex(x, y);
                    var tile = _tiles[idx];
                    tile.Normal = normal;
                    _tiles[idx] = tile;
                }
        }











        private float GetHeight(int x, int y)
        {
            if (!InBounds(x, y)) return 0f;
            return _tiles[TileIndex(x, y)].Height;
        }

        // ── Brush Iterator ────────────────────────────────────────────────────
        // Calls action for every tile within BrushSize radius of center
        // falloff = 1.0 at center, 0.0 at edge — smooth circular falloff
        private void ForEachInBrush(int cx, int cy, Action<int, int, float> action)
        {
            int radius = Math.Max(0, BrushSize);

            for (int dy = -radius; dy <= radius; dy++)
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int x = cx + dx;
                    int y = cy + dy;
                    if (!InBounds(x, y)) continue;

                    // Circular falloff — tiles outside circle radius skipped
                    float dist = MathF.Sqrt(dx * dx + dy * dy);
                    if (dist > radius + 0.5f) continue;

                    // Smooth falloff: 1.0 at center, 0.0 at edge
                    float falloff = radius > 0
                        ? 1f - Math.Clamp(dist / radius, 0f, 1f)
                        : 1f;

                    // Smooth the falloff curve
                    falloff = falloff * falloff * (3f - 2f * falloff);

                    action(x, y, falloff);
                }
        }

        // ── Frame Data Builder ────────────────────────────────────────────────
        // Called from BuildWebGLFrame — only sends data when dirty
        public TileMapFrameData? BuildFrameData(bool forceFullUpload = false)
        {
            if (!_isDirty && !forceFullUpload) return null;
            Console.WriteLine($"[TileMap] BuildFrameData firing — full: {_fullUploadPending}");
            // ... rest unchanged


            _isDirty = false;

            bool sendFull = _fullUploadPending || forceFullUpload;
            _fullUploadPending = false;

            int vertCount = (GridSize + 1) * (GridSize + 1);

            if (_cachedHeights == null)
            {
                _cachedHeights = new float[vertCount];
                _cachedNormals = new float[TileCount * 3];
                _cachedMaterials = new int[TileCount];
                _cachedBlendWeights = new float[TileCount];
                _cachedBlendMats = new int[TileCount];
            }

            if (sendFull)
            {
                // Full upload — init or force
                Array.Copy(_vertexHeights, _cachedHeights, vertCount);

                for (int y = 0; y < GridSize; y++)
                    for (int x = 0; x < GridSize; x++)
                    {
                        int idx = TileIndex(x, y);
                        var tile = _tiles[idx];
                        _cachedNormals[idx * 3] = tile.Normal.X;
                        _cachedNormals[idx * 3 + 1] = tile.Normal.Y;
                        _cachedNormals[idx * 3 + 2] = tile.Normal.Z;
                        _cachedMaterials[idx] = tile.MaterialIndex;
                        _cachedBlendWeights[idx] = tile.BlendWeight;
                        _cachedBlendMats[idx] = tile.BlendMaterial;
                    }

                // Reset dirty region to full after full upload
                _dirtyMinX = GridSize; _dirtyMinY = GridSize;
                _dirtyMaxX = 0; _dirtyMaxY = 0;

                return new TileMapFrameData
                {
                    Heights = _cachedHeights,
                    Normals = Array.Empty<float>(), // JS computes from heights
                    Materials = _cachedMaterials,
                    BlendWeights = _cachedBlendWeights,
                    BlendMaterials = _cachedBlendMats,
                    IsDirty = true,
                    IsFullUpload = true,
                    DirtyX = 0,
                    DirtyY = 0,
                    DirtyW = GridSize,
                    DirtyH = GridSize,
                };
            }
            else
            {
                // Partial upload — only dirty rectangle
                int minX = _dirtyMinX, minY = _dirtyMinY;
                int maxX = _dirtyMaxX, maxY = _dirtyMaxY;
                int w = maxX - minX + 1;
                int h = maxY - minY + 1;

                // Vertex region — one extra border for corner verts
                int vMinX = Math.Max(0, minX);
                int vMinY = Math.Max(0, minY);
                int vMaxX = Math.Min(GridSize, maxX + 1);
                int vMaxY = Math.Min(GridSize, maxY + 1);
                int vW = vMaxX - vMinX + 1;
                int vH = vMaxY - vMinY + 1;

                var partialHeights = new float[vW * vH];
                for (int y = vMinY; y <= vMaxY; y++)
                    for (int x = vMinX; x <= vMaxX; x++)
                        partialHeights[(y - vMinY) * vW + (x - vMinX)] =
                            _vertexHeights[VertexIndex(x, y)];

                var partialNormals = new float[w * h * 3];
                var partialMats = new int[w * h];
                var partialBlend = new float[w * h];
                var partialBlendMat = new int[w * h];

                for (int y = minY; y <= maxY; y++)
                    for (int x = minX; x <= maxX; x++)
                    {
                        int src = TileIndex(x, y);
                        int dst = (y - minY) * w + (x - minX);
                        var tile = _tiles[src];
                        partialNormals[dst * 3] = tile.Normal.X;
                        partialNormals[dst * 3 + 1] = tile.Normal.Y;
                        partialNormals[dst * 3 + 2] = tile.Normal.Z;
                        partialMats[dst] = tile.MaterialIndex;
                        partialBlend[dst] = tile.BlendWeight;
                        partialBlendMat[dst] = tile.BlendMaterial;
                    }

                // Reset dirty region
                _dirtyMinX = GridSize; _dirtyMinY = GridSize;
                _dirtyMaxX = 0; _dirtyMaxY = 0;

                return new TileMapFrameData
                {
                    Heights = partialHeights,
                    Normals = partialNormals,
                    Materials = partialMats,
                    BlendWeights = partialBlend,
                    BlendMaterials = partialBlendMat,
                    IsDirty = true,
                    IsFullUpload = false,
                    DirtyX = minX,
                    DirtyY = minY,
                    DirtyW = w,
                    DirtyH = h,
                };
            }
        }








        // ── Public Tile Read ──────────────────────────────────────────────────
        public TileData GetTile(int x, int y) => _tiles[TileIndex(x, y)];
        public TileData GetTile(int index) => _tiles[index];

        // ── Force Dirty — call after any external change ──────────────────────
        public void MarkDirty() => _isDirty = true;

        public void MarkDirtyRegion(int cx, int cy, int radius)
        {
            int pad = radius + 2; // +2 for normal recalc border
            _dirtyMinX = Math.Max(0, Math.Min(_dirtyMinX, cx - pad));
            _dirtyMinY = Math.Max(0, Math.Min(_dirtyMinY, cy - pad));
            _dirtyMaxX = Math.Min(GridSize - 1, Math.Max(_dirtyMaxX, cx + pad));
            _dirtyMaxY = Math.Min(GridSize - 1, Math.Max(_dirtyMaxY, cy + pad));
            _isDirty = true;
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static float Lerp(float a, float b, float t) =>
            a + (b - a) * Math.Clamp(t, 0f, 1f);

        public LandscapeSaveData ExportSaveData()
        {
            int vertCount = (GridSize + 1) * (GridSize + 1);
            var heights = new float[vertCount];
            Array.Copy(_vertexHeights, heights, vertCount);

            var materials = new int[TileCount];
            var blendWeights = new float[TileCount];
            var blendMats = new int[TileCount];

            for (int i = 0; i < TileCount; i++)
            {
                materials[i] = _tiles[i].MaterialIndex;
                blendWeights[i] = _tiles[i].BlendWeight;
                blendMats[i] = _tiles[i].BlendMaterial;
            }

            return new LandscapeSaveData
            {
                Heights = heights,
                Materials = materials,
                BlendWeights = blendWeights,
                BlendMats = blendMats
            };
        }

        public async Task ImportSaveDataAsync(LandscapeSaveData data)
        {
            if (data == null) return;

            int vertCount = (GridSize + 1) * (GridSize + 1);
            if (data.Heights?.Length == vertCount)
                Array.Copy(data.Heights, _vertexHeights, vertCount);

            if (data.Materials?.Length == TileCount)
            {
                for (int i = 0; i < TileCount; i++)
                {
                    var tile = _tiles[i];
                    tile.MaterialIndex = data.Materials[i];
                    tile.BlendWeight = data.BlendWeights?[i] ?? 0f;
                    tile.BlendMaterial = data.BlendMats?[i] ?? (int)TileMaterial.Grass;

                    var (x, y) = TileCoord(i);
                    tile.Height =
                        (_vertexHeights[VertexIndex(x, y)] +
                         _vertexHeights[VertexIndex(x + 1, y)] +
                         _vertexHeights[VertexIndex(x, y + 1)] +
                         _vertexHeights[VertexIndex(x + 1, y + 1)]) * 0.25f;

                    _tiles[i] = tile;

                    // Yield every 4096 tiles — keeps render thread responsive
                    if (i % 4096 == 0)
                        await Task.Yield();
                }
            }

            // Chunked normal recalculation — yields every 32 rows
            await RecalculateNormalsAsync();
            MarkDirty();
        }

        private async Task RecalculateNormalsAsync()
        {
            const int RowsPerChunk = 32;
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    float hL = GetHeight(x - 1, y);
                    float hR = GetHeight(x + 1, y);
                    float hD = GetHeight(x, y - 1);
                    float hU = GetHeight(x, y + 1);

                    float dX = (hR - hL) / (2f * TileSize);
                    float dY = (hU - hD) / (2f * TileSize);

                    var normal = Vector3.Normalize(new Vector3(-dX, -dY, 1f));
                    int idx = TileIndex(x, y);
                    var tile = _tiles[idx];
                    tile.Normal = normal;
                    _tiles[idx] = tile;
                }

                // Yield every RowsPerChunk rows
                if (y % RowsPerChunk == 0)
                    await Task.Yield();
            }
        }



    }

    public class LandscapeSaveData
    {
        public float[] Heights { get; set; } = Array.Empty<float>();
        public int[] Materials { get; set; } = Array.Empty<int>();
        public float[] BlendWeights { get; set; } = Array.Empty<float>();
        public int[] BlendMats { get; set; } = Array.Empty<int>();
    }

}