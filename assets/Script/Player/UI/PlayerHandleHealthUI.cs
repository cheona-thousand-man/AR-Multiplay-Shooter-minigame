using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerHandleHealthUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text HealthText;

    private Camera _mainCamera;

    public override void OnNetworkSpawn()
    {
        _mainCamera = GameObject.FindObjectOfType<Camera>();
        AllPlayerDataManager.Instance.OnPlayerHealthChanged += InstanceOnOnPlayerHealthChangedServerRpc;
        InstanceOnOnPlayerHealthChangedServerRpc(GetComponentInParent<NetworkObject>().OwnerClientId);
    }

    public override void OnNetworkDespawn()
    {
        AllPlayerDataManager.Instance.OnPlayerHealthChanged -= InstanceOnOnPlayerHealthChangedServerRpc;
    }


    // 실제로 Health가 변경된 플레이어 정보만 업데이트 되면 되므로, 서버에서 해당 플레이어가 피격자(hit.to)인 플레이어만 체크
    [ServerRpc(RequireOwnership = false)]
    private void InstanceOnOnPlayerHealthChangedServerRpc(ulong id)
    {
        if (GetComponentInParent<NetworkObject>().OwnerClientId == id)
        {
            SetHealthTextClientRpc(id);
        }
    }

    private void Update()
    {
        if (_mainCamera)
        {
            HealthText.transform.LookAt(_mainCamera.transform);
        }
    }

    // 피격자(hit.to)로 체크된 플레이어 정보만, 모든 플레이어가 Health UI업데이트 실행
    [ClientRpc]
    private void SetHealthTextClientRpc(ulong id)
    {
        HealthText.text = AllPlayerDataManager.Instance.GetPlayerHealth(id).ToString();
    }
}
