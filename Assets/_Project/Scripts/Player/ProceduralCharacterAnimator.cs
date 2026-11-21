using System.Collections;
using UnityEngine;
using DungeonSurvivor.Core;

namespace DungeonSurvivor.Player
{

    public class ProceduralCharacterAnimator : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private Transform _visualModel;
        [SerializeField] private Transform _swordTransform;
        [SerializeField] private Transform _shieldTransform;
        [SerializeField] private Transform _upperArmLeft;
        [SerializeField] private Transform _lowerArmLeft;
        [SerializeField] private TrailRenderer _slashTrail;

        [Header("Configuracion de locomocion")]
        [SerializeField] private float _walkBobFrequency = 9f;
        [SerializeField] private float _walkBobAmount = 0.06f;
        [SerializeField] private float _walkSwayAmount = 4f;
        [SerializeField] private float _sprintLeanAmount = 10f;

        private CharacterController _characterController;
        private PlayerController _playerController;
        private PlayerCombat _playerCombat;

        private Vector3 _modelInitialLocalPos;
        private Quaternion _modelInitialLocalRot;
        private Vector3 _swordInitialLocalPos;
        private Quaternion _swordInitialLocalRot;
        private Vector3 _shieldInitialLocalPos;
        private Quaternion _shieldInitialLocalRot;

        private bool _shieldSearched;
        private float _blockWeight;
        private float _locomotionPhase;
        private bool _isAttacking;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _playerController = GetComponent<PlayerController>();
            _playerCombat = GetComponent<PlayerCombat>();

            if (_visualModel == null)
            {
                Transform knightChild = transform.Find("Knight");
                _visualModel = knightChild != null ? knightChild : (transform.childCount > 0 ? transform.GetChild(0) : transform);
            }

            _modelInitialLocalPos = _visualModel != null ? _visualModel.localPosition : Vector3.zero;
            _modelInitialLocalRot = _visualModel != null ? _visualModel.localRotation : Quaternion.identity;

            FindAndSetupSword();
            FindAndSetupShield();
        }

        public void FindAndSetupShield()
        {
            _shieldSearched = true;

            if (_shieldTransform == null)
            {
                foreach (Transform child in GetComponentsInChildren<Transform>(true))
                {
                    string n = child.name;
                    if (n.Contains("Round_Shield") || (n.Contains("Shield") && !n.Contains("Large") && !n.Contains("Spike") && !n.Contains("Badge")))
                    {
                        _shieldTransform = child;
                        break;
                    }
                }
            }

            if (_shieldTransform != null)
            {
                _shieldTransform.localPosition = Vector3.zero;
                _shieldTransform.localRotation = Quaternion.identity;
                _shieldInitialLocalPos = Vector3.zero;
                _shieldInitialLocalRot = Quaternion.identity;
            }
        }

        public void FindAndSetupSword()
        {
            if (_swordTransform == null)
            {
                foreach (Transform child in GetComponentsInChildren<Transform>(true))
                {
                    if (child.name.Contains("1H_Sword") && !child.name.Contains("Offhand"))
                    {
                        _swordTransform = child;
                        break;
                    }
                }
            }

            if (_swordTransform != null)
            {
                _swordInitialLocalPos = _swordTransform.localPosition;
                _swordInitialLocalRot = _swordTransform.localRotation;

                _slashTrail = _swordTransform.GetComponentInChildren<TrailRenderer>();
                if (_slashTrail == null)
                {
                    GameObject trailObj = new GameObject("Slash_Arc_Trail");
                    trailObj.transform.SetParent(_swordTransform);
                    trailObj.transform.localPosition = new Vector3(0, 0, 0.6f);
                    _slashTrail = trailObj.AddComponent<TrailRenderer>();
                    _slashTrail.time = 0.22f;
                    _slashTrail.startWidth = 0.35f;
                    _slashTrail.endWidth = 0.02f;
                    _slashTrail.emitting = false;

                    var gradient = new Gradient();
                    gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0f), new GradientColorKey(new Color(0.3f, 0.85f, 1f), 1f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(0.85f, 0f), new GradientAlphaKey(0f, 1f) }
                    );
                    _slashTrail.colorGradient = gradient;

                    Material trailMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default"));
                    _slashTrail.sharedMaterial = trailMat;
                }
            }
        }

        private void Update()
        {
            if (_visualModel == null) return;
            UpdateLocomotion();
        }

        private void LateUpdate()
        {
            UpdateShieldPose();
        }

        private void UpdateShieldPose()
        {
            if (_shieldTransform == null && !_shieldSearched)
            {
                FindAndSetupShield();
            }

            if (_shieldTransform == null) return;

            _shieldTransform.localPosition = Vector3.zero;
            _shieldTransform.localRotation = Quaternion.identity;
        }

        private void UpdateLocomotion()
        {
            if (_characterController == null) return;

            Vector3 horizontalVelocity = new Vector3(_characterController.velocity.x, 0, _characterController.velocity.z);
            float speed = horizontalVelocity.magnitude;
            bool isGrounded = _characterController.isGrounded;

            if (isGrounded && speed > 0.3f)
            {
                float freq = (speed > 5f) ? _walkBobFrequency * 1.35f : _walkBobFrequency;
                _locomotionPhase += Time.deltaTime * freq;

                float bob = Mathf.Abs(Mathf.Sin(_locomotionPhase)) * _walkBobAmount;
                float sway = Mathf.Sin(_locomotionPhase * 0.5f) * _walkSwayAmount;
                float forwardLean = Mathf.Clamp(speed / 7.5f, 0f, 1f) * _sprintLeanAmount;

                _visualModel.localPosition = _modelInitialLocalPos + new Vector3(0, bob, 0);
                _visualModel.localRotation = _modelInitialLocalRot * Quaternion.Euler(forwardLean, sway, -sway * 0.5f);
            }
            else if (!isGrounded)
            {
                float airTilt = Mathf.Clamp(_characterController.velocity.y * 1.5f, -15f, 15f);
                _visualModel.localPosition = Vector3.Lerp(_visualModel.localPosition, _modelInitialLocalPos, Time.deltaTime * 6f);
                _visualModel.localRotation = Quaternion.Slerp(_visualModel.localRotation, _modelInitialLocalRot * Quaternion.Euler(-airTilt, 0, 0), Time.deltaTime * 6f);
            }
            else
            {
                float idleBob = Mathf.Sin(Time.time * 2.2f) * 0.015f;
                _visualModel.localPosition = Vector3.Lerp(_visualModel.localPosition, _modelInitialLocalPos + new Vector3(0, idleBob, 0), Time.deltaTime * 8f);
                _visualModel.localRotation = Quaternion.Slerp(_visualModel.localRotation, _modelInitialLocalRot, Time.deltaTime * 8f);
            }
        }

        public void TriggerAttackSlash()
        {
            if (!_isAttacking && _swordTransform != null)
            {
                StartCoroutine(PerformSwordSlashRoutine());
            }
        }

        private IEnumerator PerformSwordSlashRoutine()
        {
            _isAttacking = true;

            if (_slashTrail != null)
            {
                _slashTrail.Clear();
                _slashTrail.emitting = true;
            }

            Quaternion windupRot = _swordInitialLocalRot * Quaternion.Euler(-35f, 40f, -20f);
            float t = 0f;
            while (t < 0.07f)
            {
                t += Time.deltaTime;
                _swordTransform.localRotation = Quaternion.Slerp(_swordInitialLocalRot, windupRot, t / 0.07f);
                yield return null;
            }

            Quaternion slashRot = _swordInitialLocalRot * Quaternion.Euler(65f, -85f, 45f);
            t = 0f;
            while (t < 0.14f)
            {
                t += Time.deltaTime;
                float progress = t / 0.14f;
                _swordTransform.localRotation = Quaternion.Slerp(windupRot, slashRot, Mathf.SmoothStep(0, 1, progress));
                yield return null;
            }

            if (_slashTrail != null)
            {
                _slashTrail.emitting = false;
            }

            t = 0f;
            while (t < 0.18f)
            {
                t += Time.deltaTime;
                _swordTransform.localRotation = Quaternion.Slerp(slashRot, _swordInitialLocalRot, t / 0.18f);
                yield return null;
            }

            _swordTransform.localRotation = _swordInitialLocalRot;
            _swordTransform.localPosition = _swordInitialLocalPos;
            _isAttacking = false;
        }
    }
}

