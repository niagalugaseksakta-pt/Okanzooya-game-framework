using Game.Model.Player;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DeadPoint : MonoBehaviour
{
    [Header("References")]
    public PlayerEntity playerEntity;

    private void Awake()
    {
        if (playerEntity == null)
        {
            playerEntity = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
        }
    }
    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerEntity.Stats.CurrentHealth -= 1;
        if (!playerEntity.IsDead)
        {
            ReSpawnPlayer();
        }

    }

    private void ReSpawnPlayer()
    {
        playerEntity.transform.position = new Vector3(playerEntity.Stats.lastCheckpointPosition.x - 100f, playerEntity.Stats.lastCheckpointPosition.y, playerEntity.Stats.lastCheckpointPosition.z);
    }
}
