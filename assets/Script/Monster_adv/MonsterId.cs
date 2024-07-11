using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MonsterId : NetworkBehaviour
{
    public ulong monsterId { get; private set; }

    public void Initialize(ulong id)
    {
        this.monsterId = id;
    }
}
