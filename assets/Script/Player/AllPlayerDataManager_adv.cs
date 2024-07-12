using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class AllPlayerDataManager_adv : NetworkBehaviour
{
    // static으로 선언되어서 GameObject 없이 동작 가능
    // BUT 네트워크에서 동작하는 NetworkList와 NetworkObject는 NetworkBehaviour가 부착된 GameObject에 삽입되어야만 동작한다
    public static AllPlayerDataManager_adv Instance; 
    private NetworkList<PlayerData> allPlayerData;
    private const int LIFEPOINTS = 5;
    private const int LIFEPOINTS_TO_REDUCE = 1;

    private bool isDead = false;
    private string deathMessage;

    public event Action<ulong> OnHitPlayerDead;
    public event Action<ulong> OnPlayerHealthChanged;

    private void Awake()
    {
        allPlayerData = new NetworkList<PlayerData>();

        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }

        Instance = this;
    }

        void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += AddNewClientToList;
        BulletData_adv.OnHitPlayer += BulletDataOnOnHitPlayer;
        BulletData_adv.OnHitMonster += ScoreAddOnHitMonster;
        KillPlayer.OnKillPlayer += KillPlayerOnOnKillerPlayer;
        RestartGame.OnRestartGame += RestartGameOnOnRestartGame;
    }

    public void OnDisable()
    {
        if (IsServer)
        {
            allPlayerData.Clear();
            NetworkManager.Singleton.OnClientConnectedCallback -= AddNewClientToList;
        }
        BulletData_adv.OnHitPlayer -= BulletDataOnOnHitPlayer;
        BulletData_adv.OnHitMonster -= ScoreAddOnHitMonster;
        KillPlayer.OnKillPlayer -= KillPlayerOnOnKillerPlayer;
        RestartGame.OnRestartGame -= RestartGameOnOnRestartGame;
    }

    private void ScoreAddOnHitMonster((ulong shooter, ulong monster) ids)
    {
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == ids.shooter)
            {
                PlayerData newData = new PlayerData(
                    allPlayerData[i].clientID,
                    allPlayerData[i].score + 1,
                    allPlayerData[i].lifePoints,
                    true
                );

                allPlayerData[i] = newData;

                Debug.Log($"Player {allPlayerData[i].clientID}'s Score is {allPlayerData[i].score}");
            }
        }
    }

    public void AddPlacedPlayer(ulong id)
    {
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == id)
            {
                PlayerData newData = new PlayerData(
                    allPlayerData[i].clientID,
                    allPlayerData[i].score,
                    allPlayerData[i].lifePoints,
                    true
                );

                allPlayerData[i] = newData;
            }
        }
    }

    public bool GetHasPlayerPlaced(ulong id)
    {
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == id)
            {
                return allPlayerData[i].playerPlaced;
            }
        }
        return false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            AddNewClientToList(NetworkManager.LocalClientId);
        }
    }

     public float GetPlayerHealth(ulong id)
    {
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == id)
            {
                return allPlayerData[i].lifePoints;
            }
        }

        return default;
    }

    void AddNewClientToList(ulong clientID)
    {
        if (!IsServer) return;

        foreach (var playerData in allPlayerData)
        {
            if (playerData.clientID == clientID) return;
        }

        PlayerData newPlayerData = new PlayerData();
        newPlayerData.clientID = clientID;
        newPlayerData.score = 0;
        newPlayerData.lifePoints = LIFEPOINTS;
        newPlayerData.playerPlaced = false;

        if (allPlayerData.Contains(newPlayerData)) return;

        allPlayerData.Add(newPlayerData);
        PrintAllPlayerPlayerList();
    }

    void PrintAllPlayerPlayerList()
    {
        foreach (var playerData in allPlayerData)
        {
            Debug.Log("Player ID => " + playerData.clientID + " hasPlaced " + playerData.playerPlaced + " Called by " + NetworkManager.Singleton.LocalClientId);
        }
    }

    private void BulletDataOnOnHitPlayer((ulong from, ulong to) ids)
    {
        if (IsServer)
        {
            if (ids.from != ids.to)
            {
                for (int i = 0; i < allPlayerData.Count; i++)
                {
                    if (allPlayerData[i].clientID == ids.to)
                    {
                        int lifePointsToReduce = allPlayerData[i].lifePoints == 0 ? 0 : LIFEPOINTS_TO_REDUCE;

                        PlayerData newData = new PlayerData(
                            allPlayerData[i].clientID,
                            allPlayerData[i].score,
                            allPlayerData[i].lifePoints - lifePointsToReduce,
                            allPlayerData[i].playerPlaced
                        );

                        OnPlayerHealthChanged?.Invoke(ids.to);

                        if (newData.lifePoints <= 0)
                        {
                            OnHitPlayerDead?.Invoke(ids.to); // 게임 종료 조건을 몬스터 사망으로 변경
                            DespawnPlayerServerRpc(ids.to); // 왠지 동작 안 함
                        }

                        Debug.Log($"Player got hit {ids.to} Lifepoints left => {newData.lifePoints} shot by {ids.from}");

                        allPlayerData[i] = newData;
                        break;
                    }
                }
            }
        }
        SyncReducePlayerHealthClientRpc(ids.to);
    }

    [ClientRpc]
    void SyncReducePlayerHealthClientRpc(ulong hitID)
    {
        OnPlayerHealthChanged?.Invoke(hitID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnPlayerServerRpc(ulong playerID)
    {
        var player = NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject;
        if (player != null)
        {
            Debug.Log("Player despawn now!");
            player.Despawn();
            NotifyPlayerDeathClientRpc(playerID);
        }
    }

    [ClientRpc]
    void NotifyPlayerDeathClientRpc(ulong playerID)
    {
        if (NetworkManager.Singleton.LocalClientId == playerID)
        {
            isDead = true;
            deathMessage = "You have died.";
        }
    }

    void OnGUI()
    {
        if (isDead)
        {
            GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 25, 400, 100), deathMessage);
        }
    }

    private void RestartGameOnOnRestartGame()
    {
        if (!IsServer) return;

        List<NetworkObject> playerObjects = FindObjectsOfType<PlayerMovement>()
            .Select(x => x.transform.GetComponent<NetworkObject>()).ToList();
        List<NetworkObject> bulletObjects = FindObjectsOfType<BulletData_adv>()
            .Select(x => x.transform.GetComponent<NetworkObject>()).ToList();
        
        foreach (var playerobj in playerObjects)
        {
            playerobj.Despawn();
        }

        foreach (var bulletobj in bulletObjects)
        {
            bulletobj.Despawn();
        }

        ResetNetworkList();
    }

    void ResetNetworkList()
    {
        for(int i = 0; i < allPlayerData.Count; i++)
        {
            PlayerData resetPlayer = new PlayerData(
                allPlayerData[i].clientID,
                playerPlaced: false,
                lifePoints: LIFEPOINTS,
                score: 0
            );

            allPlayerData[i] = resetPlayer;
        }
    }

    // Debug용 기능 구현
    private void KillPlayerOnOnKillerPlayer(ulong id)
    {
        (ulong, ulong) fromTO = new(555, id);
        BulletDataOnOnHitPlayer(fromTO);
    }

}
