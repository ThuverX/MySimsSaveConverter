using System.Numerics;
using System.Text;

namespace SaveConverter;

public static class BinaryWriterExtensions
{
    public static void WriteCString(this BinaryWriter bin, string text)
    {
        foreach (var by in Encoding.UTF8.GetBytes(text)) bin.Write(by);

        bin.Write('\0');
    }

    public static void WriteString(this BinaryWriter bin, string text)
    {
        foreach (var by in Encoding.UTF8.GetBytes(text)) bin.Write(by);
    }

    public static void WriteLength(this BinaryWriter bin, int length, byte value = 0)
    {
        for (var i = 0; i < length; i++) bin.Write(value);
    }

    public static void WriteVector3(this BinaryWriter bin, Vector3 vec)
    {
        bin.Write(vec.X);
        bin.Write(vec.Y);
        bin.Write(vec.Z);
    }

    public static void WriteVector2(this BinaryWriter bin, Vector2 vec)
    {
        bin.Write(vec.X);
        bin.Write(vec.Y);
    }

    public static void WriteVector4(this BinaryWriter bin, Vector4 vec)
    {
        bin.Write(vec.X);
        bin.Write(vec.Y);
        bin.Write(vec.Z);
        bin.Write(vec.W);
    }

    public static void WriteMatrix4x4(this BinaryWriter bin, Matrix4x4 mat)
    {
        bin.Write(mat.M11);
        bin.Write(mat.M12);
        bin.Write(mat.M13);
        bin.Write(mat.M14);
        bin.Write(mat.M21);
        bin.Write(mat.M22);
        bin.Write(mat.M23);
        bin.Write(mat.M24);
        bin.Write(mat.M31);
        bin.Write(mat.M32);
        bin.Write(mat.M33);
        bin.Write(mat.M34);
        bin.Write(mat.M41);
        bin.Write(mat.M42);
        bin.Write(mat.M43);
        bin.Write(mat.M44);
    }


    public static byte[] ToBytes(this BinaryWriter bin)
    {
        if (bin.BaseStream is MemoryStream ms)
            return ms.ToArray();
        throw new Exception($"Can't get bytes out of a {bin.BaseStream.GetType().Name}");
    }

    public static void ToNearest32Bytes(this BinaryWriter bin)
    {
        var offset = bin.BaseStream.Position;

        bin.WriteLength((int)offset % 32);
    }

    public static HoleLocation<T> Hole<T>(this BinaryWriter bin, int size)
        where T : INumber<T>
    {
        var position = bin.BaseStream.Position;
        bin.WriteLength(size);

        return new HoleLocation<T>
        {
            Position = position,
            Fill = value =>
            {
                var jump = bin.BaseStream.Position;
                bin.BaseStream.Position = position;
                WriteNumber(bin, value);
                bin.BaseStream.Position = jump;
            }
        };
    }

    private static void WriteNumber<T>(this BinaryWriter writer, T value)
        where T : INumber<T>
    {
        if (typeof(T) == typeof(int))
            writer.Write(Convert.ToInt32(value));
        else if (typeof(T) == typeof(uint))
            writer.Write(Convert.ToUInt32(value));
        else if (typeof(T) == typeof(float))
            writer.Write(Convert.ToSingle(value));
        else if (typeof(T) == typeof(double))
            writer.Write(Convert.ToDouble(value));
        else if (typeof(T) == typeof(long))
            writer.Write(Convert.ToInt64(value));
        else if (typeof(T) == typeof(ulong))
            writer.Write(Convert.ToUInt64(value));
        else if (typeof(T) == typeof(short))
            writer.Write(Convert.ToInt16(value));
        else if (typeof(T) == typeof(ushort))
            writer.Write(Convert.ToUInt16(value));
        else if (typeof(T) == typeof(byte))
            writer.Write(Convert.ToByte(value));
        else if (typeof(T) == typeof(sbyte))
            writer.Write(Convert.ToSByte(value));
        else
            throw new NotSupportedException($"Type {typeof(T)} is not supported.");
    }

    public class HoleLocation<T>
        where T : INumber<T>
    {
        public required Action<T> Fill;
        public required long Position;
    }
}