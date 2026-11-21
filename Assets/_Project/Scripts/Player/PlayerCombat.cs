using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonSurvivor.Core;
using DungeonSurvivor.Audio;

namespace DungeonSurvivor.Player
{

    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Attack Settings")]
        [SerializeField] private float _attackCooldown = 0.5f;
        [SerializeField] private float _attackStaminaCost = 15f;
        [SerializeField] private float _hitboxActiveStart = 0.04f;
        [SerializeField] private float _hitboxActiveDuration = 0.35f;

        [Header("Hitbox Reference")]
        [SerializeField] private WeaponHitbox _weaponHitbox;

        private PlayerHealth _playerHealth;
        private Animator _animator;
        private ProceduralCharacterAnimator _proceduralAnim;
        private bool _isAttacking;
        private bool _isBlocking;
        private float _blockStartTime;
        private float _lastAttackTime;
        private HashSet<IDamageable> _hitEnemiesInCurrentSwing = new HashSet<IDamageable>();

        public bool IsAttacking => _isAttacking;
        public bool IsBlocking => _isBlocking;
        public bool IsParryWindow => _isBlocking && (Time.time - _blockStartTime <= 0.35f);

        private void Awake()
        {
            _playerHealth = GetComponent<PlayerHealth>();
            _animator = GetComponentInChildren<Animator>();
            _proceduralAnim = GetComponent<ProceduralCharacterAnimator>();

            if (_weaponHitbox == null)
            {
                _weaponHitbox = GetComponentInChildren<WeaponHitbox>(true);
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;
            if (!_playerHealth.IsAlive) return;

            HandleBlockInput();

            if (!_isBlocking && InputHelper.IsAttackPressed())
            {
                TryAttack();
            }
        }

        private void HandleBlockInput()
        {
            if (_isAttacking)
            {
                if (_isBlocking) StopBlocking();
                return;
            }

            bool wantsBlock = InputHelper.IsBlockHeld() && _playerHealth.CurrentStamina > 3f;
            if (wantsBlock && !_isBlocking)
            {
                StartBlocking();
            }
            else if (!wantsBlock && _isBlocking)
            {
                StopBlocking();
            }
        }

        private void StartBlocking()
        {
            _isBlocking = true;
            _blockStartTime = Time.time;
            if (_animator != null)
            {
                _animator.SetBool("IsBlocking", true);
            }
        }

        private void StopBlocking()
        {
            _isBlocking = false;
            if (_animator != null)
            {
                _animator.SetBool("IsBlocking", false);
            }
        }

        public void TryAttack()
        {
            if (_isAttacking || Time.time < _lastAttackTime + _attackCooldown) return;

            if (_playerHealth.ConsumeStamina(_attackStaminaCost))
            {
                StartCoroutine(PerformAttackRoutine());
            }
        }

        private IEnumerator PerformAttackRoutine()
        {
            _isAttacking = true;
            _lastAttackTime = Time.time;
            _hitEnemiesInCurrentSwing.Clear();

            if (_animator != null)
            {
                _animator.SetTrigger("Attack");
            }
            if (_proceduralAnim != null)
            {
                _proceduralAnim.TriggerAttackSlash();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.PlayerSwing, transform.position);
            }

            yield return new WaitForSeconds(_hitboxActiveStart);

            if (_weaponHitbox == null)
            {
                _weaponHitbox = GetComponentInChildren<WeaponHitbox>(true);
            }

            if (_weaponHitbox != null)
            {
                _weaponHitbox.EnableHitbox();
            }

            PerformDirectAttackSweep();

            yield return new WaitForSeconds(_hitboxActiveDuration * 0.45f);

            PerformDirectAttackSweep();

            yield return new WaitForSeconds(_hitboxActiveDuration * 0.55f);

            if (_weaponHitbox != null)
            {
                _weaponHitbox.DisableHitbox();
            }

            _isAttacking = false;
        }

        private void PerformDirectAttackSweep()
        {
            Vector3 sweepOrigin = transform.position + transform.forward * 1.3f + Vector3.up * 0.9f;
            float sweepRadius = 1.9f;

            Collider[] hits = Physics.OverlapSphere(sweepOrigin, sweepRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider col = hits[i];
                if (col == null || col.gameObject == gameObject || col.transform.IsChildOf(transform)) continue;
                if (col.CompareTag("Player") || col.GetComponentInParent<PlayerHealth>() != null) continue;

                var damageable = col.GetComponent<IDamageable>() ?? col.GetComponentInParent<IDamageable>();
                if (damageable != null && damageable.IsAlive && !_hitEnemiesInCurrentSwing.Contains(damageable))
                {
                    _hitEnemiesInCurrentSwing.Add(damageable);

                    Vector3 hitPoint = col.ClosestPoint(sweepOrigin);
                    Vector3 hitDir = (col.transform.position - transform.position).normalized;

                    damageable.TakeDamage(new DamageInfo(30, hitPoint, hitDir, gameObject));

                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySound(SoundType.PlayerHit, hitPoint);
                    }
                }
            }
        }
    }
}

