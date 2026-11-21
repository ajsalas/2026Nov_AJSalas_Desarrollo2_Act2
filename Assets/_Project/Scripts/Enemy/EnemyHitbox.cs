using System.Collections.Generic;
using UnityEngine;
using DungeonSurvivor.Core;

namespace DungeonSurvivor.Enemy
{

    public class EnemyHitbox : MonoBehaviour
    {
        [Header("Damage Configuration")]
        [SerializeField] private int _damageAmount = 20;
        [SerializeField] private GameObject _instigator;

        private Collider _collider;
        private HashSet<IDamageable> _hitEntities = new HashSet<IDamageable>();
        private bool _isActive;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            if (_collider != null)
            {
                _collider.isTrigger = true;
                _collider.enabled = false;
            }

            if (_instigator == null)
            {
                var enemy = GetComponentInParent<EnemyController>();
                _instigator = enemy != null ? enemy.gameObject : (GetComponentInParent<EnemyHealth>() != null ? GetComponentInParent<EnemyHealth>().gameObject : gameObject);
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
            if (_collider != null)
            {
                _collider.enabled = true;
            }
            PerformActiveSweep();
        }

        public void DisableHitbox()
        {
            _isActive = false;
            _hitEntities.Clear();
            if (_collider != null)
            {
                _collider.enabled = false;
            }
        }

        private void PerformActiveSweep()
        {
            bool isBoss = false;
            if (_instigator != null)
            {
                var h = _instigator.GetComponent<EnemyHealth>() ?? _instigator.GetComponentInParent<EnemyHealth>();
                if (h != null && h.IsBoss) isBoss = true;
            }

            Vector3 origin = _instigator != null
                ? _instigator.transform.position + _instigator.transform.forward * (isBoss ? 1.6f : 1.1f) + Vector3.up * 0.9f
                : transform.position;

            float sweepRadius = isBoss ? 2.6f : 1.65f;
            Collider[] hits = Physics.OverlapSphere(origin, sweepRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                ProcessHit(hits[i]);
            }

            if (_instigator != null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    Vector3 toPlayer = player.transform.position - _instigator.transform.position;
                    toPlayer.y = 0;
                    float dist = toPlayer.magnitude;
                    float maxConnectDist = isBoss ? 2.6f : 1.7f;

                    if (dist <= maxConnectDist)
                    {
                        float dot = Vector3.Dot(_instigator.transform.forward, toPlayer.normalized);
                        if (dot > -0.1f)
                        {
                            var playerCol = player.GetComponent<Collider>();
                            if (playerCol != null) ProcessHit(playerCol);
                        }
                    }
                }
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

            if (other.CompareTag("Player") || other.GetComponentInParent<Player.PlayerHealth>() != null)
            {
                var damageable = other.GetComponent<IDamageable>() ?? other.GetComponentInParent<IDamageable>();
                if (damageable != null && damageable.IsAlive && !_hitEntities.Contains(damageable))
                {
                    _hitEntities.Add(damageable);
                    Vector3 hitPoint = other.ClosestPoint(transform.position);
                    Vector3 hitDir = _instigator != null ? (other.transform.position - _instigator.transform.position).normalized : transform.forward;

                    damageable.TakeDamage(new DamageInfo(_damageAmount, hitPoint, hitDir, _instigator != null ? _instigator : gameObject));
                }
            }
        }
    }
}

