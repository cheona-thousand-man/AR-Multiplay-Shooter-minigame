using System;
using Unity.Netcode;
using UnityEngine;

public struct MonsterData : IEquatable<MonsterData>, INetworkSerializable
{
    public ulong monsterID;
    public float lifePoints;
    public bool monsterPlaced;

    public MonsterData(ulong monsterID, float lifePoints, bool monsterPlaced)
    {
        this.monsterID = monsterID;
        this.lifePoints = lifePoints;
        this.monsterPlaced = monsterPlaced;
    }

    public bool Equals(MonsterData other)
    {
        return (
            other.monsterPlaced == monsterPlaced &&
            other.lifePoints == lifePoints &&
            other.monsterID == monsterID
        );
    }

    // INetworkSerializable 인터페이스 구현
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref monsterID);
        serializer.SerializeValue(ref lifePoints);
        serializer.SerializeValue(ref monsterPlaced);
    }
}