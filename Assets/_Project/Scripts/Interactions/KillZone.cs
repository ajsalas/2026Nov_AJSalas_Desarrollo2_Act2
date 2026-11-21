using UnityEngine;
using DungeonSurvivor.Core;

namespace DungeonSurvivor.Interactions
{

    [RequireComponent(typeof(Collider))]
    public class KillZone : MonoBehaviour
    {
        private void Awake()
        {
            var col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                damageable.TakeDamage(new DamageInfo(9999, other.transform.position, Vector3.up, gameObject));
            }
        }
    }
}

