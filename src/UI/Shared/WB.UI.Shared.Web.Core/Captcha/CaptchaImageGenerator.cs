using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Convolution;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using Color = SixLabors.ImageSharp.Color;
using FontStyle = SixLabors.Fonts.FontStyle;
using SystemFonts = SixLabors.Fonts.SystemFonts;

namespace WB.UI.Shared.Web.Captcha
{
    public class CaptchaImageGenerator
    {
        static readonly string[] fontFamilies = { "Noto Sans", "Arial", "Verdana", "Times New Roman" };
        static readonly Color[] colors = { Color.Red, Color.DarkBlue, Color.Chocolate, Color.DarkCyan, Color.Orange };
        private static readonly FontStyle[] fontStyles = {FontStyle.Bold, FontStyle.Italic, FontStyle.Regular};

        public const int ReduceLines = 30;
        public const int ReducePoints = 15;

        readonly Random rnd = new Random();

        T RandomItemFrom<T>(T[] collection)
        {
            return collection[rnd.Next(0, collection.Length)];
        }

        IEnumerable<(PointF a, PointF b)> GetRandomPointsAtCircle(double radius, float cx, float cy)
        {
            while (true)
            {
                PointF GetPointAtCircle(int deg)
                {
                    double angle = Math.PI * deg / 180.0;

                    float x = (float)(Math.Cos(angle) * radius) + cx;
                    float y = (float)(Math.Sin(angle) * radius) + cy;

                    return new PointF(x, y);
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

        public byte[] Generate(string code, int width = 300, int height = 70)
        {
            using var imgText = new Image<Rgba32>(width, height);

            var totalWidth = 0f;
            var totalHeight = 0f;
            var builder = new AffineTransformBuilder();

            (float x, float y) center = (width / 2.0f, height / 2.0f);

            imgText.Mutate(ctx =>
            {
                ctx.BackgroundColor(Color.Transparent);

                float position = 0;
                foreach (char c in code)
                {
                    // choose random size and font for each letter
                    var size = rnd.Next((int)(center.y / 2.0 * 0.8), (int)(center.y * 1.2));
                    var font = SystemFonts.CreateFont(
                        RandomItemFrom(fontFamilies), 
                        Math.Max(60, size), 
                        RandomItemFrom(fontStyles));

                    // allowing letters to overlap each other a bit
                    var location = new PointF(0 + position, rnd.Next(-10, 5));

                    ctx.DrawText(c.ToString(), font, RandomItemFrom(colors), location);

                    // determine next letter position
                    var fontSize = TextMeasurer.Measure(c.ToString(), new RendererOptions(font, location));
                    totalWidth = position + fontSize.Width;
                    position = totalWidth + rnd.Next(-3, 5);
                    totalHeight = Math.Max(totalHeight, fontSize.Height);
                }

                var maxScew = 15;
                ctx.Transform(builder.PrependSkewDegrees(rnd.Next(-maxScew, maxScew), rnd.Next(-3, 3)));
            });

            using var img = new Image<Rgba32>(width, height);

            var backColor = Color.WhiteSmoke;

            img.Mutate(ctx =>
            {
                ctx.BackgroundColor(backColor);

                // moving captcha code to the center
                var location = new Point((int)((width - totalWidth) / 2), (int)((height - totalHeight) / 2));

                // ReSharper disable once AccessToDisposedClosure
                ctx.DrawImage(imgText, location, 1.0f);

                DrawRandomLines(ctx, width, height);

                ctx.ApplyProcessor(new EdgeDetectorProcessor(EdgeDetectorKernel.LaplacianOfGaussian, false));
                ctx.Invert();

                DrawRandomPixels(ctx, width, height);
                ctx.ApplyProcessor(new QuantizeProcessor(new WebSafePaletteQuantizer()));
            });

            using var ms = new MemoryStream();
            img.SaveAsJpeg(ms, new JpegEncoder(){Quality = 75});
            return ms.ToArray();
        }

        private void DrawRandomPixels(IImageProcessingContext ctx, int width, int height)
        {
            foreach (var pair in GetRandomPointsAtCircle(width / 2f, width / 2f, height / 2f).Take(width / ReducePoints))
            {
                ctx.DrawLines(
                    RandomItemFrom(colors).WithAlpha((float)rnd.NextDouble()), // random color with some transparency
                   (float) NextDoubleBetween(0.2, 3), // random thickness
                   pair.a, pair.b);
            }
        }

        private void DrawRandomLines(IImageProcessingContext ctx, int width, int height)
        {
            foreach (var pair in GetRandomPointsAtCircle(width / 2f, width / 2f, height / 2f).Take(width * height / ReduceLines))
            {
                var point = new PointF(
                    GetRandomBetween(pair.a.X, pair.b.X),
                    GetRandomBetween(pair.a.Y, pair.b.Y));
                ctx.DrawLines(
                    RandomItemFrom(colors).WithAlpha((float)rnd.NextDouble()),  // random color with some transparency
                    (float)NextDoubleBetween(0.2, 2), // random thickness
                    point, point);
            }
        }

        int GetRandomBetween(float a, float b)
        {
            var min = Math.Min(a, b);
            var max = Math.Max(a, b);

            return rnd.Next((int)min, (int)max);
        }
    }
}
