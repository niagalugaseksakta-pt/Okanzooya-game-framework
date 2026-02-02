using UnityEngine;

namespace Game.Model.Enemy
{
    public class EnemyEntity : EntityBase
    {
        [SerializeField] private int attackPower = 10;

        private void Reset()
        {
            base.characterName = gameObject.name;
            base.teamId = 2; // Enemy team
            base.stats.CurrentHealth = 10;
            base.stats.MaxHealth = 10;

        }

        protected override void Awake()
        {

            // sprite rule
            base.sprite = gameObject.GetComponentInChildren<SpriteRenderer>();
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
