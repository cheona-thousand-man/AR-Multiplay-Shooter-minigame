using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletData_adv : NetworkBehaviour
{
    private NetworkVariable<ulong> owner = new(999);
    private NetworkVariable<bool> isActiveSelf = new(true);

    public static event Action<(ulong from, ulong to)> OnHitPlayer, OnHitMonster;


    private const int MAX_FLY_TIME = 3;

    public override void OnNetworkSpawn()
    {
        DeactivateSelfDelay();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetOwnershipServerRpc(ulong id)
    {
        this.owner.Value = id;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetBulletIsAvticeServerRpc(bool isActive)
    {
        if (!GetComponent<NetworkObject>()) return;

        // isActiveSelf.Value = isActive;

        if (isActive == false)
        {
            GetComponent<NetworkObject>().Despawn();
        }
        else
        {
            GetComponent<NetworkObject>().Spawn();
        }
    }

    public void DeactivateSelfDelay()
    {
        StartCoroutine(DeactivateSelfDelayCoroutine());
    }

    IEnumerator DeactivateSelfDelayCoroutine()
    {
        yield return new WaitForSeconds(MAX_FLY_TIME);
        SetBulletIsAvticeServerRpc(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsServer)
        {
            if (collision.transform.TryGetComponent(out NetworkObject networkObject))
            {
                if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    (ulong, ulong) fromShooterToHit = new (owner.Value, networkObject.OwnerClientId);
                    OnHitPlayer?.Invoke(fromShooterToHit);
                    SetBulletIsAvticeServerRpc(false);
                    return;
                }
                else if (collision.gameObject.layer == LayerMask.NameToLayer("Monster"))
                {
                    if (collision.gameObject.TryGetComponent<MonsterId>(out MonsterId monster))
                    {
                    (ulong, ulong) fromShooterToHit = new (owner.Value, monster.monsterId);
                    OnHitMonster?.Invoke(fromShooterToHit);
                    }
                    SetBulletIsAvticeServerRpc(false);
                    return;
                }
            }
            else
            {
                SetBulletIsAvticeServerRpc(false);
            }
        }
    }
}
