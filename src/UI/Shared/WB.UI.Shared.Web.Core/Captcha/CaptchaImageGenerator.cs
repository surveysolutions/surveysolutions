using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace WB.UI.Shared.Web.Captcha
{
    public class CaptchaImageGenerator
    {
        public bool IsFontFound { get; private set; }
        
        string[] fontFamilies = [];
        static readonly SKColor[] colors = { SKColors.Red, SKColors.DarkBlue, SKColors.Chocolate, SKColors.DarkCyan, SKColors.Orange };
        private static readonly SKFontStyle[] fontStyles = { SKFontStyle.Bold, SKFontStyle.Italic, SKFontStyle.Normal };

        public const int ReduceLines = 30;
        public const int ReducePoints = 15;

        readonly Random rnd = new Random();

        public CaptchaImageGenerator()
        {
            var notoSans = "Noto Sans";
            SetFonts(notoSans);
        }

        public void SetFonts(params string[] fontFamilies)
        {
            var installedFamilies = SKFontManager.Default.GetFontFamilies();
            IsFontFound = fontFamilies.All(font => installedFamilies.Contains(font, StringComparer.OrdinalIgnoreCase));
            this.fontFamilies = fontFamilies;
        }
        
        T RandomItemFrom<T>(T[] collection)
        {
            return collection[rnd.Next(0, collection.Length)];
        }

        IEnumerable<(SKPoint a, SKPoint b)> GetRandomPointsAtCircle(double radius, float cx, float cy)
        {
            while (true)
            {
                SKPoint GetPointAtCircle(int deg)
                {
                    double angle = Math.PI * deg / 180.0;

                    float x = (float)(Math.Cos(angle) * radius) + cx;
                    float y = (float)(Math.Sin(angle) * radius) + cy;

                    return new SKPoint(x, y);
                }

                int degree = rnd.Next(0, 360);
                var a = GetPointAtCircle(degree);

                // get degree of opposite side of circle
                var opposite = degree > 180 ? degree - 180 : degree + 180;

                // adding randomness, so that all lines do not intersect in image center
                opposite = (int)(opposite + rnd.Next(-45, 45));
                var b = GetPointAtCircle(opposite);
                yield return (a, b);
            }
        }

        double NextDoubleBetween(double minimum, double maximum)
        {
            return rnd.NextDouble() * (maximum - minimum) + minimum;
        }

        public byte[]? Generate(string code, int width = 300, int height = 70)
        {
            if (!IsFontFound)
                return null;

            using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));

            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.WhiteSmoke);

                DrawCode(canvas, code, width, height);
                DrawRandomPoints(canvas, width, height);
            }

            DetectEdgesAndInvert(bitmap);

            using (var canvas = new SKCanvas(bitmap))
            {
                DrawRandomLines(canvas, width, height);
            }

            QuantizeToWebSafePalette(bitmap);

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 75);
            return data.ToArray();
        }

        private void DrawCode(SKCanvas canvas, string code, int width, int height)
        {
            var disposables = new List<IDisposable>();
            try
            {
                var letters = new List<(string text, SKFont font, SKColor color, float x, float y)>();
                var totalWidth = 0f;
                var totalHeight = 0f;
                float position = 0;

                (float x, float y) center = (width / 2.0f, height / 2.0f);

                foreach (char c in code)
                {
                    // choose random size and font for each letter
                    var size = rnd.Next((int)(center.y / 2.0 * 0.8), (int)(center.y * 1.2));
                    var typeface = SKTypeface.FromFamilyName(RandomItemFrom(fontFamilies), RandomItemFrom(fontStyles));
                    var font = new SKFont(typeface, Math.Max(60, size)) { Edging = SKFontEdging.Antialias };
                    disposables.Add(typeface);
                    disposables.Add(font);

                    var metrics = font.Metrics;
                    var text = c.ToString();

                    // allowing letters to overlap each other a bit; skia draws text by baseline, not by top of the line
                    letters.Add((text, font, RandomItemFrom(colors), position, rnd.Next(-10, 5) - metrics.Ascent));

                    // determine next letter position
                    totalWidth = position + font.MeasureText(text);
                    position = totalWidth + rnd.Next(-3, 5);
                    totalHeight = Math.Max(totalHeight, metrics.Descent - metrics.Ascent);
                }

                var maxSkew = 15;
                var skewX = (float)Math.Tan(rnd.Next(-maxSkew, maxSkew) * Math.PI / 180);
                var skewY = (float)Math.Tan(rnd.Next(-3, 3) * Math.PI / 180);

                canvas.Save();

                // moving captcha code to the center
                canvas.Translate((width - totalWidth) / 2, (height - totalHeight) / 2);

                canvas.Translate(totalWidth / 2, totalHeight / 2);
                canvas.Skew(skewX, skewY);
                canvas.Translate(-totalWidth / 2, -totalHeight / 2);

                foreach (var letter in letters)
                {
                    using var paint = new SKPaint { IsAntialias = true, Color = letter.color };
                    canvas.DrawText(letter.text, letter.x, letter.y, letter.font, paint);
                }

                canvas.Restore();
            }
            finally
            {
                foreach (var disposable in disposables)
                    disposable.Dispose();
            }
        }

        // Laplacian of Gaussian 5x5 kernel
        private static readonly int[,] edgeKernel =
        {
            { 0, 0, -1, 0, 0 },
            { 0, -1, -2, -1, 0 },
            { -1, -2, 16, -2, -1 },
            { 0, -1, -2, -1, 0 },
            { 0, 0, -1, 0, 0 }
        };

        private static void DetectEdgesAndInvert(SKBitmap bitmap)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;
            var stride = bitmap.RowBytes;

            var pixels = bitmap.GetPixelSpan();
            var source = pixels.ToArray();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var offset = y * stride + x * 4;

                    for (var channel = 0; channel < 3; channel++)
                    {
                        var sum = 0;
                        for (var ky = 0; ky < 5; ky++)
                        {
                            var sy = Math.Clamp(y + ky - 2, 0, height - 1);
                            for (var kx = 0; kx < 5; kx++)
                            {
                                var kernelValue = edgeKernel[ky, kx];
                                if (kernelValue == 0)
                                    continue;

                                var sx = Math.Clamp(x + kx - 2, 0, width - 1);
                                sum += kernelValue * source[sy * stride + sx * 4 + channel];
                            }
                        }

                        pixels[offset + channel] = (byte)(255 - Math.Clamp(sum, 0, 255));
                    }

                    pixels[offset + 3] = 255;
                }
            }
        }

        private static void QuantizeToWebSafePalette(SKBitmap bitmap)
        {
            var pixels = bitmap.GetPixelSpan();
            for (var i = 0; i < pixels.Length; i++)
            {
                if (i % 4 == 3)
                    continue;

                pixels[i] = (byte)(Math.Round(pixels[i] / 51.0) * 51);
            }
        }

        private void DrawRandomLines(SKCanvas canvas, int width, int height)
        {
            foreach (var pair in GetRandomPointsAtCircle(width / 2f, width / 2f, height / 2f).Take(width / ReducePoints))
            {
                using var paint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    Color = RandomColorWithAlpha(), // random color with some transparency
                    StrokeWidth = (float)NextDoubleBetween(0.2, 3) // random thickness
                };
                canvas.DrawLine(pair.a, pair.b, paint);
            }
        }

        private void DrawRandomPoints(SKCanvas canvas, int width, int height)
        {
            foreach (var pair in GetRandomPointsAtCircle(width / 2f, width / 2f, height / 2f).Take(width * height / ReduceLines))
            {
                var point = new SKPoint(
                    GetRandomBetween(pair.a.X, pair.b.X),
                    GetRandomBetween(pair.a.Y, pair.b.Y));

                using var paint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    Color = RandomColorWithAlpha(),
                    StrokeWidth = (float)NextDoubleBetween(0.2, 2)
                };
                using var star = CreateStar(point, 3, 0.1f, 0.2f);
                canvas.DrawPath(star, paint);
            }
        }

        private SKColor RandomColorWithAlpha() =>
            RandomItemFrom(colors).WithAlpha((byte)(rnd.NextDouble() * 255));

        private static SKPath CreateStar(SKPoint center, int prongs, float innerRadius, float outerRadius)
        {
            var path = new SKPath();
            var step = Math.PI / prongs;
            for (var i = 0; i < prongs * 2; i++)
            {
                var radius = i % 2 == 0 ? outerRadius : innerRadius;
                var angle = -Math.PI / 2 + i * step;
                var vertex = new SKPoint(
                    center.X + (float)(Math.Cos(angle) * radius),
                    center.Y + (float)(Math.Sin(angle) * radius));

                if (i == 0)
                    path.MoveTo(vertex);
                else
                    path.LineTo(vertex);
            }

            path.Close();
            return path;
        }

        int GetRandomBetween(float a, float b)
        {
            var min = Math.Min(a, b);
            var max = Math.Max(a, b);

            return rnd.Next((int)min, (int)max);
        }
    }
}
