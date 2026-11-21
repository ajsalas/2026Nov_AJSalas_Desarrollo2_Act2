using System.Collections;
using UnityEngine;
using DungeonSurvivor.Core;
using DungeonSurvivor.Player;
using DungeonSurvivor.Audio;

namespace DungeonSurvivor.Interactions
{

    public class ChestInteractable : InteractableBase, IDamageable
    {
        [Header("Chest Contents")]
        [SerializeField] private KeyType _containedKey = KeyType.BronzeKey;
        [SerializeField] private int _containedPotions = 2;

        [Header("Lid Animation")]
        [SerializeField] private Transform _chestLid;
        [SerializeField] private float _openAngle = -75f;
        [SerializeField] private float _openDuration = 0.6f;

        public bool IsAlive => !_hasInteracted;
        public int CurrentHealth => _hasInteracted ? 0 : 1;
        public int MaxHealth => 1;

        protected override void Awake()
        {
            base.Awake();
            _promptMessage = "Abrir Cofre [Llave de Bronce]";
            _isReusable = false;
        }

        protected override void OnInteract(PlayerController player)
        {
            StartCoroutine(AnimateLidOpen());

            var inventory = player != null ? player.GetComponent<PlayerInventory>() : null;
            if (inventory != null)
            {
                if (_containedKey != KeyType.None)
                {
                    inventory.AddKey(_containedKey);
                }

                if (_containedPotions > 0)
                {
                    inventory.AddPotion(_containedPotions);
                }
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.ChestOpen, transform.position);
            }

            GameEvents.TriggerInteractionPrompt("¡Obtuviste la Llave de Bronce y Pociones!", true);
            GameEvents.TriggerObjectiveUpdated("Llave de Bronce obtenida. Abre la gran reja norte.");
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (_hasInteracted) return;

            var player = damageInfo.Instigator != null
                ? (damageInfo.Instigator.GetComponent<PlayerController>() ?? damageInfo.Instigator.GetComponentInParent<PlayerController>())
                : null;
            if (player == null) player = FindFirstObjectByType<PlayerController>();

            if (player != null)
            {
                Interact(player);
            }
        }

        private IEnumerator AnimateLidOpen()
        {
            if (_chestLid == null) yield break;

            Quaternion startRot = _chestLid.localRotation;
            Quaternion targetRot = startRot * Quaternion.Euler(_openAngle, 0f, 0f);

            float elapsed = 0f;
            while (elapsed < _openDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / _openDuration);
                _chestLid.localRotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }

            _chestLid.localRotation = targetRot;
        }
    }
}

