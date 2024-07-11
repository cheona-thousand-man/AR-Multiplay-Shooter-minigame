using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class KillPlayer : MonoBehaviour
{
    [SerializeField] private Button killPlayerButton;

    public static event Action<ulong> OnKillPlayer;

    void Start()
    {
        killPlayerButton.onClick.AddListener(() =>
        {
            OnKillPlayer?.Invoke(NetworkManager.Singleton.LocalClientId);
        });
    }
}
