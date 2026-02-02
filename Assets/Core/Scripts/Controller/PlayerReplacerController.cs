using Game.Model.Player;
using UnityEngine;

public class PlayerReplacerController : MonoBehaviour
{
    [SerializeField] private PlayerEntity playerEntity;

    private void Awake()
    {
        // Ensure playerEntity: try inspector, then fall back to existing search logic
        if (playerEntity == null)
        {
            playerEntity = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
        }

        if (playerEntity == null)
            return; // abort if still missing

        playerEntity.transform.position = transform.position;

        playerEntity.Stats.lastCheckpointPosition = playerEntity.Stats.isThroughDoor ? PortalRegistry.Get(playerEntity.Stats.nextPortal).transform.position : playerEntity.Stats.lastCheckpointPosition == Vector3.zero ? transform.position : playerEntity.Stats.lastCheckpointPosition;
        playerEntity.Stats.isFreshStart = playerEntity.Stats.isThroughDoor ? false : true;

        try
        {
            SaveManager._instance.Save(playerEntity);
        }
        catch
        {
            Debug.LogWarning("[Checkpoint] SaveManager belum di-assign.");
        }
    }

    private void Reset()
    {
        // Ensure playerEntity: try inspector, then fall back to existing search logic
        if (playerEntity == null)
        {
            playerEntity = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
        }

        if (playerEntity == null)
            return; // abort if still missing

        playerEntity.transform.position = transform.position;

        playerEntity.Stats.lastCheckpointPosition = playerEntity.Stats.isThroughDoor ? PortalRegistry.Get(playerEntity.Stats.nextPortal).transform.position : playerEntity.Stats.lastCheckpointPosition == Vector3.zero ? transform.position : playerEntity.Stats.lastCheckpointPosition;
        playerEntity.Stats.isFreshStart = playerEntity.Stats.isThroughDoor ? false : true;

        try
        {
            SaveManager._instance.Save(playerEntity);
        }
        catch
        {
            Debug.LogWarning("[Checkpoint] SaveManager belum di-assign.");
        }
    }
}
