using UnityEngine;

public class BattleStartController : MonoBehaviour
{
    void Start()
    {
        // optional delay
        Invoke(nameof(StartBattle), 1f);
    }

    private void StartBattle()
    {
        BattleEventBus.StartBattle();
    }
}
