using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RestartGame : NetworkBehaviour
{
    [SerializeField] private Button restartButton;

    public static event Action OnRestartGame;

    void Start()
    {
        restartButton.onClick.AddListener(() => RestartGameServerRpc());
    }

    [ServerRpc(RequireOwnership = false)]
    void RestartGameServerRpc()
    {
        RestartGameClientRpc();
    }

    [ClientRpc]
    void RestartGameClientRpc()
    {
        OnRestartGame?.Invoke();
    }
}
