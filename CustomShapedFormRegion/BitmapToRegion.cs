using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace CustomShapedFormRegion
{
  internal class BitmapToRegion
  {
    private static int DPI_96 = 96;
    private static int DPI_120 = 120;
    private static Size IMAGE_SIZE_1 = new Size(615, 551);
    private static Size IMAGE_SIZE_2 = new Size(820, 678);
    private static Size IMAGE_SIZE_3 = new Size(925, 840);

    public static Region getRegion(Bitmap inputBmp, Color transperancyKey, int tolerance)
    {
      GraphicsPath path = new GraphicsPath();
      for (int x = 0; x < inputBmp.Width; ++x)
      {
        for (int y = 0; y < inputBmp.Height; ++y)
        {
          if (!BitmapToRegion.colorsMatch(inputBmp.GetPixel(x, y), transperancyKey, tolerance))
            path.AddRectangle(new Rectangle(x, y, 1, 1));
        }
      }
      Region region = new Region(path);
      path.Dispose();
      return region;
    }

    private static bool colorsMatch(Color color1, Color color2, int tolerance)
    {
      if (tolerance < 0)
        tolerance = 0;
      return Math.Abs((int) color1.R - (int) color2.R) <= tolerance && Math.Abs((int) color1.G - (int) color2.G) <= tolerance && Math.Abs((int) color1.B - (int) color2.B) <= tolerance;
    }

    private static unsafe bool colorsMatch(uint* pixelPtr, Color color1, int tolerance)
    {
      if (tolerance < 0)
        tolerance = 0;
      int alpha = (int) (byte) (*pixelPtr >> 24);
      byte num1 = (byte) (*pixelPtr >> 16);
      byte num2 = (byte) (*pixelPtr >> 8);
      byte num3 = (byte) *pixelPtr;
      int red = (int) num1;
      int green = (int) num2;
      int blue = (int) num3;
      Color color = Color.FromArgb(alpha, red, green, blue);
      return Math.Abs((int) color1.A - (int) color.A) <= tolerance && Math.Abs((int) color1.R - (int) color.R) <= tolerance && Math.Abs((int) color1.G - (int) color.G) <= tolerance && Math.Abs((int) color1.B - (int) color.B) <= tolerance;
    }

    public static unsafe Color GetBtnBackImageColor(Bitmap bitmap)
    {
      GraphicsUnit pageUnit = GraphicsUnit.Pixel;
      RectangleF bounds = bitmap.GetBounds(ref pageUnit);
      Rectangle rect = new Rectangle((int) bounds.Left, (int) bounds.Top, (int) bounds.Width, (int) bounds.Height);
      BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
      uint* pointer = (uint*) bitmapData.Scan0.ToPointer();
      for (int index = 0; index < 15; ++index)
        pointer += bitmapData.Stride;
      int num1 = 0;
      while (num1 < 15)
      {
        ++num1;
        ++pointer;
      }
      int alpha = (int) (byte) (*pointer >> 24);
      byte num2 = (byte) (*pointer >> 16);
      byte num3 = (byte) (*pointer >> 8);
      byte num4 = (byte) *pointer;
      int red = (int) num2;
      int green = (int) num3;
      int blue = (int) num4;
      return Color.FromArgb(alpha, red, green, blue);
    }

    public static unsafe Region getRegionFast(
      Size size,
      Bitmap bitmap,
      Color transparencyKey,
      int tolerance)
    {
      GraphicsUnit pageUnit = GraphicsUnit.Pixel;
      RectangleF bounds = bitmap.GetBounds(ref pageUnit);
      Rectangle rect = new Rectangle((int) bounds.Left, (int) bounds.Top, (int) bounds.Width, (int) bounds.Height);
      int height = (int) bounds.Height;
      int width = (int) bounds.Width;
      if (tolerance <= 0)
        tolerance = 1;
      BitmapData bitmapdata = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
      uint* pixelPtr = (uint*) bitmapdata.Scan0.ToPointer();
      GraphicsPath path = new GraphicsPath();
      for (int y = 0; y < height; ++y)
      {
        byte* numPtr = (byte*) pixelPtr;
        int num = 0;
        while (num < width)
        {
          if (BitmapToRegion.colorsMatch(pixelPtr, transparencyKey, tolerance))
          {
            int x = num;
            for (; num < width && BitmapToRegion.colorsMatch(pixelPtr, transparencyKey, tolerance); ++pixelPtr)
              ++num;
            if (y == 15)
            {
              for (int index = 0; index < size.Height - height; ++index)
                path.AddRectangle(new Rectangle(x, y + index, size.Width - x * 2, 1));
              break;
            }
            if (y > 15)
            {
              path.AddRectangle(new Rectangle(x, y + size.Height - height - 1, size.Width - x * 2, 1));
              break;
            }
            path.AddRectangle(new Rectangle(x, y, size.Width - x * 2, 1));
            break;
          }
          ++num;
          ++pixelPtr;
        }
        pixelPtr = (uint*) (numPtr + bitmapdata.Stride);
      }
      Region regionFast = new Region(path);
      path.Dispose();
      bitmap.UnlockBits(bitmapdata);
      return regionFast;
    }

    public static Size GetNewSize(int dpi) => dpi != BitmapToRegion.DPI_96 ? (dpi != BitmapToRegion.DPI_120 ? BitmapToRegion.IMAGE_SIZE_3 : BitmapToRegion.IMAGE_SIZE_2) : BitmapToRegion.IMAGE_SIZE_1;
  }
}
