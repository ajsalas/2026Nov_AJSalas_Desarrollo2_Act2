using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using DungeonSurvivor.Core;
using DungeonSurvivor.Audio;

namespace DungeonSurvivor.Enemy
{
    public enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Hurt,
        Stunned,
        Dead
    }

    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyController : MonoBehaviour
    {
        [Header("AI Sensors & Ranges")]
        [SerializeField] private float _detectionRadius = 10f;
        [SerializeField] private float _attackRadius = 2.0f;
        [SerializeField] private float _attackCooldown = 1.8f;
        [SerializeField] private float _loseTargetRadius = 14f;

        [Header("Patrol Configuration")]
        [SerializeField] private Transform[] _patrolWaypoints;
        [SerializeField] private float _patrolWaitTime = 2.0f;
        [SerializeField] private float _patrolSpeed = 2.5f;
        [SerializeField] private float _chaseSpeed = 4.2f;

        [Header("Combat Hitbox")]
        [SerializeField] private EnemyHitbox _enemyHitbox;

        private NavMeshAgent _navAgent;
        private EnemyHealth _health;
        private Animator _animator;
        private Transform _playerTransform;
        private EnemyState _currentState = EnemyState.Patrol;

        private int _currentWaypointIndex;
        private float _patrolTimer;
        private float _lastAttackTime;
        private bool _isAttacking;
        private Vector3 _prevPosition;

        public EnemyState CurrentState => _currentState;

        private void Awake()
        {
            _navAgent = GetComponent<NavMeshAgent>();
            _health = GetComponent<EnemyHealth>();
            _animator = GetComponentInChildren<Animator>();
            if (_enemyHitbox == null) _enemyHitbox = GetComponentInChildren<EnemyHitbox>();

            EnsureAnimatorController();

            if (_health != null && _health.IsBoss)
            {
                _chaseSpeed = 2.4f;
                _patrolSpeed = 1.2f;
                _attackCooldown = 2.4f;
                _detectionRadius = 12f;
                _loseTargetRadius = 18f;
                _attackRadius = 2.1f;
                if (_navAgent != null)
                {
                    _navAgent.stoppingDistance = 1.7f;
                    _navAgent.speed = _chaseSpeed;
                }
            }
            else
            {
                _chaseSpeed = 3.6f;
                _patrolSpeed = 2.2f;
                _attackCooldown = 1.6f;
                _detectionRadius = 7.5f;
                _loseTargetRadius = 14f;
                _attackRadius = 1.45f;
                if (_navAgent != null)
                {
                    _navAgent.stoppingDistance = 1.15f;
                    _navAgent.speed = _chaseSpeed;
                }
            }
        }

        public void EnsureAnimatorController()
        {
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }

            if (_animator != null && _animator.runtimeAnimatorController == null)
            {
                string targetName = (_health != null && _health.IsBoss) ? "Skeleton_Warrior_Animator" : "Skeleton_Minion_Animator";
                var allControllers = Resources.FindObjectsOfTypeAll<RuntimeAnimatorController>();
                foreach (var ctrl in allControllers)
                {
                    if (ctrl != null && ctrl.name.Equals(targetName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        _animator.runtimeAnimatorController = ctrl;
                        return;
                    }
                }

                foreach (var ctrl in allControllers)
                {
                    if (ctrl != null && ctrl.name.Contains(targetName.Substring(0, 15)))
                    {
                        _animator.runtimeAnimatorController = ctrl;
                        return;
                    }
                }
            }
        }

        private void Start()
        {
            EnsureAnimatorController();
            _prevPosition = transform.position;
            FindPlayer();

            if (_health != null && _health.IsBoss)
            {
                _attackRadius = 2.1f;
                if (_navAgent != null) _navAgent.stoppingDistance = 1.7f;
            }
            else
            {
                _attackRadius = 1.45f;
                if (_navAgent != null) _navAgent.stoppingDistance = 1.15f;
            }

            if (_health != null && _health.IsBoss)
            {
                _patrolWaypoints = new Transform[0];
            }
            else if (_patrolWaypoints != null && _patrolWaypoints.Length > 0 && _patrolWaypoints[0] != null)
            {
                float distToWp = Vector3.Distance(transform.position, _patrolWaypoints[0].position);
                if (distToWp > 14f)
                {
                    _patrolWaypoints = new Transform[0];
                }
            }

            if (_navAgent != null && _navAgent.isOnNavMesh)
            {
                _navAgent.speed = _patrolSpeed;
                MoveToNextWaypoint();
            }
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnDamaged += HandleDamaged;
                _health.OnDeath += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDamaged -= HandleDamaged;
                _health.OnDeath -= HandleDeath;
            }
        }

        private void Update()
        {
            if (_currentState == EnemyState.Dead) return;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                if (_navAgent != null && _navAgent.isOnNavMesh) _navAgent.isStopped = true;
                return;
            }

            if (_navAgent != null && !_navAgent.isOnNavMesh && _navAgent.enabled)
            {
                _navAgent.enabled = false;
            }

            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }

            bool isMoving = false;
            if (_currentState == EnemyState.Chase)
            {
                isMoving = true;
            }
            else if (_currentState == EnemyState.Patrol && _patrolWaypoints != null && _patrolWaypoints.Length > 0 && _currentWaypointIndex < _patrolWaypoints.Length)
            {
                Transform targetWp = _patrolWaypoints[_currentWaypointIndex];
                if (targetWp != null)
                {
                    float dist = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetWp.position.x, 0, targetWp.position.z));
                    if (dist > 0.45f) isMoving = true;
                }
            }

            if (_animator != null)
            {
                _animator.SetFloat("Speed", isMoving ? 1.0f : 0.0f);
            }

            if (_playerTransform == null)
            {
                FindPlayer();
                if (_playerTransform == null) return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

            switch (_currentState)
            {
                case EnemyState.Patrol:
                    UpdatePatrol(distanceToPlayer);
                    break;

                case EnemyState.Chase:
                    UpdateChase(distanceToPlayer);
                    break;

                case EnemyState.Attack:
                    UpdateAttack(distanceToPlayer);
                    break;
            }
        }

        private void FindPlayer()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
            else
            {
                var playerHealth = FindFirstObjectByType<Player.PlayerHealth>();
                if (playerHealth != null)
                {
                    _playerTransform = playerHealth.transform;
                }
            }
        }

        private void UpdatePatrol(float distanceToPlayer)
        {
            if (distanceToPlayer <= _detectionRadius)
            {
                SwitchState(EnemyState.Chase);
                return;
            }

            if (_patrolWaypoints == null || _patrolWaypoints.Length == 0) return;

            if (_navAgent != null && _navAgent.isOnNavMesh)
            {
                if (!_navAgent.pathPending && _navAgent.remainingDistance <= _navAgent.stoppingDistance)
                {
                    _patrolTimer += Time.deltaTime;
                    if (_patrolTimer >= _patrolWaitTime)
                    {
                        _patrolTimer = 0f;
                        _currentWaypointIndex = (_currentWaypointIndex + 1) % _patrolWaypoints.Length;
                        MoveToNextWaypoint();
                    }
                }
            }
            else
            {

                Transform wp = _patrolWaypoints[_currentWaypointIndex];
                if (wp != null)
                {
                    Vector3 toWp = wp.position - transform.position;
                    toWp.y = 0f;
                    if (toWp.sqrMagnitude > 0.4f)
                    {
                        Vector3 dir = toWp.normalized;
                        transform.position += dir * _patrolSpeed * Time.deltaTime;
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 6f * Time.deltaTime);
                    }
                    else
                    {
                        _patrolTimer += Time.deltaTime;
                        if (_patrolTimer >= _patrolWaitTime)
                        {
                            _patrolTimer = 0f;
                            _currentWaypointIndex = (_currentWaypointIndex + 1) % _patrolWaypoints.Length;
                        }
                    }
                }
            }
        }

        private void MoveToNextWaypoint()
        {
            if (_patrolWaypoints == null || _patrolWaypoints.Length == 0) return;
            if (_navAgent != null && _navAgent.isOnNavMesh)
            {
                Transform wp = _patrolWaypoints[_currentWaypointIndex];
                if (wp != null)
                {
                    _navAgent.SetDestination(wp.position);
                }
            }
        }

        private void UpdateChase(float distanceToPlayer)
        {
            if (distanceToPlayer > _loseTargetRadius)
            {
                SwitchState(EnemyState.Patrol);
                return;
            }

            if (distanceToPlayer <= _attackRadius)
            {
                SwitchState(EnemyState.Attack);
                return;
            }

            if (_navAgent != null && _navAgent.isOnNavMesh)
            {
                _navAgent.isStopped = false;
                _navAgent.speed = _chaseSpeed;
                _navAgent.SetDestination(_playerTransform.position);
            }
            else
            {

                Vector3 dir = (_playerTransform.position - transform.position).normalized;
                dir.y = 0f;
                transform.position += dir * _chaseSpeed * Time.deltaTime;
                if (dir.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);
                }
            }
        }

        private void UpdateAttack(float distanceToPlayer)
        {
            float maxAttackRange = (_health != null && _health.IsBoss) ? 2.6f : 1.85f;
            if (distanceToPlayer > maxAttackRange && !_isAttacking)
            {
                SwitchState(EnemyState.Chase);
                return;
            }

            Vector3 lookDir = (_playerTransform.position - transform.position).normalized;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude >= 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), 12f * Time.deltaTime);
            }

            if (_navAgent != null && _navAgent.isOnNavMesh && !_isAttacking)
            {
                _navAgent.isStopped = true;
            }

            if (!_isAttacking && Time.time >= _lastAttackTime + _attackCooldown)
            {
                StartCoroutine(PerformAttackRoutine());
            }
        }

        private IEnumerator PerformAttackRoutine()
        {
            _isAttacking = true;
            _lastAttackTime = Time.time;

            if (_animator != null)
            {
                _animator.SetTrigger("Attack");
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.EnemyAttack, transform.position);
            }

            if (_enemyHitbox == null)
            {
                _enemyHitbox = GetComponentInChildren<EnemyHitbox>();
            }

            float windup = 0.35f;
            float elapsed = 0f;
            Vector3 stepDir = transform.forward;
            float stepDist = (_health != null && _health.IsBoss) ? 0.35f : 0.25f;

            while (elapsed < windup)
            {
                elapsed += Time.deltaTime;
                if (_playerTransform != null)
                {
                    Vector3 look = (_playerTransform.position - transform.position).normalized;
                    look.y = 0;
                    if (look.sqrMagnitude > 0.01f)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), 8f * Time.deltaTime);
                        stepDir = transform.forward;
                    }
                }

                float moveDelta = stepDist * (Time.deltaTime / windup);
                if (_navAgent != null && _navAgent.isOnNavMesh)
                {
                    _navAgent.Move(stepDir * moveDelta);
                }
                else
                {
                    transform.position += stepDir * moveDelta;
                }
                yield return null;
            }

            if (_enemyHitbox != null)
            {
                _enemyHitbox.EnableHitbox();
            }

            yield return new WaitForSeconds(0.28f);

            if (_enemyHitbox != null)
            {
                _enemyHitbox.DisableHitbox();
            }

            yield return new WaitForSeconds(0.35f);

            _isAttacking = false;
        }

        private void SwitchState(EnemyState newState)
        {
            if (_currentState == EnemyState.Attack && newState != EnemyState.Attack)
            {
                _isAttacking = false;
            }

            _currentState = newState;

            switch (_currentState)
            {
                case EnemyState.Patrol:
                    if (_navAgent != null && _navAgent.isOnNavMesh)
                    {
                        _navAgent.isStopped = false;
                        _navAgent.speed = _patrolSpeed;
                    }
                    MoveToNextWaypoint();
                    break;

                case EnemyState.Chase:
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySound(SoundType.EnemyDetect, transform.position);
                    }
                    break;

                case EnemyState.Attack:
                    break;
            }
        }

        public void Stun(float duration)
        {
            if (_currentState == EnemyState.Dead) return;
            StopAllCoroutines();
            _isAttacking = false;
            StartCoroutine(StunRoutine(duration));
        }

        private IEnumerator StunRoutine(float duration)
        {
            var prevState = _currentState;
            SwitchState(EnemyState.Stunned);
            _isAttacking = false;

            if (_navAgent != null && _navAgent.isOnNavMesh)
            {
                _navAgent.isStopped = true;
                _navAgent.velocity = Vector3.zero;
            }
            if (_enemyHitbox != null) _enemyHitbox.DisableHitbox();

            if (_health != null)
            {
                _health.FlashColor(new Color(1f, 0.85f, 0.2f), 0.5f);
            }

            Transform visual = transform.childCount > 0 ? transform.GetChild(0) : transform;
            Vector3 visualInitialPos = visual.localPosition;
            Quaternion visualInitialRot = visual.localRotation;

            Vector3 recoilDir = -transform.forward;
            if (_playerTransform != null)
            {
                recoilDir = (transform.position - _playerTransform.position);
                recoilDir.y = 0;
                recoilDir.Normalize();
            }

            float staggerDuration = 0.28f;
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Vector3 targetPos = startPos + recoilDir * 1.6f;

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                targetPos = hit.position;
            }

            while (elapsed < staggerDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / staggerDuration;
                transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0, 1, t));

                float tilt = Mathf.Sin(t * Mathf.PI) * 24f;
                visual.localRotation = visualInitialRot * Quaternion.Euler(-tilt, 0, 0);
                yield return null;
            }

            float remainingStun = Mathf.Max(0.1f, duration - staggerDuration);
            float shakeElapsed = 0f;
            while (shakeElapsed < remainingStun)
            {
                shakeElapsed += Time.deltaTime;
                float wobbleX = Mathf.Sin(shakeElapsed * 40f) * 0.08f;
                float wobbleZ = Mathf.Cos(shakeElapsed * 35f) * 0.06f;
                float tiltAngle = Mathf.Sin(shakeElapsed * 18f) * 10f;

                visual.localPosition = visualInitialPos + new Vector3(wobbleX, 0, wobbleZ);
                visual.localRotation = visualInitialRot * Quaternion.Euler(-10f, tiltAngle, 0);
                yield return null;
            }

            visual.localPosition = visualInitialPos;
            visual.localRotation = visualInitialRot;

            if (_currentState == EnemyState.Stunned)
            {
                if (_navAgent != null && _navAgent.isOnNavMesh)
                {
                    _navAgent.isStopped = false;
                }
                _isAttacking = false;
                _lastAttackTime = Time.time;
                SwitchState(prevState == EnemyState.Stunned ? EnemyState.Chase : prevState);
            }
        }

        private void HandleDamaged()
        {
            if (_currentState != EnemyState.Dead && _playerTransform != null)
            {
                _isAttacking = false;

                SwitchState(EnemyState.Chase);
            }
        }

        private void HandleDeath()
        {
            _currentState = EnemyState.Dead;
            _isAttacking = false;
            StopAllCoroutines();

            if (_enemyHitbox != null)
            {
                _enemyHitbox.DisableHitbox();
            }

            if (_animator != null)
            {
                _animator.SetTrigger("Death");
            }

            if (_navAgent != null && _navAgent.isOnNavMesh)
            {
                _navAgent.isStopped = true;
                _navAgent.enabled = false;
            }
        }
    }
}

