using UnityEngine;
using static Game.Config.Config;

namespace Game.Model
{
    public class EntitySpellGlobal : EntityProjectile
    {
        [Header("Spell Add-ons")]
        public float ExplosionRadius = 0f;
        public GameObject ExplosionEffect;

        // Do animation Here or other stuff
        protected override void OnHitTarget()
        {
            base.OnHitTarget();

            // Optional explosion FX
            if (ExplosionEffect != null)
            {
                Instantiate(ExplosionEffect, transform.position, Quaternion.identity);
            }

            PlayAction(EntityProjectileActionType.Finish);
            StartCoroutine(DestroyAfterAnimation());
        }

        // Hit defined by physics
        public void OnTriggerEnter2D(Collider2D other)
        {

            // Jika mengenai Player (misal digunakan oleh monster)
            if (other.CompareTag("Player"))
            {
                EntityBase hit = other.GetComponent<EntityBase>();
                if (hit != null)
                {
                    hit.TakeDamage(damage);
                }

                OnHitTarget();
            }
        }
        public void OnTriggerExit2D(Collider2D other)
        {
            StartCoroutine(DestroyAfterAnimation());

        }
    }
}
