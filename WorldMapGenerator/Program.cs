/*
 * Program.cs
 * This serves as the entry point for the World Map Generator application. It initializes the application and starts the main form.
 * 
 * The entry point. Wire all classes together and call them in right order. No logic.
 * 
 * KEY FUNCTIONALITY:
 *      - Creates a MapConfig object to hold configuration settings for the map generation process.
 *      - Initializes the NoiseGenerator, WorldMap, TerrainClassifier, ColorPalette, and MapRenderer classes in order and using the MapConfig object.
 *      - Prints the seed and output file path to the console for user reference.
 *      - Handles any top-level error output
 * 
 * KEY MEMBERS:
 *      - config (MapConfig) - an instance of the MapConfig class that holds all the configuration settings for the map generation process.
 *      - noise (NoiseGenerator) - an instance of the NoiseGenerator class that generates noise values based on the configuration settings.
 *      - map (WorldMap) - an instance of the WorldMap class that represents the world map as a 2D array of terrain types and height values.
 *      - classify (TerrainClassifier) - an instance of the TerrainClassifier class that classifies terrain types based on height values and thresholds defined in the MapConfig.
 *      - Generate map here.
 *      - palette (ColorPalette) - an instance of the ColorPalette class that defines the color scheme for each terrain type.
 *      - renderer (MapRenderer) - an instance of the MapRenderer class that renders the world map as an SKBitmap based on the terrain types and color palette, and saves the output to a file.
 *      - bitmap (SKBitmap) - the generated bitmap of the world map that is rendered by the MapRenderer and saved to a file.
 *      - Save file here.
 *      
 *      
 */







/*
 * SkiaSharp help! Saving a photo
using SkiaSharp;

var bitmap = new SKBitmap(512, 512);
bitmap.SetPixel(0, 0, SKColors.Red);

using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\output\output.png");

File.WriteAllBytes(path, data.ToArray());
*/