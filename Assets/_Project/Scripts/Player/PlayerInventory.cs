using System.Collections.Generic;
using UnityEngine;
using DungeonSurvivor.Core;
using DungeonSurvivor.Audio;

namespace DungeonSurvivor.Player
{

    public class PlayerInventory : MonoBehaviour
    {
        [Header("Starting Items")]
        [SerializeField] private int _initialPotions = 1;
        [SerializeField] private int _maxPotions = 5;
        [SerializeField] private int _potionHealAmount = 40;

        private HashSet<KeyType> _keys = new HashSet<KeyType>();
        private int _currentPotions;
        private PlayerHealth _playerHealth;

        public int CurrentPotions => _currentPotions;

        private void Awake()
        {
            _playerHealth = GetComponent<PlayerHealth>();
        }

        private void Start()
        {
            _currentPotions = _initialPotions;
            GameEvents.TriggerPotionCountChanged(_currentPotions);
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

            if (InputHelper.IsPotionPressed())
            {
                UsePotion();
            }
        }

        public void AddKey(KeyType key)
        {
            if (key == KeyType.None) return;

            if (_keys.Add(key))
            {
                GameEvents.TriggerKeyCollected(key);
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(SoundType.ChestOpen, transform.position);
                }

                string keyName = key == KeyType.BronzeKey ? "Llave de Bronce" : "Llave Maestra de la Cripta";
                GameEvents.TriggerObjectiveUpdated($"¡Obtuviste la {keyName}!");
            }
        }

        public bool HasKey(KeyType key)
        {
            return _keys.Contains(key);
        }

        public void AddPotion(int amount)
        {
            _currentPotions = Mathf.Clamp(_currentPotions + amount, 0, _maxPotions);
            GameEvents.TriggerPotionCountChanged(_currentPotions);
        }

        public bool UsePotion()
        {
            if (_currentPotions <= 0) return false;
            if (_playerHealth != null && _playerHealth.CurrentHealth >= _playerHealth.MaxHealth) return false;

            _currentPotions--;
            GameEvents.TriggerPotionCountChanged(_currentPotions);

            if (_playerHealth != null)
            {
                _playerHealth.Heal(_potionHealAmount);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.PotionDrink, transform.position);
            }

            return true;
        }
    }
}

