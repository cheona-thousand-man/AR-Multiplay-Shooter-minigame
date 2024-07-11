using System;
using Unity.Netcode;
using UnityEngine;

public class MonsterDataManager : NetworkBehaviour
{
    public static MonsterDataManager Instance;
    private NetworkList<MonsterData> allMonsterData;
    private const int LIFEPOINTS = 50;
    private const int LIFEPOINTS_TO_REDUCE = 1;
    public event Action<ulong> OnHitMonsterDead;
    public event Action<ulong> OnMonsterHealthChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPlacedMonster(ulong id)
    {
        MonsterData newMonsterData = new MonsterData(id, 100f, true);
        allMonsterData.Add(newMonsterData);
    }

    private void Start()
    {
        if (IsServer)
        {
            BulletData_adv.OnHitMonster += BulletDataOnHitMonster;
        }
    }    

    public void OnDisable()
    {
        if (IsServer)
        {
            allMonsterData.Clear();
            BulletData_adv.OnHitMonster -= BulletDataOnHitMonster;
        }
    }

    public float GetMonsterHealth(ulong id)
    {
        for (int i = 0; i < allMonsterData.Count; i++)
        {
            if (allMonsterData[i].monsterID == id)
            {
                return allMonsterData[i].lifePoints;
            }
        }

        return default;
    }

    private void BulletDataOnHitMonster((ulong shooter, ulong monster) ids)
    {
        if (IsServer)
        {
            for (int i = 0; i < allMonsterData.Count; i++)
            {
                if (allMonsterData[i].monsterID == ids.monster)
                {
                    int lifePointsToReduce = allMonsterData[i].lifePoints == 0 ? 0 : LIFEPOINTS_TO_REDUCE;

                    // 몬스터 HP 처리
                    MonsterData newData = new MonsterData(
                        allMonsterData[i].monsterID,
                        allMonsterData[i].lifePoints - lifePointsToReduce,
                        allMonsterData[i].monsterPlaced
                    );

                    OnMonsterHealthChanged?.Invoke(ids.monster);

                    // 플레이어 score 처리 event 발생

                    if (newData.lifePoints <= 0)
                    {
                        OnHitMonsterDead?.Invoke(ids.monster);
                    }

                    Debug.Log($"Player got hit {ids.monster} Lifepoints left => {newData.lifePoints} shot by {ids.shooter}");

                    allMonsterData[i] = newData;
                    break;
                }
            }
        }
        SyncReducedMonsterHealthClientRpc(ids.monster);
    }

    [ClientRpc]
    void SyncReducedMonsterHealthClientRpc(ulong monster)
    {
        OnMonsterHealthChanged?.Invoke(monster);
    }
}
