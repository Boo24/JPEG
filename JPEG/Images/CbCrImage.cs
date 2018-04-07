using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace JPEG.Images
{
    class CbCrImage
    {

        public readonly int Height;
        public readonly int Width;
        public readonly double[,] y;
        public readonly double[,] Cb;
        public readonly double[,] Cr;

        public CbCrImage(int height, int width)
        {
            Height = height;
            Width = width;
            y = new double[height, width];
            Cb = new double[height, width];
            Cr = new double[height, width];
        }

        public static explicit operator CbCrImage(Bitmap processedBitmap)
        {
            var height = processedBitmap.Height - processedBitmap.Height % 8;
            var width = processedBitmap.Width - processedBitmap.Width % 8;
            var cbCrImage = new CbCrImage(height, width);
            unsafe
            {
                var bitmapData = processedBitmap.LockBits(new Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height), ImageLockMode.ReadWrite, processedBitmap.PixelFormat);
                int bytesPerPixel = Bitmap.GetPixelFormatSize(processedBitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height - processedBitmap.Height % 8;
                int widthInBytes =( bitmapData.Width - processedBitmap.Width % 8 )* bytesPerPixel ;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;
                Parallel.For(0, heightInPixels, y =>
                {
                    var c = 0;
                    byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        int oldBlue = currentLine[x];
                        int oldGreen = currentLine[x + 1];
                        int oldRed = currentLine[x + 2];
                        var yc = 16.0 + (65.738 * oldRed + 129.057 * oldGreen + 24.064 * oldBlue) / 256.0;
                        var cb = 128.0 + (-37.945 * oldRed - 74.494 * oldGreen + 112.439 * oldBlue) / 256.0;
                        var cr = 128.0 + (112.439 * oldRed - 94.154 * oldGreen - 18.285 * oldBlue) / 256.0;
                        cbCrImage.y[y, c] = yc;
                        cbCrImage.Cb[y, c] = cb;
                        cbCrImage.Cr[y, c] = cr;
                        c++; 
                        if (c == width)
                            c = 0;
                    }
                });
                processedBitmap.UnlockBits(bitmapData);
            }

            return cbCrImage;
        }

        public static explicit operator Bitmap(CbCrImage cbCrImage)
        {
            var width = cbCrImage.Width - cbCrImage.Width % 8;
            var height = cbCrImage.Height - cbCrImage.Height % 8;
            var bmp = new Bitmap(width, height);
            unsafe
            {
                var bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                var bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
                var heightInPixels = height;
                var widthInBytes = bmpdata.Width * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)bmpdata.Scan0;

                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = ptrFirstPixel + (y * bmpdata.Stride);
                    for (var x = 0; x < width; x++)
                    {
                        var xPor3 = x * 4;
                        var _y = (298.082 * cbCrImage.y[y, x] + 408.583 * cbCrImage.Cr[y, x]) / 256.0 - 222.921;
                        var cb = (298.082 * cbCrImage.y[y, x] - 100.291 * cbCrImage.Cb[y,x] - 208.120 * cbCrImage.Cr[y, x]) / 256.0 + 135.576;
                        var cr =  (298.082 * cbCrImage.y[y, x] + 516.412 * cbCrImage.Cb[y,x]) / 256.0 - 276.836;;
                        currentLine[xPor3] = ToByte(cr);
                        currentLine[xPor3+1] = ToByte(cb);
                        currentLine[xPor3+2] = ToByte(_y);
                        currentLine[xPor3+3] = 255;
                    }
                });
                bmp.UnlockBits(bmpdata);
            }

            return bmp;
        }

//private double YtoRgb(int x, int c) => (298.082 * y[c, x] + 408.583 * Cr[c, x]) / 256.0 - 222.921;

        public static byte ToByte(double d)
        {
            var val = (int) d;
            if (val > byte.MaxValue)
                return byte.MaxValue;
            if (val < byte.MinValue)
                return byte.MinValue;
            return (byte)val;
        }
    }
}