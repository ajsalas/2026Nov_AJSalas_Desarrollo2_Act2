using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonSurvivor.Core
{

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State Settings")]
        [SerializeField] private GameState _initialState = GameState.Playing;
        [SerializeField] private bool _enableDebugCheats = true;

        [Header("Objective Strings")]
        [SerializeField] private string _initialObjective = "Explora la cripta y encuentra la Llave de Bronce.";

        private GameState _currentState;
        public GameState CurrentState => _currentState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            SanitizeFloatingTorches();
        }

        private void SanitizeFloatingTorches()
        {
            var allGOs = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            for (int i = 0; i < allGOs.Length; i++)
            {
                var go = allGOs[i];
                if (go != null && (go.name == "Wall_Torch" || go.name.Contains("torch_mounted")))
                {
                    Vector3 pos = go.transform.position;
                    if (pos.x > 1.2f && pos.x < 4.5f && pos.z > 14.5f && pos.z < 25.5f)
                    {
                        go.transform.position = new Vector3(2.85f, 2.2f, 11.5f);
                        go.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
                    }
                }
            }
        }

        private void Start()
        {
            SetGameState(_initialState);
            GameEvents.TriggerObjectiveUpdated(_initialObjective);
        }

        private void Update()
        {
            HandlePauseInput();

            if (_enableDebugCheats)
            {
                HandleDebugCheats();
            }
        }

        public void SetGameState(GameState newState)
        {
            _currentState = newState;

            switch (_currentState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    SetCursorState(true);
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    SetCursorState(false);
                    break;

                case GameState.Victory:
                case GameState.GameOver:
                    Time.timeScale = 0f;
                    SetCursorState(false);
                    break;

                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    SetCursorState(false);
                    break;
            }

            GameEvents.TriggerGameStateChanged(_currentState);
        }

        public void TogglePause()
        {
            if (_currentState == GameState.Playing)
            {
                SetGameState(GameState.Paused);
            }
            else if (_currentState == GameState.Paused)
            {
                SetGameState(GameState.Playing);
            }
        }

        public void TriggerVictory()
        {
            if (_currentState == GameState.Victory || _currentState == GameState.GameOver) return;
            SetGameState(GameState.Victory);
        }

        public void TriggerGameOver()
        {
            if (_currentState == GameState.GameOver || _currentState == GameState.Victory) return;
            SetGameState(GameState.GameOver);
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void SetCursorState(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void HandlePauseInput()
        {
            if (InputHelper.IsPausePressed())
            {
                if (_currentState == GameState.Playing || _currentState == GameState.Paused)
                {
                    TogglePause();
                }
            }
        }

        private void HandleDebugCheats()
        {

            if (InputHelper.IsCheatPressed(1))
            {
                var playerHealth = FindFirstObjectByType<Player.PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.Heal(100);
                    Debug.Log("[Cheat F1]: Player Healed to Max.");
                }
            }

            if (InputHelper.IsCheatPressed(2))
            {
                var inventory = FindFirstObjectByType<Player.PlayerInventory>();
                if (inventory != null)
                {
                    inventory.AddKey(KeyType.BronzeKey);
                    inventory.AddKey(KeyType.CryptMasterKey);
                    Debug.Log("[Cheat F2]: Granted BronzeKey & CryptMasterKey.");
                }
            }

            if (InputHelper.IsCheatPressed(3))
            {
                var enemies = FindObjectsByType<Enemy.EnemyHealth>(FindObjectsSortMode.None);
                foreach (var enemy in enemies)
                {
                    enemy.TakeDamage(new DamageInfo(999, enemy.transform.position, Vector3.up, gameObject));
                }
                Debug.Log($"[Cheat F3]: Defeated {enemies.Length} enemies in scene.");
            }

            if (InputHelper.IsCheatPressed(4))
            {
                TriggerVictory();
                Debug.Log("[Cheat F4]: Instant Victory Triggered.");
            }

            if (InputHelper.IsCheatPressed(5))
            {
                TriggerGameOver();
                Debug.Log("[Cheat F5]: Instant Game Over Triggered.");
            }
        }
    }
}

