using UnityEngine;
using DungeonSurvivor.Core;
using DungeonSurvivor.Player;

namespace DungeonSurvivor.Interactions
{

    [RequireComponent(typeof(Collider))]
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        public static InteractableBase ActiveInteractable { get; private set; }

        [Header("Interaction Settings")]
        [SerializeField] protected string _promptMessage = "Interactuar";
        [SerializeField] protected bool _isReusable = false;

        protected bool _hasInteracted;
        protected bool _isPlayerInRange;
        protected PlayerController _playerInRange;

        public virtual string InteractionPrompt => _promptMessage;
        public bool HasInteracted => _hasInteracted;

        public virtual Vector3 InteractionIconWorldOffset => Vector3.up * 1.1f;

        protected virtual void Awake()
        {
            var col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        protected virtual void OnDisable()
        {
            if (ActiveInteractable == this)
            {
                ActiveInteractable = null;
            }
        }

        protected virtual void Update()
        {
            if (_hasInteracted && !_isReusable)
            {
                if (ActiveInteractable == this) ActiveInteractable = null;
                return;
            }
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

            if (_playerInRange == null)
            {
                _playerInRange = FindFirstObjectByType<PlayerController>();
            }

            if (_playerInRange != null)
            {
                Vector3 p1 = transform.position;
                Vector3 p2 = _playerInRange.transform.position;
                p1.y = 0f;
                p2.y = 0f;
                float dist = Vector3.Distance(p1, p2);

                if (dist <= 4.2f)
                {
                    ActiveInteractable = this;
                    if (!_isPlayerInRange)
                    {
                        _isPlayerInRange = true;
                        var inventory = _playerInRange.GetComponent<PlayerInventory>();
                        string prompt = CanInteract(inventory) ? $"[E] {InteractionPrompt}" : GetLockedPrompt();
                        GameEvents.TriggerInteractionPrompt(prompt, true);
                    }
                }
                else if (dist > 4.8f && _isPlayerInRange)
                {
                    _isPlayerInRange = false;
                    if (ActiveInteractable == this) ActiveInteractable = null;
                    GameEvents.TriggerInteractionPrompt("", false);
                }
            }

            if (_isPlayerInRange && _playerInRange != null && InputHelper.IsInteractPressed())
            {
                var inventory = _playerInRange.GetComponent<PlayerInventory>();
                if (CanInteract(inventory))
                {
                    Interact(_playerInRange);
                }
                else
                {
                    OnPrerequisitesFailed();
                }
            }
        }

        public virtual bool CanInteract(PlayerInventory inventory)
        {
            return !_hasInteracted || _isReusable;
        }

        public virtual void Interact(PlayerController player)
        {
            _hasInteracted = true;
            if (ActiveInteractable == this) ActiveInteractable = null;
            GameEvents.TriggerInteractionPrompt("", false);
            OnInteract(player);
        }

        protected abstract void OnInteract(PlayerController player);

        protected virtual void OnPrerequisitesFailed()
        {
        }

        private void OnTriggerEnter(Collider other)
        {
            HandlePlayerDetected(other);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!_isPlayerInRange)
            {
                HandlePlayerDetected(other);
            }
        }

        private void HandlePlayerDetected(Collider other)
        {
            if (_hasInteracted && !_isReusable) return;

            var player = other.GetComponent<PlayerController>() ?? other.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                _isPlayerInRange = true;
                _playerInRange = player;

                var inventory = player.GetComponent<PlayerInventory>();
                string prompt = CanInteract(inventory) ? $"[E] {InteractionPrompt}" : GetLockedPrompt();
                GameEvents.TriggerInteractionPrompt(prompt, true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var player = other.GetComponent<PlayerController>() ?? other.GetComponentInParent<PlayerController>();
            if (player != null && player == _playerInRange)
            {
                _isPlayerInRange = false;
                _playerInRange = null;
                GameEvents.TriggerInteractionPrompt("", false);
            }
        }

        protected virtual string GetLockedPrompt()
        {
            return $"[Bloqueado] {InteractionPrompt}";
        }
    }
}

