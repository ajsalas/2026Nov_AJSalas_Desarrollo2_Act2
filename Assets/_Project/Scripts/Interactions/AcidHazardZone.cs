using UnityEngine;
using DungeonSurvivor.Core;
using DungeonSurvivor.Player;
using DungeonSurvivor.Audio;

namespace DungeonSurvivor.Interactions
{

    [RequireComponent(typeof(Collider))]
    public class AcidHazardZone : MonoBehaviour
    {
        [Header("Acid Hazard Configuration")]
        [SerializeField] private int _damagePerTick = 15;
        [SerializeField] private float _damageInterval = 0.65f;
        [SerializeField] private string _customWarningMessage;

        private bool _isPlayerInside;
        private PlayerHealth _playerHealth;
        private float _damageTimer;

        private void Awake()
        {
            var col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private string GetWarningText()
        {
            if (!string.IsNullOrEmpty(_customWarningMessage))
            {
                return _customWarningMessage;
            }

            if (gameObject.name.IndexOf("Sanctum", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                gameObject.name.IndexOf("Arcan", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "¡Peligro: Ácido arcano corrosivo! ¡Salta para salir!";
            }

            return "¡Peligro: Ácido corrosivo! ¡Salta para salir!";
        }

        private void Update()
        {
            if (!_isPlayerInside || _playerHealth == null || !_playerHealth.IsAlive) return;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

            _damageTimer += Time.deltaTime;
            if (_damageTimer >= _damageInterval)
            {
                _damageTimer = 0f;
                _playerHealth.TakeDamage(new DamageInfo(_damagePerTick, _playerHealth.transform.position, Vector3.up, gameObject));

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(SoundType.PlayerHit, _playerHealth.transform.position);
                }

                GameEvents.TriggerInteractionPrompt(GetWarningText(), true);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var playerHealth = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                _playerHealth = playerHealth;
                _isPlayerInside = true;
                _damageTimer = 0f;
                GameEvents.TriggerInteractionPrompt(GetWarningText(), true);
            }

            var enemyHealth = other.GetComponent<Enemy.EnemyHealth>() ?? other.GetComponentInParent<Enemy.EnemyHealth>();
            if (enemyHealth != null && enemyHealth.IsAlive)
            {
                if (enemyHealth.IsBoss)
                {
                    enemyHealth.TakeDamage(new DamageInfo(15, other.transform.position, Vector3.up, gameObject));
                }
                else
                {
                    enemyHealth.TakeDamage(new DamageInfo(9999, other.transform.position, Vector3.up, gameObject));
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!_isPlayerInside)
            {
                var playerHealth = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
                if (playerHealth != null)
                {
                    _playerHealth = playerHealth;
                    _isPlayerInside = true;
                    GameEvents.TriggerInteractionPrompt(GetWarningText(), true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var playerHealth = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null && playerHealth == _playerHealth)
            {
                _isPlayerInside = false;
                _playerHealth = null;
                GameEvents.TriggerInteractionPrompt("", false);
            }
        }
    }
}

