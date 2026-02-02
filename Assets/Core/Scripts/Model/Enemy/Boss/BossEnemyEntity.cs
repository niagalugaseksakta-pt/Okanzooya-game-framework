using UnityEngine;

namespace Game.Model.Enemy
{
    public class BossEnemyEntity : EntityBase
    {
        [SerializeField] private int attackPower = 10;

        protected override void Awake()
        {
            teamId = 2; // Enemy team
            DontDestroyOnLoad(gameObject);
        }
        public void Attack(EntityBase target)
        {
            Debug.Log($"{DisplayName} attacks {target.DisplayName}");
            target.TakeDamage(attackPower);
        }

        public override void OnDeath()
        {
            Debug.Log($"{DisplayName} has been destroyed!");
            Destroy(gameObject);
        }
    }
}
