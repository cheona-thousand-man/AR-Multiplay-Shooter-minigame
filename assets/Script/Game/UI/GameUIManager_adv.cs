using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameUIManager_adv : NetworkBehaviour
{
    [SerializeField] private Canvas CreateGameCanvas;
    [SerializeField] private Canvas ControllerCanvas;
    [SerializeField] private Canvas RestartQuitCanvas;

    private void Start()
    {
        ShowCreateGameCanvas();
        
        // 게임 종료 조건 변경 : 상대 플레이어 사망 → 몬스터 사망
        // AllPlayerDataManager_adv.Instance.OnHitPlayerDead += InstanceOnOnPlayerDead;
        MonsterDataManager.Instance.OnHitMonsterDead += OnMonsterDead;
        RestartGame.OnRestartGame += RestartGameOnOnRestartGame;
    }

    public override void OnNetworkDespawn()
    {
        // AllPlayerDataManager_adv.Instance.OnHitPlayerDead -= InstanceOnOnPlayerDead;
        MonsterDataManager.Instance.OnHitMonsterDead -= OnMonsterDead;
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

    // 상대 플레이어가 죽을 경우 게임 종료되는 조건에서 실행
    private void InstanceOnOnPlayerDead(ulong obj)
    {
        if (IsServer)
        {
            PlayerIsDeadClientRpc();
        }
    }

    // 몬스터가 죽을 경우 게임 종료되는 조건에서 실행
    private void OnMonsterDead(ulong obj)
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
