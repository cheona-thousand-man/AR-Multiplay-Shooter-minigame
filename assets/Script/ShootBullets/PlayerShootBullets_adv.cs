using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShootBullets_adv : NetworkBehaviour
{
    private PlayerInputControls _playerInputControls;

    private const float BULLET_DELAY = .2f;
    private const float SHOOTING_DELAY = .2f;
    private const float BULLET_SPEED = 10f;
    private const float BULLET_ANGLE_AMPLYFY = .11f;
    private const float BULLET_SHOOT_ANGLE_MAX = 25;

    private Transform bulletSpawnTransform;

    [SerializeField] private GameObject bulletPrefab;

    private float bulletShootAngle;

    private Coroutine ShootAutoCoroutine;

    public override void OnNetworkSpawn()
    {
        bulletSpawnTransform = GetComponentInChildren<ShootBulletTransformReference>().transform;

        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerInputControls = GetComponent<PlayerInputControls>();

            _playerInputControls.OnShootInput += StartShooting;
            _playerInputControls.OnShootInputCancelled += StopShooting;
            _playerInputControls.OnShootAnglePerformed += PlayerInputControlsOnOnShootAnglePerformed;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerInputControls.OnShootInput -= StartShooting;
            _playerInputControls.OnShootInputCancelled -= StopShooting;
            _playerInputControls.OnShootAnglePerformed -= PlayerInputControlsOnOnShootAnglePerformed;
        }
    }

    private void PlayerInputControlsOnOnShootAnglePerformed(Vector2 angleValue)
    {
        float newAngle;

        if (angleValue == Vector2.zero)
        {
            newAngle = 0;
        }
        else
        {
            newAngle = bulletShootAngle + angleValue.y * -BULLET_ANGLE_AMPLYFY;
            newAngle = Mathf.Clamp(newAngle, -BULLET_SHOOT_ANGLE_MAX, BULLET_SHOOT_ANGLE_MAX);
        }

        bulletShootAngle = newAngle;
    }

    private void StartShooting()
    {
        if (ShootAutoCoroutine == null)
        {
            ShootAutoCoroutine = StartCoroutine(ShootCoroutine());
        }
    }

    IEnumerator ShootCoroutine()
    {
        yield return new WaitForSeconds(SHOOTING_DELAY);

        while (true)
        {
            StartShootBulletServerRpc(bulletShootAngle, NetworkManager.Singleton.LocalClientId);
            yield return new WaitForSeconds(BULLET_DELAY);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void StartShootBulletServerRpc(float bulletShootAngle, ulong callerID)
    {
        Quaternion rotation = Quaternion.Euler(bulletShootAngle, 0, 1);

        bulletSpawnTransform.localRotation = rotation;

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTransform.position, 
            Quaternion.LookRotation(bulletSpawnTransform.up));

        NetworkObject bulletNetworkObject = bullet.GetComponent<NetworkObject>();

        bulletNetworkObject.Spawn();
        bullet.GetComponent<BulletData_adv>().SetOwnershipServerRpc(callerID);

        Rigidbody bulletRigidBody = bullet.GetComponent<Rigidbody>();

        bulletRigidBody.AddForce(bulletSpawnTransform.forward * BULLET_SPEED, ForceMode.VelocityChange);
    }

    private void StopShooting()
    {
        if (ShootAutoCoroutine != null)
        {
            bulletShootAngle = 0f;
            StopCoroutine(ShootAutoCoroutine);
            ShootAutoCoroutine = null;
        }
    }
}
