using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlaceMonster : NetworkBehaviour
{
    [SerializeField] private GameObject monsterObject;
    private const float MONSTER_SPAWN_TIME = 5f;
    private Vector3 targetSpawnLocation;
    private Quaternion targetSpawnRotation;
    private static ulong monsterIdCounter = 0;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            PlaceCharacter_adv.OnHostSpawned += SetSpawnLocation;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            PlaceCharacter_adv.OnHostSpawned -= SetSpawnLocation;
        }
    }

    private void SetSpawnLocation(Vector3 position, Quaternion rotation)
    {
        targetSpawnLocation = new Vector3(position.x + UnityEngine.Random.Range(-4f, 4f), position.y, position.z + UnityEngine.Random.Range(-4f, 4f));
        targetSpawnRotation = rotation;
        StartCoroutine(SpawnMonster());
    }

    private IEnumerator SpawnMonster()
    {
        yield return new WaitForSeconds(MONSTER_SPAWN_TIME);

        GameObject monsterInstance = Instantiate(monsterObject, targetSpawnLocation, targetSpawnRotation);
        NetworkObject monsterNetworkObject = monsterInstance.GetComponent<NetworkObject>();
        
        // 몬스터 고유id 설정
        MonsterId monsterId = monsterInstance.GetComponent<MonsterId>();
        monsterId.Initialize(monsterIdCounter);
        
        monsterNetworkObject.Spawn(); // 서버에서만 스폰 호출

        yield return new WaitUntil(() => MonsterDataManager.Instance != null);
        MonsterDataManager.Instance.AddPlacedMonster(monsterIdCounter++);
    }

    public static float GetMonsterSpawnTime()
    {
        return MONSTER_SPAWN_TIME;
    }
}
