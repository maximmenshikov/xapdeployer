using System;
using System.Windows.Media.Imaging;
using ImageTools.Helpers;
using ImageTools;

public static class ImageConverter
{
    public static WriteableBitmap ToBitmap(ImageBase image)
    {
        Guard.NotNull(image, "image");
        var bitmap = new WriteableBitmap(image.Width, image.Height);
        ImageBase temp = image;
        byte[] pixels = temp.GetPixels();
        int[] raster = bitmap.Pixels;
        Buffer.BlockCopy(pixels, 0, raster, 0, pixels.Length);
        for (int i = 0; i < raster.Length; i++)
        {
            int abgr = raster[i];
            int a = (abgr >> 24) & 0xff;
            float m = a / 255f;
            int argb = a << 24 |
                       (int)((abgr & 0xff) * m) << 16 |
                       (int)(((abgr >> 8) & 0xff) * m) << 8 |
                       (int)(((abgr >> 16) & 0xff) * m);
            raster[i] = argb;
        }
        bitmap.Invalidate();
        return bitmap;
    }
}
