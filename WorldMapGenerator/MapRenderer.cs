/*
 * MapRenderer.cs
 * Draws the bitmap based on the terrain types and color palette. This is used to render the world map.
 * 
 * Takes a WorldMap and a ColorPalette, and generates an SKBitmap where each pixel's color is determined by the terrain type at that location on the WorldMap.
 * The only class that utilizes SkiaSharp drawing. All other classes are unaware of the rendering details, which allows for better separation of concerns and easier maintenance.
 * 
 * KEY FUNCTIONALITY:
 *      - Constructor accepts a MapConfig object to access map dimensions and output file path.
 *      - Primary method Render(WorldMap map, ColorPalette palette) that generates an SKBitmap based on the terrain types in the WorldMap and the corresponding colors from the ColorPalette.
 *      - Loops over every pixel, asks palette for a color, and paints the pixel
 *      - Separate method handles saving the SKBitmap to a file using the output file path from MapConfig.
 * 
 * KEY MEMBERS:
 *      - MapRenderer(MapConfig config) - constructor that takes in a MapConfig object to initialize the renderer with the map dimensions and output file path.
 *      - SKBitmap Render(WorldMap map, ColorPalette palette) - method that takes a WorldMap and a ColorPalette, and generates an SKBitmap where each pixel's color is determined by the terrain type at that location on the WorldMap using the ColorPalette.
 *      - void SaveBitmap(SKBitmap bitmap) - method that saves the generated SKBitmap to a file using the output file path specified in the MapConfig.
 *          // PHASE 3 METHOD - TO DO LATER
 *      - void ApplyElevationShading(SKBitmap bitmap, WorldMap map) - method that applies elevation-based shading to the SKBitmap to add depth and visual interest to the map. 
 *      This method would analyze the height values from the WorldMap and adjust the brightness of each pixel accordingly to create a shaded effect that enhances the perception of terrain elevation.    
 *      - void DrawRivers(SKBitmap bitmap, List<List<(int x, int y)>> rivers) - method that draws river paths on the SKBitmap based on the river data generated in the WorldMap. 
 *      This would involve iterating through the river coordinates and coloring those pixels with a specific river color from the ColorPalette to visually represent rivers on the map.
 */
using SkiaSharp;

namespace WorldMapGenerator
{
    public class MapRenderer
    {
        private readonly MapConfig _config;
        private readonly TerrainClassifier _classifier;
        
        private readonly float _shadingIntensity = 0.85f; // Adjust this value to increase or decrease the intensity of the elevation shading effect.
        private readonly float _lightIntensity = 1.15f; // Adjust this value to increase or decrease the intensity of the light source for elevation shading.

        public MapRenderer(MapConfig config)
        {
            _config = config;
            _classifier = new TerrainClassifier(config);
        }

        /*
         * Render Method:
         * Method that takes a WorldMap and a ColorPalette, and generates an SKBitmap where each pixel's color is determined by the terrain type at that location on the WorldMap using the ColorPalette.
         */
        public SKBitmap Render(WorldMap map, ColorPalette palette)
        {
            var _bitmap = new SKBitmap(map.Width, map.Height);

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    var terrainType = map.GetTerrainAt(x, y);
                    var color = palette.GetColor(terrainType);
                    _bitmap.SetPixel(x, y, color);
                }
            }
            return _bitmap;
        }

        /*
         * ApplyElevationShading Method:
         * Method that applies elevation-based shading to the SKBitmap to add depth and visual interest to the map.
         * 
         */
        public void ApplyElevationShading(SKBitmap bitmap, WorldMap map)
        {
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    if (x - 1 < 0 || y - 1 < 0) continue;
                    if (x - 1 >= map.Width || y - 1 >= map.Height) continue;

                    var heightOfCurrentPixel = map.GetHeightAt(x, y);

                    //if (_classifier.Classify(heightOfCurrentPixel) == TerrainType.DeepOcean 
                    //    || _classifier.Classify(heightOfCurrentPixel) == TerrainType.ShallowWater 
                    //    || _classifier.Classify(heightOfCurrentPixel) == TerrainType.Beach)
                    //{
                    //    continue;
                    //}

                    bool lowerThanNeighbor = heightOfCurrentPixel < map.GetHeightAt(x - 1, y - 1);

                    var pixelColor = bitmap.GetPixel(x, y);
                    var shadedColor = pixelColor;

                    float factor = lowerThanNeighbor ? _shadingIntensity : _lightIntensity;
                    float heightDiff = Math.Abs(heightOfCurrentPixel - map.GetHeightAt(x - 1, y - 1));
                    float strength = Math.Min(heightDiff * 200f, 1f); // scale to [0,1]

                    shadedColor = new SKColor(
                        (byte)Math.Clamp((int)(pixelColor.Red * (1f + (factor - 1f) * strength)), 0, 255),
                        (byte)Math.Clamp((int)(pixelColor.Green * (1f + (factor - 1f) * strength)), 0, 255),
                        (byte)Math.Clamp((int)(pixelColor.Blue * (1f + (factor - 1f) * strength)), 0, 255)
                    );

                    bitmap.SetPixel(x, y, shadedColor);
                }
            }
        }

        /*
         * River Drawing Method (Stretch Goal):
         * Draws river paths on the SKBitmap based on the river data generated in the WorldMap.
         */
        public void DrawRivers(SKBitmap bitmap, List<List<(int x, int y)>> rivers, ColorPalette palette)
        {
            foreach (var river in rivers)
            {
                foreach (var (x, y) in river)
                {
                    bitmap.SetPixel(x, y, palette.GetColor(TerrainType.ShallowWater));
                }
            }
        }
    }
}