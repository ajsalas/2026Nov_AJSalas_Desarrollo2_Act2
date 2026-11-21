using UnityEngine;
using DungeonSurvivor.Core;
using DungeonSurvivor.Audio;

namespace DungeonSurvivor.Player
{

    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Attributes")]
        [SerializeField] private int _maxHealth = 100;

        [Header("Stamina Attributes")]
        [SerializeField] private float _maxStamina = 100f;
        [SerializeField] private float _staminaRegenRate = 25f;
        [SerializeField] private float _staminaRegenDelay = 1.0f;

        [Header("Health Regeneration (Kite Recovery)")]
        [SerializeField] private float _healthRegenDelay = 3.5f;
        [SerializeField] private float _healthRegenRate = 4.0f;

        private int _currentHealth;
        private float _currentStamina;
        private float _lastStaminaUseTime;
        private float _lastDamageTime;
        private float _healthRegenAccumulator;
        private bool _isInvulnerable;

        public bool IsAlive => _currentHealth > 0;
        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;
        public float CurrentStamina => _currentStamina;
        public float MaxStamina => _maxStamina;
        public bool IsInvulnerable => _isInvulnerable;

        private void Awake()
        {
            _currentHealth = _maxHealth;
            _currentStamina = _maxStamina;
        }

        private void Start()
        {
            GameEvents.TriggerPlayerHealthChanged(_currentHealth, _maxHealth);
            GameEvents.TriggerPlayerStaminaChanged(_currentStamina, _maxStamina);
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

            RegenerateStamina();
            RegenerateHealth();
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (!IsAlive) return;

            _lastDamageTime = Time.time;
            _healthRegenAccumulator = 0f;

            if (_isInvulnerable)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(SoundType.PlayerDodge, transform.position);
                }
                return;
            }

            var combat = GetComponent<PlayerCombat>();
            if (combat != null && combat.IsBlocking)
            {
                Vector3 toAttacker = damageInfo.Instigator != null
                    ? (damageInfo.Instigator.transform.position - transform.position).normalized
                    : -damageInfo.HitDirection;
                toAttacker.y = 0f;

                if (Vector3.Dot(transform.forward, toAttacker) > -0.2f)
                {
                    if (combat.IsParryWindow)
                    {

                        if (AudioManager.Instance != null)
                        {
                            AudioManager.Instance.PlaySound(SoundType.PlayerHit, transform.position);
                        }

                        Enemy.EnemyController enemyCtrl = null;
                        if (damageInfo.Instigator != null)
                        {
                            enemyCtrl = damageInfo.Instigator.GetComponent<Enemy.EnemyController>()
                                ?? damageInfo.Instigator.GetComponentInParent<Enemy.EnemyController>()
                                ?? damageInfo.Instigator.GetComponentInChildren<Enemy.EnemyController>();
                        }

                        if (enemyCtrl == null)
                        {
                            Collider[] nearby = Physics.OverlapSphere(transform.position + transform.forward * 1.5f, 3.0f);
                            for (int i = 0; i < nearby.Length; i++)
                            {
                                enemyCtrl = nearby[i].GetComponent<Enemy.EnemyController>() ?? nearby[i].GetComponentInParent<Enemy.EnemyController>();
                                if (enemyCtrl != null && enemyCtrl.gameObject != gameObject) break;
                            }
                        }

                        if (enemyCtrl != null)
                        {
                            enemyCtrl.Stun(1.8f);
                        }
                        return;
                    }
                    else
                    {

                        ConsumeStamina(14f);
                        int reduced = Mathf.Max(1, Mathf.RoundToInt(damageInfo.Amount * 0.15f));
                        damageInfo = new DamageInfo(reduced, damageInfo.HitPoint, damageInfo.HitDirection, damageInfo.Instigator);
                        if (AudioManager.Instance != null)
                        {
                            AudioManager.Instance.PlaySound(SoundType.PlayerHit, transform.position);
                        }
                    }
                }
            }

            _currentHealth = Mathf.Max(0, _currentHealth - damageInfo.Amount);
            Debug.Log($"[PlayerHealth] Recibio {damageInfo.Amount} de dano por {(damageInfo.Instigator != null ? damageInfo.Instigator.name : "Entorno")}. Salud restante: {_currentHealth}/{_maxHealth}");
            GameEvents.TriggerPlayerHealthChanged(_currentHealth, _maxHealth);
            GameEvents.TriggerPlayerDamaged(damageInfo.HitPoint);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.PlayerHit, transform.position);
            }

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (!IsAlive) return;

            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            GameEvents.TriggerPlayerHealthChanged(_currentHealth, _maxHealth);
        }

        public bool ConsumeStamina(float amount)
        {
            if (_currentStamina < amount) return false;

            _currentStamina = Mathf.Max(0f, _currentStamina - amount);
            _lastStaminaUseTime = Time.time;
            GameEvents.TriggerPlayerStaminaChanged(_currentStamina, _maxStamina);
            return true;
        }

        public void SetInvulnerable(bool state)
        {
            _isInvulnerable = state;
        }

        private void RegenerateStamina()
        {
            if (Time.time < _lastStaminaUseTime + _staminaRegenDelay) return;
            if (_currentStamina >= _maxStamina) return;

            _currentStamina = Mathf.Min(_maxStamina, _currentStamina + _staminaRegenRate * Time.deltaTime);
            GameEvents.TriggerPlayerStaminaChanged(_currentStamina, _maxStamina);
        }

        private void RegenerateHealth()
        {
            if (!IsAlive || _currentHealth >= _maxHealth) return;

            if (Time.time >= _lastDamageTime + _healthRegenDelay)
            {
                _healthRegenAccumulator += _healthRegenRate * Time.deltaTime;
                if (_healthRegenAccumulator >= 1.0f)
                {
                    int heal = Mathf.FloorToInt(_healthRegenAccumulator);
                    _healthRegenAccumulator -= heal;
                    Heal(heal);
                }
            }
        }

        private void Die()
        {
            StartCoroutine(PlayerDeathRoutine());
        }

        private System.Collections.IEnumerator PlayerDeathRoutine()
        {

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.Defeat, transform.position);
            }

            var controller = GetComponent<PlayerController>();
            if (controller != null) controller.enabled = false;

            var combat = GetComponent<PlayerCombat>();
            if (combat != null) combat.enabled = false;

            var procAnim = GetComponent<ProceduralCharacterAnimator>();
            if (procAnim != null) procAnim.enabled = false;

            var charCol = GetComponent<CharacterController>();
            if (charCol != null) charCol.enabled = false;

            var anim = GetComponentInChildren<Animator>();
            if (anim != null) anim.enabled = false;

            DropWeaponPhysical("1H_Sword");
            DropWeaponPhysical("Badge_Shield");
            DropWeaponPhysical("Spike_Shield");
            DropWeaponPhysical("Rectangle_Shield");
            DropWeaponPhysical("Round_Shield");

            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            Quaternion fallRot = startRot * Quaternion.Euler(72f, 15f, -18f);
            Vector3 endPos = startPos - transform.forward * 0.35f;

            while (elapsed < 0.95f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / 0.95f);
                transform.position = Vector3.Lerp(startPos, endPos, t);
                transform.rotation = Quaternion.Slerp(startRot, fallRot, t);
                yield return null;
            }

            yield return new WaitForSeconds(2.5f);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }

        private void DropWeaponPhysical(string weaponName)
        {
            try
            {
                Transform[] children = GetComponentsInChildren<Transform>();
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i] != null && children[i].name == weaponName && children[i].gameObject.activeInHierarchy)
                    {
                        Transform weapon = children[i];
                        weapon.SetParent(null);

                        var hitbox = weapon.gameObject.GetComponent<WeaponHitbox>();
                        if (hitbox != null)
                        {
                            hitbox.DisableHitbox();
                            hitbox.enabled = false;
                        }

                        Collider col = weapon.gameObject.GetComponent<Collider>();
                        if (col == null)
                        {
                            col = weapon.gameObject.AddComponent<BoxCollider>();
                        }
                        col.isTrigger = false;
                        col.enabled = true;

                        Rigidbody rb = weapon.gameObject.GetComponent<Rigidbody>();
                        if (rb == null)
                        {
                            rb = weapon.gameObject.AddComponent<Rigidbody>();
                        }
                        rb.isKinematic = false;
                        rb.mass = 2f;
                        rb.linearVelocity = Vector3.up * 1.6f + UnityEngine.Random.insideUnitSphere * 0.9f;
                        rb.angularVelocity = UnityEngine.Random.insideUnitSphere * 5f;
                        break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[PlayerHealth] Dropping {weaponName} notice: {ex.Message}");
            }
        }
    }
}

