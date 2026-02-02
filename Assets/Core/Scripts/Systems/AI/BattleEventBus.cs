using System;
using System.Collections.Generic;

public static class BattleEventBus
{
    public static event Action<EntityTurnBaseAI> OnTurnStart;
    public static event Action<EntityTurnBaseAI> OnTurnEnd;
    public static event Action OnBattleStart;
    public static event Action OnBattleEnd;

    private static List<EntityTurnBaseAI> entities = new();
    private static int currentIndex = -1;
    private static bool battleActive = false;

    public static void Register(EntityTurnBaseAI entity)
    {
        if (!entities.Contains(entity))
            entities.Add(entity);
    }

    public static void Unregister(EntityTurnBaseAI entity)
    {
        entities.Remove(entity);
    }

    public static void StartBattle()
    {
        if (entities.Count == 0) return;
        battleActive = true;
        OnBattleStart?.Invoke();
        NextTurn();
    }

    public static void NextTurn()
    {
        if (!battleActive || entities.Count == 0) return;

        // Skip dead ones
        entities.RemoveAll(e => e == null || e.IsDead());

        if (entities.Count <= 1)
        {
            battleActive = false;
            OnBattleEnd?.Invoke();
            return;
        }

        currentIndex = UnityEngine.Random.Range(0, entities.Count);
        OnTurnStart?.Invoke(entities[currentIndex]);
    }

    public static void EndTurn(EntityTurnBaseAI entity)
    {
        if (!battleActive) return;
        OnTurnEnd?.Invoke(entity);
        NextTurn();
    }
}
