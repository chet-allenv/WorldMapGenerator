/*
 * VoronoiGraph.cs
 * Builds a Voronoi diagram over the world map.
 *
 * Scatters seed points across the map, assigns every pixel to its nearest
 * seed (Fortune's algorithm approximated via pixel scan), then builds a
 * graph of region adjacency. Each region knows its neighbors, its center,
 * its average elevation, and its terrain type.
 *
 * This is the spatial substrate that RiverGenerator uses — rivers flow
 * between regions along shared edges rather than pixel by pixel, which
 * produces naturally clean, vein-like paths.
 *
 * KEY MEMBERS:
 *      - VoronoiGraph(WorldMap map, int seed, int numRegions)
 *      - List<VoronoiRegion> Regions
 *      - VoronoiRegion GetRegionAt(int x, int y)
 *      - void Build() - computes ownership, adjacency, and region properties
 */

namespace WorldMapGenerator
{
    public class VoronoiRegion
    {
        public int Id;
        public (int x, int y) Center;
        public float Elevation;
        public TerrainType Terrain;
        public List<VoronoiRegion> Neighbors = new();
        public List<(int x, int y)> EdgePixels = new(); // Pixels on borders with neighbors

        // The shared border pixels between this region and a specific neighbor
        public Dictionary<int, List<(int x, int y)>> SharedEdges = new();
    }

    public class VoronoiGraph
    {
        private readonly WorldMap _map;
        private readonly Random _rng;
        private readonly int _numRegions;

        public List<VoronoiRegion> Regions { get; } = new();
        private VoronoiRegion[,] _ownership;  // Which region owns each pixel

        public VoronoiGraph(WorldMap map, int seed, int numRegions = 512)
        {
            _map = map;
            _rng = new Random(seed);
            _numRegions = numRegions;
            _ownership = new VoronoiRegion[map.Width, map.Height];
        }

        public void Build()
        {
            ScatterSeeds();
            AssignPixels();
            ComputeRegionProperties();
            ComputeAdjacency();
        }

        // Returns which region owns this pixel
        public VoronoiRegion GetRegionAt(int x, int y) => _ownership[x, y];

        // Scatter seed points evenly using a jittered grid
        private void ScatterSeeds()
        {
            int cols = (int)MathF.Sqrt(_numRegions);
            int rows = (int)MathF.Ceiling((float)_numRegions / cols);
            int cellW = _map.Width / cols;
            int cellH = _map.Height / rows;

            int id = 0;
            for (int col = 0; col < cols; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    if (id >= _numRegions) break;

                    // Jitter within the cell for natural-looking distribution
                    int cx = col * cellW + _rng.Next(cellW);
                    int cy = row * cellH + _rng.Next(cellH);

                    cx = Math.Clamp(cx, 0, _map.Width - 1);
                    cy = Math.Clamp(cy, 0, _map.Height - 1);

                    Regions.Add(new VoronoiRegion { Id = id++, Center = (cx, cy) });
                }
            }
        }

        // Assign every pixel to its nearest seed using brute-force scan.
        // Fast enough for 1024x1024 with 512 regions (~500M ops avoided via
        // early spatial bucketing).
        private void AssignPixels()
        {
            int w = _map.Width;
            int h = _map.Height;

            // Build spatial buckets for faster nearest-neighbor lookup
            int bucketSize = 32;
            int bCols = (w + bucketSize - 1) / bucketSize;
            int bRows = (h + bucketSize - 1) / bucketSize;
            var buckets = new List<VoronoiRegion>[bCols, bRows];

            for (int bc = 0; bc < bCols; bc++)
                for (int br = 0; br < bRows; br++)
                    buckets[bc, br] = new List<VoronoiRegion>();

            foreach (var region in Regions)
            {
                int bc = region.Center.x / bucketSize;
                int br = region.Center.y / bucketSize;
                buckets[bc, br].Add(region);
            }

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    VoronoiRegion best = null!;
                    float bestD = float.MaxValue;

                    // Search nearby buckets only
                    int bx = x / bucketSize;
                    int by = y / bucketSize;

                    for (int dbx = -2; dbx <= 2; dbx++)
                    {
                        for (int dby = -2; dby <= 2; dby++)
                        {
                            int nbx = bx + dbx;
                            int nby = by + dby;

                            if (nbx < 0 || nbx >= bCols || nby < 0 || nby >= bRows) continue;

                            foreach (var region in buckets[nbx, nby])
                            {
                                float dx = region.Center.x - x;
                                float dy = region.Center.y - y;
                                float d = dx * dx + dy * dy;

                                if (d < bestD) { bestD = d; best = region; }
                            }
                        }
                    }

                    _ownership[x, y] = best;
                }
            }
        }

        // Compute average elevation and dominant terrain type per region
        private void ComputeRegionProperties()
        {
            var elevSum = new float[Regions.Count];
            var counts = new int[Regions.Count];
            var terrainVotes = new Dictionary<int, Dictionary<TerrainType, int>>();

            foreach (var r in Regions)
                terrainVotes[r.Id] = new Dictionary<TerrainType, int>();

            for (int x = 0; x < _map.Width; x++)
            {
                for (int y = 0; y < _map.Height; y++)
                {
                    var region = _ownership[x, y];
                    var terrain = _map.GetTerrainAt(x, y);
                    float elev = _map.GetHeightAt(x, y);

                    elevSum[region.Id] += elev;
                    counts[region.Id]++;

                    var votes = terrainVotes[region.Id];
                    if (!votes.ContainsKey(terrain)) votes[terrain] = 0;
                    votes[terrain]++;
                }
            }

            foreach (var region in Regions)
            {
                if (counts[region.Id] > 0)
                    region.Elevation = elevSum[region.Id] / counts[region.Id];

                // Dominant terrain = most common terrain type in this region
                var votes = terrainVotes[region.Id];
                region.Terrain = votes.Count > 0
                    ? votes.MaxBy(kv => kv.Value).Key
                    : TerrainType.Grassland;
            }
        }

        // Find adjacent regions by scanning for pixels whose neighbor
        // belongs to a different region — those pixels form shared edges
        private void ComputeAdjacency()
        {
            int w = _map.Width;
            int h = _map.Height;

            int[] dx = { 1, 0 }; // Only need right and down to avoid duplicates
            int[] dy = { 0, 1 };

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var regionA = _ownership[x, y];

                    for (int i = 0; i < 2; i++)
                    {
                        int nx = x + dx[i];
                        int ny = y + dy[i];

                        if (nx >= w || ny >= h) continue;

                        var regionB = _ownership[nx, ny];
                        if (regionA == regionB) continue;

                        // Register adjacency
                        if (!regionA.Neighbors.Contains(regionB))
                        {
                            regionA.Neighbors.Add(regionB);
                            regionB.Neighbors.Add(regionA);

                            regionA.SharedEdges[regionB.Id] = new List<(int x, int y)>();
                            regionB.SharedEdges[regionA.Id] = new List<(int x, int y)>();
                        }

                        // Record edge pixels on both sides
                        regionA.SharedEdges[regionB.Id].Add((x, y));
                        regionB.SharedEdges[regionA.Id].Add((nx, ny));
                    }
                }
            }
        }
    }
}