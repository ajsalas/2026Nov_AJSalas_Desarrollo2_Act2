using System.Collections.Generic;
using UnityEngine;
using DungeonSurvivor.Core;
using DungeonSurvivor.Audio;

namespace DungeonSurvivor.Player
{

    public class WeaponHitbox : MonoBehaviour
    {
        [Header("Damage Configuration")]
        [SerializeField] private int _damageAmount = 30;
        [SerializeField] private float _sweepRadius = 1.8f;
        [SerializeField] private GameObject _instigator;

        private Collider _hitboxCollider;
        private HashSet<IDamageable> _hitEntities = new HashSet<IDamageable>();
        private bool _isActive;

        private void Awake()
        {
            _hitboxCollider = GetComponent<Collider>();
            if (_hitboxCollider != null)
            {
                _hitboxCollider.isTrigger = true;
                _hitboxCollider.enabled = false;
            }

            if (_instigator == null)
            {
                var player = GetComponentInParent<PlayerHealth>();
                _instigator = player != null ? player.gameObject : gameObject;
            }
        }

        private void Update()
        {
            if (_isActive)
            {
                PerformActiveSweep();
            }
        }

        public void EnableHitbox()
        {
            _isActive = true;
            _hitEntities.Clear();
            if (_hitboxCollider != null)
            {
                _hitboxCollider.enabled = true;
            }
            PerformActiveSweep();
        }

        public void DisableHitbox()
        {
            _isActive = false;
            _hitEntities.Clear();
            if (_hitboxCollider != null)
            {
                _hitboxCollider.enabled = false;
            }
        }

        private void PerformActiveSweep()
        {
            Vector3 origin = _instigator != null
                ? _instigator.transform.position + _instigator.transform.forward * 1.2f + Vector3.up * 0.9f
                : transform.position;

            Collider[] hits = Physics.OverlapSphere(origin, _sweepRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                ProcessHit(hits[i]);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive) return;
            ProcessHit(other);
        }

        private void ProcessHit(Collider other)
        {
            if (other == null) return;
            if (_instigator != null && (other.gameObject == _instigator || other.transform.IsChildOf(_instigator.transform))) return;
            if (other.CompareTag("Player") || other.GetComponentInParent<PlayerHealth>() != null) return;

            var damageable = other.GetComponent<IDamageable>() ?? other.GetComponentInParent<IDamageable>();
            if (damageable != null && damageable.IsAlive && !_hitEntities.Contains(damageable))
            {
                _hitEntities.Add(damageable);

                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Vector3 hitDir = _instigator != null ? (other.transform.position - _instigator.transform.position).normalized : transform.forward;

                damageable.TakeDamage(new DamageInfo(_damageAmount, hitPoint, hitDir, _instigator != null ? _instigator : gameObject));

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(SoundType.PlayerHit, hitPoint);
                }
            }
        }
    }
}

