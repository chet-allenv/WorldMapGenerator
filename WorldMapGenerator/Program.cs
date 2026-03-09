







/*
 * SkiaSharp help! Saving a photo
using SkiaSharp;

var bitmap = new SKBitmap(512, 512);
bitmap.SetPixel(0, 0, SKColors.Red);

using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\output\output.png");

File.WriteAllBytes(path, data.ToArray());
*/