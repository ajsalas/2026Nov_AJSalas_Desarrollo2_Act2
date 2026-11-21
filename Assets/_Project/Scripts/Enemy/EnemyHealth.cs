using System;
using System.Collections;
using UnityEngine;
using DungeonSurvivor.Core;
using DungeonSurvivor.Audio;

namespace DungeonSurvivor.Enemy
{

    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private int _maxHealth = 60;
        [SerializeField] private bool _isBoss = false;

        [Header("Loot Drop on Death")]
        [SerializeField] private KeyType _dropKey = KeyType.None;
        [SerializeField] private GameObject _dropPrefab;

        public event Action OnDamaged;
        public event Action OnDeath;

        private int _currentHealth;
        private Renderer[] _renderers;
        private Color[] _originalColors;

        public bool IsAlive => _currentHealth > 0;
        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;
        public bool IsBoss => _isBoss;

        private void Awake()
        {
            _currentHealth = _maxHealth;
            CacheRenderers();
        }

        private void CacheRenderers()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _originalColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i].material.HasProperty("_BaseColor"))
                {
                    _originalColors[i] = _renderers[i].material.GetColor("_BaseColor");
                }
                else if (_renderers[i].material.HasProperty("_Color"))
                {
                    _originalColors[i] = _renderers[i].material.color;
                }
            }
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (!IsAlive) return;

            _currentHealth = Mathf.Max(0, _currentHealth - damageInfo.Amount);
            OnDamaged?.Invoke();

            FlashColor(Color.red, 0.12f);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.PlayerHit, transform.position);
            }

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void FlashColor(Color color, float duration)
        {
            StartCoroutine(FlashColorRoutine(color, duration));
        }

        private IEnumerator FlashColorRoutine(Color color, float duration)
        {
            if (_renderers == null || _renderers.Length == 0) yield break;

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && _renderers[i].material != null)
                {
                    if (_renderers[i].material.HasProperty("_BaseColor"))
                        _renderers[i].material.SetColor("_BaseColor", color);
                    else if (_renderers[i].material.HasProperty("_Color"))
                        _renderers[i].material.color = color;
                }
            }

            yield return new WaitForSeconds(duration);

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && _renderers[i].material != null && i < _originalColors.Length)
                {
                    if (_renderers[i].material.HasProperty("_BaseColor"))
                        _renderers[i].material.SetColor("_BaseColor", _originalColors[i]);
                    else if (_renderers[i].material.HasProperty("_Color"))
                        _renderers[i].material.color = _originalColors[i];
                }
            }
        }

        private void Die()
        {
            OnDeath?.Invoke();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.EnemyDeath, transform.position);
            }

            if (_dropKey != KeyType.None)
            {
                var playerInventory = FindFirstObjectByType<Player.PlayerInventory>();
                if (playerInventory != null)
                {
                    playerInventory.AddKey(_dropKey);
                }

                if (_isBoss || _dropKey == KeyType.CryptMasterKey)
                {
                    GameEvents.TriggerInteractionPrompt("¡El Guardián ha caído! Obtuviste la Llave Maestra.", true);
                    GameEvents.TriggerObjectiveUpdated("Llave Maestra obtenida. Escapa por el Portal Arcano.");
                }
            }

            if (_dropPrefab != null)
            {
                Instantiate(_dropPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }

            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            StartCoroutine(DeathFallAndBlinkRoutine());
        }

        private IEnumerator DeathFallAndBlinkRoutine()
        {
            var anim = GetComponentInChildren<Animator>();

            if (anim != null && anim.runtimeAnimatorController != null)
            {

                yield return new WaitForSeconds(1.3f);

                anim.speed = 0f;
            }
            else
            {

                float elapsed = 0f;
                Vector3 startPos = transform.position;
                Quaternion startRot = transform.rotation;
                Quaternion fallRot = startRot * Quaternion.Euler(-75f, 15f, 0f);

                while (elapsed < 0.85f)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / 0.85f);
                    transform.rotation = Quaternion.Slerp(startRot, fallRot, t);
                    yield return null;
                }
            }

            float floorLingerTime = _isBoss ? 22.0f : 9.0f;
            yield return new WaitForSeconds(floorLingerTime);

            int blinks = 8;
            float blinkDuration = 0.18f;
            for (int b = 0; b < blinks; b++)
            {
                SetRenderersVisible(false);
                yield return new WaitForSeconds(blinkDuration);
                SetRenderersVisible(true);
                yield return new WaitForSeconds(blinkDuration);
            }

            Destroy(gameObject);
        }

        private void SetRenderersVisible(bool visible)
        {
            if (_renderers == null) return;
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                {
                    _renderers[i].enabled = visible;
                }
            }
        }
    }
}

