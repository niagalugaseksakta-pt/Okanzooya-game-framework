using UnityEngine;

[System.Serializable]
public class BossAttackPatternObject
{
    public string animationState = "Attack";

    public float windup = 0.3f;
    public float recovery = 0.3f;

    public AttackModule attackModule;     // prefab logic
    public Transform attackPoint;
    internal bool useVanish;
}
