using Unity.Netcode;
using System.Collections.Generic;

public struct NetworkSerializableDictionary : INetworkSerializable
{
    public Dictionary<string, string> Dictionary;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Write
        if (serializer.IsWriter)
        {
            serializer.GetFastBufferWriter().WriteValueSafe(Dictionary.Count);
            foreach (var pair in Dictionary)
            {
                serializer.GetFastBufferWriter().WriteValueSafe(pair.Key);
                serializer.GetFastBufferWriter().WriteValueSafe(pair.Value);
            }
        }
        // Read
        else
        {
            Dictionary = new Dictionary<string, string>();
            serializer.GetFastBufferReader().ReadValueSafe(out int count);
            for (int i = 0; i < count; i++)
            {
                serializer.GetFastBufferReader().ReadValueSafe(out string key);
                serializer.GetFastBufferReader().ReadValueSafe(out string value);
                Dictionary.Add(key, value);
            }
        }
    }
}