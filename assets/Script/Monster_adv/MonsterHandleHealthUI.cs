using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MonsterHandleHealthUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text MonsterHealthText;

    private Camera _mainCamera;
    private MonsterId _monsterId;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => GetComponentInParent<MonsterId>() != null);
        _monsterId = GetComponentInParent<MonsterId>();

        yield return new WaitUntil(() => MonsterDataManager.Instance != null);
        MonsterDataManager.Instance.OnMonsterHealthChanged += MonsterHealthChangedServerRpc;
        MonsterHealthChangedServerRpc(_monsterId.monsterId);
    }

    public override void OnNetworkSpawn()
    {
        _mainCamera = GameObject.FindObjectOfType<Camera>();
    }

    [ServerRpc(RequireOwnership = false)]
    private void MonsterHealthChangedServerRpc(ulong id)
    {
        if (_monsterId.monsterId == id)
        {
            SetMonsterHealthTextClientRpc(id);
        }
    }

    [ClientRpc]
    private void SetMonsterHealthTextClientRpc(ulong id)
    {
        MonsterHealthText.text = MonsterDataManager.Instance.GetMonsterHealth(id).ToString();
    }

    public override void OnNetworkDespawn()
    {
        MonsterDataManager.Instance.OnMonsterHealthChanged -= MonsterHealthChangedServerRpc;
    }

    void Update()
    {
        if (_mainCamera)
        {
            MonsterHealthText.transform.LookAt(_mainCamera.transform);
        }
    }
}
