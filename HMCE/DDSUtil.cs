using Pfim;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageFormat = Pfim.ImageFormat;

namespace HMCE
{
    public static class DDSUtil
    {
        public static BitmapSource ReadDDS(string path)
        {
            if (File.Exists(path))
            {
                using (IImage image = Pfimage.FromFile(path))
                {
                    PixelFormat format;

                    switch (image.Format)
                    {
                        case ImageFormat.Rgba32:
                            format = PixelFormats.Bgra32;
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    GCHandle handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                    try
                    {
                        IntPtr data = handle.AddrOfPinnedObject();
                        BitmapSource bitmap = BitmapSource.Create(image.Width, image.Height, 96, 96, format, null, data, image.DataLen, image.Stride);
                        return bitmap;
                    }
                    finally
                    {
                        handle.Free();
                    }
                }
            }
            else return null;
        }
    }
}
