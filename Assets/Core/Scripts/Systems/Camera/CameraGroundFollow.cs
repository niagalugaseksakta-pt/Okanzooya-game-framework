/*
PSEUDOCODE / PLAN (detailed):
1. Goal:
   - Make player flip feel natural (camera horizontal offset flips smoothly with player facing).
   - Keep the groundCheckPoint stable in its intended "base" position relative to the player's body
     so flips do not accidentally move the ground-check to the opposite side or cause unstable ground detection.

2. Strategy:
   - On Start:
     - Cache the absolute horizontal local position of the groundCheckPoint as baseGroundLocalX (always positive).
     - Cache Cinemachine follow offset if present.
   - Each LateUpdate:
     - Ensure player and groundCheckPoint exist; if not, skip.
     - Enforce groundCheckPoint local X to be baseGroundLocalX multiplied by the player's facing sign.
       This keeps the ground check in front of the player (stable base offset) whenever the player flips.
       Use a safe sign calculation (>= 0 => +1, else -1) to avoid zero sign from Mathf.Sign(0).
     - Compute isGrounded using Physics2D.OverlapCircle at the groundCheckPoint world position.
     - Smoothly lerp the camera horizontal target offset based on player's facing sign to avoid instant snapping.
     - Compute desired follow offset (different for grounded vs falling).
     - Apply currentXOffset and vertical offsets and lerp the Cinemachine transposer FollowOffset to that desired vector.
   - Keep OnDrawGizmos unchanged.

3. Notes:
   - Using absolute baseGroundLocalX ensures initial designer-placed groundCheckPoint distance is preserved.
   - Enforcing localPosition.x each frame cancels the visual mirroring effect caused by negative parent scale.
   - Camera flipping remains smooth by using Lerp for currentXOffset and for transposer.FollowOffset.
*/

using Game.Config;
using Game.Model.Player;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraGroundFollow : MonoBehaviour
{
    public PlayerEntity playerEntity;
    public Transform objectTrigerCammeraFollow;
    public LayerMask groundMask;
    public float groundCheckRadius = 0.25f;
    public float groundedOffsetY = 0f;
    public float airOffsetY = -2f;
    public float lookingVerticalStrengthOffsetY = -2f;
    public float horizontalOffset = 1.5f;
    public float smoothSpeed = 4f;

    public bool isGrounded = true;

    // Cinemachine
    public CinemachineCamera vcam;
    private CinemachineFollow transposer;
    private Vector3 baseFollowOffset;

    public Vector3 normalOffset = new Vector3(0, 1.5f, 0);
    public Vector3 fallOffset = new Vector3(0, 0.0f, 0);
    public float offsetLerpSpeed = 6f;

    private float currentXOffset;
    // used when groundCheckPoint is an independent GameObject
    private float baseGroundLocalX;
    private float baseGroundOffsetY;
    private float baseGroundOffsetZ;

    // smoothing state
    private float offsetVel; // for SmoothDamp
    [Header("Look Input Offset")]
    public float lookStrength = 1.2f;
    public float lookSmooth = 6f;
    private Vector3 lookOffsetSmooth;
    private Vector2 lookInput; // <--- joystick akan isi ini

    private void Awake()
    {
        if (playerEntity == null)
        {
            playerEntity = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
        }
        if (playerEntity == null) return;
        StartCoroutine(SetVcam());

        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator SetVcam()
    {
        yield return new WaitForSeconds(Config.cameraWaitToplayer);

        if (vcam == null)
            vcam = GetComponent<CinemachineCamera>();

        if (vcam != null)
        {
            // let Cinemachine follow the player transform
            vcam.Target.TrackingTarget = playerEntity.transform;
            vcam.Follow = playerEntity.transform;
            transposer = vcam.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineFollow;
            if (transposer != null)
                baseFollowOffset = transposer.FollowOffset;
        }

        groundedOffsetY = transform.position.y - playerEntity.transform.position.y;
        airOffsetY = groundedOffsetY - 2f;
        currentXOffset = horizontalOffset * FacingSign();

        // If groundCheckPoint is NOT a child, cache its offset relative to the player position.
        if (objectTrigerCammeraFollow != null)
        {
            Vector3 off = objectTrigerCammeraFollow.position - playerEntity.transform.position;
            baseGroundLocalX = Mathf.Abs(off.x);
            baseGroundOffsetY = off.y;
            baseGroundOffsetZ = off.z;
        }
    }
    private void LateUpdate()
    {
        if (playerEntity == null || objectTrigerCammeraFollow == null) return;

        // Position groundCheckPoint independently near the player based on the cached offset.
        float sign = FacingSign();
        objectTrigerCammeraFollow.position = playerEntity.transform.position + new Vector3(baseGroundLocalX * sign, baseGroundOffsetY, baseGroundOffsetZ);

        isGrounded = Physics2D.OverlapCircle(objectTrigerCammeraFollow.position, groundCheckRadius, groundMask);

        // smooth horizontal offset to avoid instant snap when flipping
        float targetX = horizontalOffset * sign;

        // Use SmoothDamp for frame-rate stable smoothing. smoothTime = 1/smoothSpeed (higher smoothSpeed => faster).
        float smoothTime = Mathf.Max(0.0001f, 1f / Mathf.Max(0.0001f, smoothSpeed));
        currentXOffset = Mathf.SmoothDamp(currentXOffset, targetX, ref offsetVel, smoothTime);

        // only update Cinemachine transposer — do NOT set camera.transform.position directly
        if (transposer != null)
        {

            if ((playerEntity.IsLookUp || playerEntity.IsLookDown) && playerEntity.isGrounded)
            {
                Vector3 desired = baseFollowOffset + normalOffset;
                // apply our horizontal and vertical offsets relative to the player
                desired.x += currentXOffset;
                desired.y += playerEntity.Vertical * lookingVerticalStrengthOffsetY;

                // keep z untouched (transposer manages depth)
                desired.z = -10f;

                transposer.FollowOffset = Vector3.Lerp(transposer.FollowOffset, desired, Time.deltaTime * offsetLerpSpeed);
            }
            else
            {
                Vector3 desired = playerEntity.IsJumping && !isGrounded ? baseFollowOffset + normalOffset : isGrounded ? baseFollowOffset + normalOffset : baseFollowOffset + fallOffset;
                // apply our horizontal and vertical offsets relative to the player
                desired.x += currentXOffset;
                desired.y += airOffsetY;

                // keep z untouched (transposer manages depth)
                desired.z = -10f;

                transposer.FollowOffset = Vector3.Lerp(transposer.FollowOffset, desired, Time.deltaTime * offsetLerpSpeed);
            }
        }
    }


    private float FacingSign()
    {
        // Use player-facing boolean if available (more robust than reading localScale.x).
        if (playerEntity != null)
            return playerEntity.FacingRight ? 1f : -1f;
        return 1f;
    }

    private void OnDrawGizmos()
    {
        if (!objectTrigerCammeraFollow) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(objectTrigerCammeraFollow.position, groundCheckRadius);
    }
}
