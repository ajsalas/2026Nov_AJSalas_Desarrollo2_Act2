using UnityEngine;
using DungeonSurvivor.Core;
using DungeonSurvivor.Player;

namespace DungeonSurvivor.Interactions
{

    public class PortalInteractable : InteractableBase
    {
        [Header("Portal Condition")]
        [SerializeField] private KeyType _requiredKey = KeyType.CryptMasterKey;

        protected override void Awake()
        {
            base.Awake();
            _promptMessage = "Activar Portal y Escapar";
            _isReusable = false;
        }

        public override bool CanInteract(PlayerInventory inventory)
        {
            return inventory != null && inventory.HasKey(_requiredKey);
        }

        protected override string GetLockedPrompt()
        {
            return "[Portal Inactivo - Requiere Llave Maestra]";
        }

        protected override void OnInteract(PlayerController player)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerVictory();
            }
        }
    }
}

