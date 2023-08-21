using System.Reflection;

namespace Lesniak.Redis;

public enum RedisDataType
{
    [ByteRepresentation('*')]
    Array,
    [ByteRepresentation('$')]
    BulkString,
}


[AttributeUsage(AttributeTargets.Field)]
sealed class ByteRepresentationAttribute : Attribute
{
    public byte Byte { get; }

    public ByteRepresentationAttribute(char b)
    {
        Byte = (byte)b;
    }
}

public static class DataTypeIdentifier
{
    public static byte Identifier(this RedisDataType type)
    {
        var fieldInfo = type.GetType().GetField(type.ToString())!;
        var attribute =
            ((ByteRepresentationAttribute)fieldInfo.GetCustomAttribute(typeof(ByteRepresentationAttribute))!);
        return attribute.Byte;
    }
}

