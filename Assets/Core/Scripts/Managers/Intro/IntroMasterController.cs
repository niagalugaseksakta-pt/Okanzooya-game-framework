using Game.Model.Player;
using UnityEngine;
using UnityEngine.Tilemaps;

public class IntroMasterController : MonoBehaviour
{
    [Header("Player")]
    public PlayerEntity player;              // Reference your existing entity
    public float runSpeed = 5f;
    public float dashInterval = 2f;

    [Header("Parallax Tilemaps")]
    public Tilemap[] tilemaps;               // Put your tilemaps here
    public float[] parallaxFactors;          // 0.1f for far, 0.5f for mid, etc.

    [Header("Auto Action Timing")]
    private float dashTimer;

    private void Start()
    {
        if (parallaxFactors.Length != tilemaps.Length)
            Debug.LogWarning("Tilemap count and parallax factor count must match.");

        dashTimer = dashInterval;

        // Force player idle animation or intro pose if needed
        //player.SetAction(PlayerEntity.EntityActionType.Run);
    }

    private void Update()
    {
        RunPlayer();
        HandleDashTimer();
        MoveParallax();
    }

    private void RunPlayer()
    {
        // Use your PlayerEntity movement logic
        //player.Move(new Vector2(runSpeed, 0f));
    }

    private void HandleDashTimer()
    {
        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
        {
            // Call your existing dash method
            //player.Dash();

            dashTimer = dashInterval; // reset
        }
    }

    private void MoveParallax()
    {
        float delta = runSpeed * Time.deltaTime;

        for (int i = 0; i < tilemaps.Length; i++)
        {
            Vector3 pos = tilemaps[i].transform.position;
            pos.x -= delta * parallaxFactors[i];
            tilemaps[i].transform.position = pos;
        }
    }
}
