using System.IO.Compression;
using System.Text;

namespace ListingManagementTestApp;

/// <summary>Builds a small striped PNG in memory, so the upload test needs no image file or imaging library.</summary>
internal static class TestImage
{
    public static byte[] Png(int width, int height)
    {
        using var output = new MemoryStream();
        output.Write([0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A]);

        var header = new byte[13];
        WriteBigEndian(header, 0, width);
        WriteBigEndian(header, 4, height);
        header[8] = 8;  // bit depth
        header[9] = 2;  // RGB
        Chunk(output, "IHDR", header);

        using var pixels = new MemoryStream();
        using (var zlib = new ZLibStream(pixels, CompressionLevel.Optimal, leaveOpen: true))
        {
            var row = new byte[1 + width * 3];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var stripe = (x / 100 + y / 100) % 2 == 0;
                    row[1 + x * 3] = stripe ? (byte)0x00 : (byte)0xE5;      // eBay-ish blue / red stripes
                    row[2 + x * 3] = stripe ? (byte)0x64 : (byte)0x32;
                    row[3 + x * 3] = stripe ? (byte)0xD2 : (byte)0x38;
                }
                zlib.Write(row);
            }
        }
        Chunk(output, "IDAT", pixels.ToArray());
        Chunk(output, "IEND", []);
        return output.ToArray();
    }

    private static void Chunk(Stream output, string type, byte[] data)
    {
        var length = new byte[4];
        WriteBigEndian(length, 0, data.Length);
        output.Write(length);
        var typeAndData = Encoding.ASCII.GetBytes(type).Concat(data).ToArray();
        output.Write(typeAndData);
        var crc = new byte[4];
        WriteBigEndian(crc, 0, (int)Crc32(typeAndData));
        output.Write(crc);
    }

    private static void WriteBigEndian(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value;
    }

    private static uint Crc32(byte[] data)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var b in data)
        {
            crc ^= b;
            for (var i = 0; i < 8; i++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
            }
        }
        return ~crc;
    }
}
