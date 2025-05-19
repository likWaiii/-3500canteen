using Unity.Collections;
using Unity.Netcode;

public struct NetworkWaitingRecipe : INetworkSerializable
{
    public FixedString64Bytes recipeName;
    public FixedString128Bytes ingredients; // “|”分割列表
    public float remainingTime;
    public bool isCompleted;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref recipeName);
        serializer.SerializeValue(ref ingredients);
        serializer.SerializeValue(ref remainingTime);
        serializer.SerializeValue(ref isCompleted);
    }
}
