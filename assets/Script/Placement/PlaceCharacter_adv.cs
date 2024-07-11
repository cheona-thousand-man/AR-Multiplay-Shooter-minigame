using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlaceCharacter_adj : NetworkBehaviour
{
    [SerializeField] private GameObject placementObject;
    private Camera mainCam;

    // share spawn Location variable for adjusting player's spawn position.y
    private Vector3 spawnPosition;
    private NetworkVariable<float> referenceHeight = new NetworkVariable<float>(0f);
    private bool isHeightSetLocally = false;

    private void Start()
    {
        mainCam = GameObject.FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (AllPlayerDataManager.Instance != default &&
            AllPlayerDataManager.Instance.GetHasPlayerPlaced(NetworkManager.Singleton.LocalClientId)) return;
        
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("UI Hit was recognized");
                return;
            }
            TouchToRay(Input.mousePosition);
        }
#endif
#if UNITY_IOS || UNITY_ANDROID
        
        if (Input.touchCount > 0 && Input.touchCount < 2 &&
            Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);
            
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = touch.position;

            List<RaycastResult> results = new List<RaycastResult>();

            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0) {
                // We hit a UI element
                Debug.Log("We hit an UI Element");
                return;
            }
            
            Debug.Log("Touch detected, fingerId: " + touch.fingerId);  // Debugging line


            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                Debug.Log("Is Pointer Over GOJ, No placement ");
                return;
            }
            TouchToRay(touch.position);
        }
#endif
    }
    
    void TouchToRay(Vector3 touch)
    {
        Ray ray = mainCam.ScreenPointToRay(touch);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            if (IsServer)
            {
                if (!isHeightSetLocally)
                {
                    // 기준 높이를 서버에 설정 요청
                    SetReferenceHeightServerRpc(hit.point.y);
                    isHeightSetLocally = true;
                }

                // 기준 높이를 설정한 후에도 캐릭터를 생성
                spawnPosition = hit.point;
                spawnPosition.y = referenceHeight.Value;

                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                SpawnPlayerServerRpc(spawnPosition, rotation, NetworkManager.Singleton.LocalClientId);
            }
            else if (referenceHeight.Value != 0f)
            {
                // 네트워크 변수에서 기준 높이를 가져와서 높이를 조정
                spawnPosition = hit.point;
                spawnPosition.y = referenceHeight.Value;

                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                SpawnPlayerServerRpc(spawnPosition, rotation, NetworkManager.Singleton.LocalClientId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetReferenceHeightServerRpc(float height)
    {
        referenceHeight.Value = height;
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnPlayerServerRpc(Vector3 position, Quaternion rotation, ulong callerID)
    {
        GameObject character = Instantiate(placementObject, position, rotation);

        NetworkObject characterNetworkObject = character.GetComponent<NetworkObject>();
        characterNetworkObject.SpawnWithOwnership(callerID);

        AllPlayerDataManager.Instance.AddPlacedPlayer(callerID);
    }

    void OnGUI()
    {
        // 디버깅용 GUI 띄우기
        GUI.Label(new Rect(50, 100, 400, 30), $"플레이어 {NetworkManager.Singleton.LocalClientId}가 생성된 위치: {spawnPosition}");
        GUI.Label(new Rect(50, 130, 400, 30), $"기준 높이: {referenceHeight.Value}");
    }
}
