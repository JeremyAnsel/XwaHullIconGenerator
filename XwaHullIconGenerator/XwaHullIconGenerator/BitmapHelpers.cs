using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XwaHullIconGenerator
{
    static class BitmapHelpers
    {
        public static byte[] CombineBuffers(int width, int height, int itemsPerRow, out int rowsCount, out int columnsCount, params byte[][] buffers)
        {
            if (buffers.Length == 0)
            {
                throw new ArgumentNullException(nameof(buffers));
            }

            if (itemsPerRow <= 0)
            {
                itemsPerRow = short.MaxValue;
            }

            rowsCount = (buffers.Length + itemsPerRow - 1) / itemsPerRow;
            columnsCount = Math.Min(buffers.Length, itemsPerRow);

            byte[] buffer2 = new byte[rowsCount * columnsCount * width * height * 4];

            for (int bufferIndex = 0; bufferIndex < buffers.Length; bufferIndex++)
            {
                for (int y = 0; y < height; y++)
                {
                    int destIndex = (bufferIndex / columnsCount * height + y) * columnsCount * width * 4 + (bufferIndex % columnsCount) * width * 4;
                    Array.Copy(buffers[bufferIndex], y * width * 4, buffer2, destIndex, width * 4);
                }
            }

            return buffer2;
        }

        public static void SaveBuffersBitmap(string filename, int width, int height, int itemsPerRow, IEnumerable<byte[]> buffers)
        {
            SaveBuffersBitmap(filename, width, height, itemsPerRow, buffers.ToArray());
        }

        public static void SaveBuffersBitmap(string filename, int width, int height, int itemsPerRow, params byte[][] buffers)
        {
            int rowsCount;
            int columnsCount;
            byte[] buffer = CombineBuffers(width, height, itemsPerRow, out rowsCount, out columnsCount, buffers);

            SaveBitmap(
                filename,
                buffer,
                width * columnsCount, height * rowsCount);
        }

        public static void SaveBitmap(string filename, byte[] buffer, int width, int height)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException(nameof(filename));
            }

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            if (buffer.Length != width * height * 4)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer));
            }

            ImageFormat format;

            switch (Path.GetExtension(filename).ToUpperInvariant())
            {
                case ".BMP":
                    format = ImageFormat.Bmp;
                    break;

                case ".PNG":
                    format = ImageFormat.Png;
                    break;

                case ".JPG":
                    format = ImageFormat.Jpeg;
                    break;

                default:
                    throw new NotSupportedException();
            }

            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                using (var bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, handle.AddrOfPinnedObject()))
                {
                    bitmap.Save(filename, format);
                }
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
