using Game.Model.Player;
using UnityEngine;

public class PlayerInstantiate : MonoBehaviour
{
    [Header("References")]
    public PlayerEntity playerEntity;

    private Animator anim;

    private void Awake()
    {
        if (playerEntity == null)
        {
            playerEntity = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
        }

        SaveManager.Init();
    }

    private void Start()
    {
        SpawnPlayer();
        HandlePlayAnimation();
    }

    private void SpawnPlayer()
    {
        SaveManager.Instance.LoadInto(playerEntity);
        playerEntity.transform.position = playerEntity.Stats.lastCheckpointPosition == Vector3.zero ? playerEntity.transform.position : playerEntity.Stats.lastCheckpointPosition;
    }

    private void HandlePlayAnimation()
    {
        if (anim == null) return;

        // Check clip existence secara runtime
        bool clipExists = false;
        RuntimeAnimatorController controller = anim.runtimeAnimatorController;

        foreach (var clip in controller.animationClips)
        {
            if (clip.name == "Smoke57")
            {
                clipExists = true;
                break;
            }
        }

        if (!clipExists)
        {
            Debug.LogError("Animation clip 'Smoke57' tidak ditemukan di Animator Controller!");
            return;
        }

        // Play the animation state
        anim.Play("Smoke57", 0, 0f);
    }
}
