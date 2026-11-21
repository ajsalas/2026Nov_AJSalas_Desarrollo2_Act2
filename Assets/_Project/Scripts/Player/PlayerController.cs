using System.Collections;
using UnityEngine;
using DungeonSurvivor.Core;
using DungeonSurvivor.Audio;

namespace DungeonSurvivor.Player
{
    public enum LocomotionState
    {
        Idle,
        Walking,
        Running,
        Dodging,
        Attacking,
        Dead
    }

    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Locomotion Speeds")]
        [SerializeField] private float _walkSpeed = 4.5f;
        [SerializeField] private float _runSpeed = 7.5f;
        [SerializeField] private float _rotationSpeed = 14f;
        [SerializeField] private float _sprintStaminaCostPerSec = 18f;

        [Header("Jump Mechanics")]
        [SerializeField] private float _jumpForce = 7.5f;

        [Header("Gravity")]
        [SerializeField] private float _gravity = -22f;

        [Header("Dodge Roll Mechanics")]
        [SerializeField] private float _dodgeSpeed = 10.5f;
        [SerializeField] private float _dodgeDuration = 0.45f;
        [SerializeField] private float _dodgeStaminaCost = 25f;

        [Header("Camera Reference")]
        [SerializeField] private Transform _cameraTransform;

        private CharacterController _characterController;
        private PlayerHealth _playerHealth;
        private Animator _animator;

        private LocomotionState _state = LocomotionState.Idle;
        private Vector3 _velocity;
        private bool _isGrounded;
        private Vector3 _dodgeDirection;
        private float _footstepTimer;

        public LocomotionState State => _state;
        public bool IsGrounded => _isGrounded;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _playerHealth = GetComponent<PlayerHealth>();
            _animator = GetComponentInChildren<Animator>();

            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;
            if (!_playerHealth.IsAlive) return;

            CheckGroundStatus();

            if (_state == LocomotionState.Dodging)
            {
                ApplyGravity();
                _characterController.Move((_dodgeDirection * _dodgeSpeed + _velocity) * Time.deltaTime);
                return;
            }

            HandleMovement();
            HandleJumpInput();
            HandleDodgeInput();
            ApplyGravity();
        }

        private void CheckGroundStatus()
        {
            _isGrounded = _characterController.isGrounded;
            if (_isGrounded && _velocity.y < 0f)
            {
                _velocity.y = -2f;
            }
        }

        private void HandleMovement()
        {
            Vector2 moveInput = InputHelper.GetMoveInput();
            Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y);

            if (inputDir.magnitude >= 0.1f)
            {
                Vector3 camForward = _cameraTransform != null ? _cameraTransform.forward : transform.forward;
                Vector3 camRight = _cameraTransform != null ? _cameraTransform.right : transform.right;

                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                Vector3 moveDir = (camForward * inputDir.z + camRight * inputDir.x).normalized;

                var combat = GetComponent<PlayerCombat>();
                bool isBlocking = combat != null && combat.IsBlocking;

                bool isSprinting = !isBlocking && InputHelper.IsSprintHeld() && _playerHealth.ConsumeStamina(_sprintStaminaCostPerSec * Time.deltaTime);
                float speed = isBlocking ? 2.5f : (isSprinting ? _runSpeed : _walkSpeed);

                _state = isSprinting ? LocomotionState.Running : LocomotionState.Walking;

                _characterController.Move((moveDir * speed + _velocity) * Time.deltaTime);

                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);

                _footstepTimer += Time.deltaTime;
                float interval = isSprinting ? 0.3f : 0.45f;
                if (_footstepTimer >= interval && _isGrounded)
                {
                    _footstepTimer = 0f;
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySound(SoundType.Footstep, transform.position);
                    }
                }
            }
            else
            {
                _state = LocomotionState.Idle;
                _characterController.Move(_velocity * Time.deltaTime);
            }

            if (_animator != null)
            {
                float animSpeed = inputDir.magnitude * (_state == LocomotionState.Running ? 1f : 0.5f);
                _animator.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);
                _animator.SetBool("IsGrounded", _isGrounded);
            }
        }

        private void HandleJumpInput()
        {
            if (_isGrounded && _state != LocomotionState.Dodging && InputHelper.IsJumpPressed())
            {
                _velocity.y = _jumpForce;
                if (_animator != null)
                {
                    _animator.SetTrigger("Jump");
                }
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(SoundType.Footstep, transform.position);
                }
            }
        }

        private void HandleDodgeInput()
        {
            if (InputHelper.IsDodgePressed())
            {
                if (_playerHealth.ConsumeStamina(_dodgeStaminaCost))
                {
                    StartCoroutine(PerformDodgeRoll());
                }
            }
        }

        private IEnumerator PerformDodgeRoll()
        {
            _state = LocomotionState.Dodging;
            _playerHealth.SetInvulnerable(true);

            Vector2 moveInput = InputHelper.GetMoveInput();
            Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y);

            if (inputDir.magnitude >= 0.1f && _cameraTransform != null)
            {
                Vector3 camForward = _cameraTransform.forward;
                Vector3 camRight = _cameraTransform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                _dodgeDirection = (camForward.normalized * inputDir.z + camRight.normalized * inputDir.x).normalized;
            }
            else
            {
                _dodgeDirection = transform.forward;
            }

            transform.rotation = Quaternion.LookRotation(_dodgeDirection);

            if (_animator != null)
            {
                _animator.SetTrigger("Roll");
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.PlayerDodge, transform.position);
            }

            yield return new WaitForSeconds(_dodgeDuration);

            _playerHealth.SetInvulnerable(false);
            _state = LocomotionState.Idle;
        }

        private void ApplyGravity()
        {
            _velocity.y += _gravity * Time.deltaTime;
        }
    }
}

