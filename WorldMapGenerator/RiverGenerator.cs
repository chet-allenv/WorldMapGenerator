/*
 * RiverGenerator.cs
 * Generates river tiles on the world map using a noise-based approach.
 *
 * Instead of pathfinding, a separate Perlin noise field is evaluated across
 * the map. Wherever that noise value is near zero, the tile is marked as a
 * river. Zero-crossings of smooth noise naturally form long, curving,
 * branching vein-like shapes — exactly what rivers look like — at zero
 * computational cost compared to pathfinding.
 *
 * This is the same core technique used by Minecraft for river generation.
 *
 * KEY MEMBERS:
 *      - RiverGenerator(WorldMap map, MapConfig config) - constructor
 *      - HashSet<(int x, int y)> GetRiverTiles() - returns all river tile coordinates
 */

namespace WorldMapGenerator
{
	public class RiverGenerator
	{
		private readonly WorldMap _map;
		private readonly MapConfig _config;
		private readonly FastNoiseLite _noise;

		public RiverGenerator(WorldMap map, MapConfig config)
		{
			_map = map;
			_config = config;

			_noise = new FastNoiseLite();
			_noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
			_noise.SetFractalType(FastNoiseLite.FractalType.FBm);
			_noise.SetSeed(config.RiverSeed);
			_noise.SetFrequency(config.RiverFrequency);
			_noise.SetFractalOctaves(2); // Low octaves = smooth, wide curves
		}

		// Returns the set of all tiles that should be drawn as river.
		// A tile is a river if:
		//   1. It is on land (not ocean or beach)
		//   2. The river noise value at that tile is within RiverThreshold of zero
		public HashSet<(int x, int y)> GetRiverTiles()
		{
			var riverTiles = new HashSet<(int x, int y)>();

			for (int x = 0; x < _map.Width; x++)
			{
				for (int y = 0; y < _map.Height; y++)
				{
					// Rivers only appear on land
					var terrain = _map.GetTerrainAt(x, y);
					if (terrain == TerrainType.DeepOcean ||
						terrain == TerrainType.ShallowWater ||
						terrain == TerrainType.Beach)
						continue;

					// The closer the noise value is to zero, the more
					// "river-like" this tile is. Threshold controls river width.
					float riverNoise = _noise.GetNoise(x, y);
					if (MathF.Abs(riverNoise) < _config.RiverThreshold)
						riverTiles.Add((x, y));
				}
			}

			return riverTiles;
		}
	}
}