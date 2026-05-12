namespace Asp.Net.Core.Learning.CatalogMicroservice.Data
{
    internal static class ProductImageGenerator
    {
        /// <summary>
        /// Generates a 374-byte 10×10 24-bit BMP image filled with the given solid colour
        /// and returns it as a Base64 string. Uses only BCL types — no NuGet packages.
        /// Layout: BITMAPFILEHEADER (14 bytes) + BITMAPINFOHEADER (40 bytes) + pixel data (320 bytes).
        /// </summary>
        internal static string GenerateBase64Bmp(byte r, byte g, byte b)
        {
            using var ms = new System.IO.MemoryStream(374);

            // BITMAPFILEHEADER (14 bytes)
            ms.Write(new byte[] { 0x42, 0x4D });               // bfType = "BM"
            ms.Write(BitConverter.GetBytes(374));               // bfSize
            ms.Write(BitConverter.GetBytes((short)0));          // bfReserved1
            ms.Write(BitConverter.GetBytes((short)0));          // bfReserved2
            ms.Write(BitConverter.GetBytes(54));                // bfOffBits (14 + 40)

            // BITMAPINFOHEADER (40 bytes)
            ms.Write(BitConverter.GetBytes(40));                // biSize
            ms.Write(BitConverter.GetBytes(10));                // biWidth
            ms.Write(BitConverter.GetBytes(10));                // biHeight (positive = bottom-up storage)
            ms.Write(BitConverter.GetBytes((short)1));          // biPlanes
            ms.Write(BitConverter.GetBytes((short)24));         // biBitCount (24-bit RGB)
            ms.Write(BitConverter.GetBytes(0));                 // biCompression (BI_RGB)
            ms.Write(BitConverter.GetBytes(0));                 // biSizeImage (may be 0 for BI_RGB)
            ms.Write(BitConverter.GetBytes(2835));              // biXPelsPerMeter (~72 DPI)
            ms.Write(BitConverter.GetBytes(2835));              // biYPelsPerMeter (~72 DPI)
            ms.Write(BitConverter.GetBytes(0));                 // biClrUsed
            ms.Write(BitConverter.GetBytes(0));                 // biClrImportant

            // Pixel data: 10 rows × 32-byte stride (10 pixels × 3 bytes BGR + 2-byte padding)
            byte[] row = new byte[32]; // zero-initialised; bytes 30-31 remain 0 (padding)
            for (int px = 0; px < 10; px++)
            {
                row[px * 3]     = b; // Blue channel (BMP stores BGR)
                row[px * 3 + 1] = g; // Green channel
                row[px * 3 + 2] = r; // Red channel
            }

            for (int i = 0; i < 10; i++)
                ms.Write(row);

            return Convert.ToBase64String(ms.ToArray());
        }
    }
}
