using UnityEngine;
using DungeonSurvivor.Core;

namespace DungeonSurvivor.Player
{

    public class ThirdPersonOrbitCamera : MonoBehaviour
    {
        [Header("Target & Offsets")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _targetOffset = new Vector3(0f, 1.6f, 0f);

        [Header("Distance & Limits")]
        [SerializeField] private float _defaultDistance = 4.5f;
        [SerializeField] private float _minDistance = 0.8f;
        [SerializeField] private float _maxDistance = 6.0f;
        [SerializeField] private float _minPitch = -25f;
        [SerializeField] private float _maxPitch = 65f;

        [Header("Sensitivity & Smoothing")]
        [SerializeField] private float _mouseSensitivity = 2.0f;
        [SerializeField] private float _smoothRotationTime = 0.05f;
        [SerializeField] private float _distanceSmoothSpeed = 15f;

        [Header("Obstacle Collision")]
        [SerializeField] private LayerMask _collisionLayers = ~0;
        [SerializeField] private float _sphereRadius = 0.25f;
        [SerializeField] private float _collisionBuffer = 0.2f;

        private float _yaw;
        private float _pitch = 15f;
        private float _currentDistance;
        private Vector3 _currentRotation;
        private Vector3 _rotationVelocity;

        public Transform Target => _target;
        public float TargetYaw => _yaw;

        private void Start()
        {
            _currentDistance = _defaultDistance;

            if (_target != null)
            {
                _yaw = _target.eulerAngles.y;
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

            ReadInput();
            UpdateCameraPosition();
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        private void ReadInput()
        {
            Vector2 look = InputHelper.GetLookInput() * _mouseSensitivity;

            _yaw += look.x;
            _pitch -= look.y;
            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
        }

        private void UpdateCameraPosition()
        {
            Vector3 targetEuler = new Vector3(_pitch, _yaw, 0f);
            _currentRotation = Vector3.SmoothDamp(_currentRotation, targetEuler, ref _rotationVelocity, _smoothRotationTime);
            Quaternion rotation = Quaternion.Euler(_currentRotation);

            Vector3 pivot = _target.position + _targetOffset;
            Vector3 desiredDirection = rotation * -Vector3.forward;

            float targetDistance = _defaultDistance;
            if (Physics.SphereCast(pivot, _sphereRadius, desiredDirection, out RaycastHit hit, _maxDistance, _collisionLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.root != _target.root)
                {
                    targetDistance = Mathf.Clamp(hit.distance - _collisionBuffer, _minDistance, _maxDistance);
                }
            }

            _currentDistance = Mathf.Lerp(_currentDistance, targetDistance, Time.deltaTime * _distanceSmoothSpeed);

            transform.position = pivot + desiredDirection * _currentDistance;
            transform.LookAt(pivot);
        }
    }
}

