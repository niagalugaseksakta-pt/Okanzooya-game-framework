using Game.Model.Player;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraPointSceneEffect : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform objectToPoint; // cinematic target
    [SerializeField] private Transform playerEntity; // optional, used if revertToPlayer is true

    [Header("Timing / Settings")]
    [SerializeField] private float lookDuration = 2f;
    [SerializeField] private bool revertToPlayer = false;

    [SerializeField] private CinemachineCamera virtualCamera;
    private CinemachineFollow transposer;

    private Transform originalFollow;
    private Coroutine runningCoroutine;

    private void Awake()
    {
        // Ensure virtual camera reference (follow CameraGroundFollow approach: find GameObject by tag and get component)
        if (virtualCamera == null)
        {
            var vcamObj = GameObject.FindWithTag("VirtualCamera");
            if (vcamObj != null)
            {
                virtualCamera = vcamObj.GetComponent<CinemachineCamera>();
            }

            if (virtualCamera == null)
            {
                Debug.LogWarning($"{nameof(CameraPointSceneEffect)}: No CinemachineVirtualCamera found in scene and none assigned.");
            }
        }

        // Cache original follow target if we have a vcam
        originalFollow = virtualCamera != null ? virtualCamera.Follow : null;

        // Cache transposer if present (safe; not required for current logic)
        if (virtualCamera != null)
        {
            transposer = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineFollow;
        }

        // Try to find player if not assigned

        if (playerEntity == null)
        {
            var playerObj = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
            if (playerObj != null)
                playerEntity = playerObj.transform;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (runningCoroutine == null)
            runningCoroutine = StartCoroutine(HandleFollowSequence());
    }

    private IEnumerator HandleFollowSequence()
    {
        if (objectToPoint == null)
        {
            Debug.LogWarning($"{nameof(CameraPointSceneEffect)}: `objectToPoint` is not assigned.");
            runningCoroutine = null;
            yield break;
        }

        if (virtualCamera == null)
        {
            Debug.LogWarning($"{nameof(CameraPointSceneEffect)}: `virtualCamera` is not assigned and none found in scene.");
            runningCoroutine = null;
            yield break;
        }

        // Set follow to cinematic target
        virtualCamera.Follow = objectToPoint;

        // Hold the cinematic follow for duration
        yield return new WaitForSeconds(lookDuration);

        // Revert follow target
        if (revertToPlayer && playerEntity != null)
        {
            virtualCamera.Follow = playerEntity;
        }
        else
        {
            virtualCamera.Follow = originalFollow;
        }

        runningCoroutine = null;
    }
}
