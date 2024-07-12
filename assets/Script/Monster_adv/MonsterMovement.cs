using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MonsterMovement : NetworkBehaviour
{
    public float moveInterval = 15f; // 몬스터가 이동하는 시간 간격
    public float moveRange = .75f; // 이동할 거리의 범위
    public float moveSpeed = 0.05f; // 몬스터가 이동하는 속도
    public float restDuration = 5f; // 몬스터가 이동 후 쉬는 시간

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private bool isMoving = false;

    private Animator animator; // 애니메이터 컴포넌트

    void Start()
    {
        originalPosition = transform.position;
        animator = GetComponent<Animator>(); // 애니메이터 컴포넌트 가져오기

        if (IsServer) // 서버에서만 이동 로직을 실행합니다
        {
            StartCoroutine(MoveRandomly());
        }
    }

    IEnumerator MoveRandomly()
    {
        while (true)
        {
            // 이동
            if (!isMoving)
            {
                Vector3 newPosition = originalPosition + new Vector3(
                    Random.Range(-moveRange, moveRange),
                    0,
                    Random.Range(-moveRange, moveRange)
                );

                MoveMonsterServerRpc(newPosition);
            }

            // 이동이 끝날 때까지 대기
            while (isMoving)
            {
                yield return null;
            }

            // 이동이 완료된 후 3초 동안 쉬기
            yield return new WaitForSeconds(restDuration);
        }
    }

    [ServerRpc]
    void MoveMonsterServerRpc(Vector3 newPosition)
    {
        targetPosition = newPosition;
        isMoving = true;
        StartCoroutine(MoveToTarget());
    }

    IEnumerator MoveToTarget()
    {
        // 이동 시작 시 Walk 애니메이션 재생
        if (animator != null)
        {
            animator.SetBool("isWalk", true);
            animator.SetBool("isIdle", false);
        }

        while (isMoving)
        {
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                transform.position = targetPosition;
                isMoving = false;

                // 이동 종료 시 Idle 애니메이션 재생
                if (animator != null)
                {
                    animator.SetBool("isWalk", false);
                    animator.SetBool("isIdle", true);
                }

                break;
            }

            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            MoveMonsterClientRpc(transform.position);
            yield return null;
        }
    }

    [ClientRpc]
    void MoveMonsterClientRpc(Vector3 newPosition)
    {
        if (!IsServer)
        {
            targetPosition = newPosition;
            if (!isMoving)
            {
                isMoving = true;
                StartCoroutine(MoveToTargetClient());

                // 이동 시작 시 Walk 애니메이션 재생
                if (animator != null)
                {
                    animator.SetBool("isWalk", true);
                    animator.SetBool("isIdle", false);
                }
            }
        }
    }

    IEnumerator MoveToTargetClient()
    {
        while (isMoving)
        {
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                transform.position = targetPosition;
                isMoving = false;

                // 이동 종료 시 Idle 애니메이션 재생
                if (animator != null)
                {
                    animator.SetBool("isWalk", false);
                    animator.SetBool("isIdle", true);
                }

                break;
            }

            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            yield return null;
        }
    }
}
