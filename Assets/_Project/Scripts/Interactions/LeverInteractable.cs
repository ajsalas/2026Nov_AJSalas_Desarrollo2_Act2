using System.Collections;
using UnityEngine;
using DungeonSurvivor.Core;
using DungeonSurvivor.Player;
using DungeonSurvivor.Audio;

namespace DungeonSurvivor.Interactions
{

    public class LeverInteractable : InteractableBase
    {
        [Header("Mechanism Targets")]
        [SerializeField] private DoorInteractable _targetDoor;

        [Header("Handle Animation")]
        [SerializeField] private Transform _handleTransform;
        [SerializeField] private float _pullAngle = 60f;
        [SerializeField] private float _pullDuration = 0.4f;

        protected override void Awake()
        {
            base.Awake();
            _promptMessage = "Tirar de la Palanca";
            _isReusable = false;
        }

        protected override void OnInteract(PlayerController player)
        {
            StartCoroutine(AnimateHandleRoutine());

            if (_targetDoor != null)
            {
                _targetDoor.OpenDoorRemote();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.ChestOpen, transform.position);
            }

            GameEvents.TriggerObjectiveUpdated("Mecanismo activado. Una reja lejana se ha abierto.");
        }

        private IEnumerator AnimateHandleRoutine()
        {
            if (_handleTransform == null) yield break;

            Quaternion startRot = _handleTransform.localRotation;
            Quaternion targetRot = startRot * Quaternion.Euler(_pullAngle, 0f, 0f);

            float elapsed = 0f;
            while (elapsed < _pullDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / _pullDuration);
                _handleTransform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }

            _handleTransform.localRotation = targetRot;
        }
    }
}

