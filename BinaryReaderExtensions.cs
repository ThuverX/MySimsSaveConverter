using System.Buffers.Binary;
using System.Numerics;
using System.Text;

namespace SaveConverter;

public static class BinaryReaderExtensions
{
    public static string ReadString(this BinaryReader reader, int size)
    {
        var bytes = reader.ReadBytes(size);

        return Encoding.Default.GetString(bytes);
    }

    public static string ReadCString(this BinaryReader reader)
    {
        List<byte> bytes = [];

        while (true)
        {
            var b = reader.ReadByte();
            if (b == 0)
                break;
            bytes.Add(b);
        }

        return Encoding.Default.GetString([.. bytes]);
    }

    public static Vector3 ReadVector3(this BinaryReader reader)
    {
        var x = reader.ReadSingle();
        var y = reader.ReadSingle();
        var z = reader.ReadSingle();
        return new Vector3(x, y, z);
    }

    public static Vector4 ReadVector4(this BinaryReader reader)
    {
        var x = reader.ReadSingle();
        var y = reader.ReadSingle();
        var z = reader.ReadSingle();
        var w = reader.ReadSingle();
        return new Vector4(x, y, z, w);
    }

    public static Vector2 ReadVector2(this BinaryReader reader)
    {
        var x = reader.ReadSingle();
        var y = reader.ReadSingle();
        return new Vector2(x, y);
    }

    public static Matrix4x4 ReadMatrix4x4(this BinaryReader reader)
    {
        var m = new Matrix4x4();
        m.M11 = reader.ReadSingle();
        m.M12 = reader.ReadSingle();
        m.M13 = reader.ReadSingle();
        m.M14 = reader.ReadSingle();
        m.M21 = reader.ReadSingle();
        m.M22 = reader.ReadSingle();
        m.M23 = reader.ReadSingle();
        m.M24 = reader.ReadSingle();
        m.M31 = reader.ReadSingle();
        m.M32 = reader.ReadSingle();
        m.M33 = reader.ReadSingle();
        m.M34 = reader.ReadSingle();
        m.M41 = reader.ReadSingle();
        m.M42 = reader.ReadSingle();
        m.M43 = reader.ReadSingle();
        m.M44 = reader.ReadSingle();
        return m;
    }

    public static uint ReadUInt32Be(this BinaryReader reader)
    {
        return BinaryPrimitives.ReadUInt32BigEndian(reader.ReadBytes(4));
    }

    public static int ReadInt32Be(this BinaryReader reader)
    {
        return BinaryPrimitives.ReadInt32BigEndian(reader.ReadBytes(4));
    }
}