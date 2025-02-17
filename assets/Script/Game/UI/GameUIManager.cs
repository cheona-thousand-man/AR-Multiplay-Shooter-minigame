using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameUIManager : NetworkBehaviour
{
    [SerializeField] private Canvas CreateGameCanvas;
    [SerializeField] private Canvas ControllerCanvas;
    [SerializeField] private Canvas RestartQuitCanvas;

    private void Start()
    {
        ShowCreateGameCanvas();
        
        AllPlayerDataManager_adv.Instance.OnHitPlayerDead += InstanceOnOnPlayerDead;
        RestartGame.OnRestartGame += RestartGameOnOnRestartGame;
    }

    public override void OnNetworkDespawn()
    {
        AllPlayerDataManager_adv.Instance.OnHitPlayerDead -= InstanceOnOnPlayerDead;
        RestartGame.OnRestartGame -= RestartGameOnOnRestartGame;
    }

    private void RestartGameOnOnRestartGame()
    {
        ShowPlayerControlsServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void ShowPlayerControlsServerRpc()
    {
        ShowClientControlsClientRpc();
    }

    [ClientRpc]
    void ShowClientControlsClientRpc()
    {
        CreateGameCanvas.gameObject.SetActive(false);
        ControllerCanvas.gameObject.SetActive(true);
        RestartQuitCanvas.gameObject.SetActive(false);
    }

    private void InstanceOnOnPlayerDead(ulong obj)
    {
        if (IsServer)
        {
            PlayerIsDeadClientRpc();
        }
    }

    [ClientRpc]
    void PlayerIsDeadClientRpc()
    {
        CreateGameCanvas.gameObject.SetActive(false);
        ControllerCanvas.gameObject.SetActive(false);
        RestartQuitCanvas.gameObject.SetActive(true);
    }

    void ShowCreateGameCanvas()
    {
        CreateGameCanvas.gameObject.SetActive(true);
        ControllerCanvas.gameObject.SetActive(false);
        RestartQuitCanvas.gameObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        CreateGameCanvas.gameObject.SetActive(false);
        ControllerCanvas.gameObject.SetActive(true);
        RestartQuitCanvas.gameObject.SetActive(false);
    }
}
