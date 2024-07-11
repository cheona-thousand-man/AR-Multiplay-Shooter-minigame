using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class HandleWarningUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text warningText;

    void Start()
    {
        warningText.gameObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            PlaceCharacter_adv.OnHostSpawned += ShowWarningUIHandler;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            PlaceCharacter_adv.OnHostSpawned -= ShowWarningUIHandler;
        }
    }

    private void ShowWarningUIHandler(Vector3 vector, Quaternion quaternion)
    {
        ShowWarningUIClientRpc();
    }

    [ClientRpc]
    private void ShowWarningUIClientRpc()
    {
        StartCoroutine(ShowWarningUI());
    }

    IEnumerator ShowWarningUI()
    {
        warningText.gameObject.SetActive(true);
        float remainTime = PlaceMonster.GetMonsterSpawnTime();
        while(true)
        {
            warningText.text = $"Boss Monster Appear in {remainTime} seconds";
            if (remainTime-- <= 0) 
            {
                warningText.text = $"Boss Monster Appeared!!";
                yield return new WaitForSeconds(2f);
                warningText.gameObject.SetActive(false);
                yield break;
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
