using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MonsterHandleHealthUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text MonsterHealthText;

    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = GameObject.FindObjectOfType<Camera>();
    }

    void Update()
    {
        if (_mainCamera)
        {
            MonsterHealthText.transform.LookAt(_mainCamera.transform);
        }
    }
}
