using System.Collections;
using UnityEngine;
using DungeonSurvivor.Core;
using DungeonSurvivor.Player;
using DungeonSurvivor.Audio;

namespace DungeonSurvivor.Interactions
{
    public enum DoorOpenType
    {
        SlideUp,
        RotateOpen
    }

    public class DoorInteractable : InteractableBase
    {
        [Header("Key Requirement")]
        [SerializeField] private KeyType _requiredKey = KeyType.BronzeKey;

        [Header("Animation Setup")]
        [SerializeField] private DoorOpenType _openType = DoorOpenType.SlideUp;
        [SerializeField] private Transform _doorMover;
        [SerializeField] private float _slideHeight = 4.8f;
        [SerializeField] private float _rotateAngle = 90f;
        [SerializeField] private float _animationDuration = 1.2f;

        private bool _isOpen;

        protected override void Awake()
        {
            base.Awake();
            _promptMessage = "Abrir Reja de Hierro [E]";
            _isReusable = false;

            if (_doorMover == null)
            {
                _doorMover = transform;
            }
        }

        public override bool CanInteract(PlayerInventory inventory)
        {
            if (_isOpen) return false;
            if (_requiredKey == KeyType.None) return true;
            return inventory != null && inventory.HasKey(_requiredKey);
        }

        protected override string GetLockedPrompt()
        {
            string keyName = _requiredKey == KeyType.BronzeKey ? "Llave de Bronce" : "Llave Maestra";
            return $"[Cerrada - Requiere {keyName}]";
        }

        protected override void OnInteract(PlayerController player)
        {
            OpenDoor();
        }

        public void OpenDoorRemote()
        {
            if (!_isOpen)
            {
                OpenDoor();
            }
        }

        private void OpenDoor()
        {
            _isOpen = true;
            _hasInteracted = true;
            StartCoroutine(AnimateOpenRoutine());

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.DoorOpen, transform.position);
            }

            GameEvents.TriggerObjectiveUpdated("Reja levadiza abierta. Avanza al Sanctum del Guardián.");
        }

        private IEnumerator AnimateOpenRoutine()
        {
            if (_doorMover == null) yield break;

            Vector3 startPos = _doorMover.localPosition;
            Quaternion startRot = _doorMover.localRotation;

            Vector3 targetPos = startPos + Vector3.up * _slideHeight;
            Quaternion targetRot = startRot * Quaternion.Euler(0f, _rotateAngle, 0f);

            float elapsed = 0f;
            while (elapsed < _animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / _animationDuration);

                if (_openType == DoorOpenType.SlideUp)
                {
                    _doorMover.localPosition = Vector3.Lerp(startPos, targetPos, t);
                }
                else
                {
                    _doorMover.localRotation = Quaternion.Slerp(startRot, targetRot, t);
                }

                yield return null;
            }

            if (_openType == DoorOpenType.SlideUp) _doorMover.localPosition = targetPos;
            else _doorMover.localRotation = targetRot;
        }
    }
}

