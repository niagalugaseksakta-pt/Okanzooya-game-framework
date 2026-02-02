using UnityEngine;

public abstract class AttackModule : MonoBehaviour
{
    public abstract void Execute(Transform attackPoint, Transform target);
}
