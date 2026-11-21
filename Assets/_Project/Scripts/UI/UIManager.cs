using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonSurvivor.Core;

namespace DungeonSurvivor.UI
{

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("uGUI HUD References (Optional)")]
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private Slider _staminaSlider;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private TextMeshProUGUI _staminaText;
        [SerializeField] private TextMeshProUGUI _objectiveText;
        [SerializeField] private TextMeshProUGUI _interactionPromptText;
        [SerializeField] private TextMeshProUGUI _potionCountText;
        [SerializeField] private GameObject _interactionPromptPanel;

        [Header("uGUI Overlay Panels")]
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private GameObject _defeatPanel;

        private int _currentHealth = 100;
        private int _maxHealth = 100;
        private float _currentStamina = 100f;
        private float _maxStamina = 100f;
        private int _potionCount = 1;
        private string _activeObjective = "Explora la cripta y encuentra la Llave de Bronce.";
        private string _activePrompt = "";
        private bool _showPrompt = false;
        private bool _hasBronzeKey;
        private bool _hasMasterKey;
        private GameState _currentGameState = GameState.Playing;

        private Texture2D _iconPotion;
        private Texture2D _iconKeyBronze;
        private Texture2D _iconKeyMaster;
        private Texture2D _iconInteractE;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            LoadIcons();
        }

        private void LoadIcons()
        {
            if (_iconPotion == null) _iconPotion = LoadTextureFromDisk("Assets/_Project/Textures/UI/Icon_Potion_Health.png");
            if (_iconKeyBronze == null) _iconKeyBronze = LoadTextureFromDisk("Assets/_Project/Textures/UI/Icon_Key_Bronze.png");
            if (_iconKeyMaster == null) _iconKeyMaster = LoadTextureFromDisk("Assets/_Project/Textures/UI/Icon_Key_Master.png");
            if (_iconInteractE == null) _iconInteractE = LoadTextureFromDisk("Assets/_Project/Textures/UI/Icon_Interact_E.png");
        }

        private Texture2D LoadTextureFromDisk(string projectRelativePath)
        {
            try
            {
                string path = projectRelativePath;
                if (!System.IO.Path.IsPathRooted(path))
                {
                    path = System.IO.Path.Combine(Application.dataPath, "..", projectRelativePath);
                }
                path = System.IO.Path.GetFullPath(path);

                if (System.IO.File.Exists(path))
                {
                    byte[] data = System.IO.File.ReadAllBytes(path);
                    Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                    if (tex.LoadImage(data))
                    {
                        tex.wrapMode = TextureWrapMode.Clamp;
                        tex.filterMode = FilterMode.Bilinear;
                        return tex;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[UIManager] Could not load icon at {projectRelativePath}: {ex.Message}");
            }
            return null;
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerHealthChanged += HandleHealthChanged;
            GameEvents.OnPlayerStaminaChanged += HandleStaminaChanged;
            GameEvents.OnPotionCountChanged += HandlePotionChanged;
            GameEvents.OnKeyCollected += HandleKeyCollected;
            GameEvents.OnObjectiveUpdated += HandleObjectiveUpdated;
            GameEvents.OnInteractionPromptChanged += HandlePromptChanged;
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerHealthChanged -= HandleHealthChanged;
            GameEvents.OnPlayerStaminaChanged -= HandleStaminaChanged;
            GameEvents.OnPotionCountChanged -= HandlePotionChanged;
            GameEvents.OnKeyCollected -= HandleKeyCollected;
            GameEvents.OnObjectiveUpdated -= HandleObjectiveUpdated;
            GameEvents.OnInteractionPromptChanged -= HandlePromptChanged;
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void HandleHealthChanged(int current, int max)
        {
            _currentHealth = current;
            _maxHealth = max;

            if (_healthSlider != null)
            {
                _healthSlider.maxValue = max;
                _healthSlider.value = current;
            }

            if (_healthText != null)
            {
                _healthText.text = $"HP: {current}/{max}";
            }
        }

        private void HandleStaminaChanged(float current, float max)
        {
            _currentStamina = current;
            _maxStamina = max;

            if (_staminaSlider != null)
            {
                _staminaSlider.maxValue = max;
                _staminaSlider.value = current;
            }

            if (_staminaText != null)
            {
                _staminaText.text = $"SP: {Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";
            }
        }

        private void HandlePotionChanged(int count)
        {
            _potionCount = count;
            if (_potionCountText != null)
            {
                _potionCountText.text = $"Pociones [Q]: {count}";
            }
        }

        private void HandleKeyCollected(KeyType key)
        {
            if (key == KeyType.BronzeKey) _hasBronzeKey = true;
            if (key == KeyType.CryptMasterKey) _hasMasterKey = true;
        }

        private void HandleObjectiveUpdated(string text)
        {
            _activeObjective = text;
            if (_objectiveText != null)
            {
                _objectiveText.text = text;
            }
        }

        private void HandlePromptChanged(string prompt, bool show)
        {
            _activePrompt = prompt;
            _showPrompt = show;

            if (_interactionPromptPanel != null)
            {
                _interactionPromptPanel.SetActive(show);
            }

            if (_interactionPromptText != null)
            {
                _interactionPromptText.text = prompt;
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            _currentGameState = state;

            if (_pausePanel != null) _pausePanel.SetActive(state == GameState.Paused);
            if (_victoryPanel != null) _victoryPanel.SetActive(state == GameState.Victory);
            if (_defeatPanel != null) _defeatPanel.SetActive(state == GameState.GameOver);
        }

        public void OnResumeClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TogglePause();
            }
        }

        public void OnRestartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartLevel();
            }
        }

        public void OnQuitClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
        }

        private void OnGUI()
        {

            if (_healthSlider != null && _currentGameState == GameState.Playing && !_showPrompt) return;

            GUI.skin.box.fontSize = 11;
            GUI.skin.button.fontSize = 13;

            if (_currentGameState == GameState.Playing)
            {
                DrawCleanHUD();
            }
            else if (_currentGameState == GameState.Paused)
            {
                DrawPauseMenu();
            }
            else if (_currentGameState == GameState.Victory)
            {
                DrawVictoryMenu();
            }
            else if (_currentGameState == GameState.GameOver)
            {
                DrawGameOverMenu();
            }
        }

        private void DrawCleanHUD()
        {

            float startX = 16f;
            float startY = 16f;

            float hpPercent = Mathf.Clamp01(_currentHealth / (float)Mathf.Max(1, _maxHealth));
            DrawBarRect(new Rect(startX, startY, 180f, 10f), hpPercent, new Color(0.85f, 0.15f, 0.15f), new Color(0.12f, 0.03f, 0.03f, 0.9f));

            float staminaPercent = Mathf.Clamp01(_currentStamina / Mathf.Max(1f, _maxStamina));
            DrawBarRect(new Rect(startX, startY + 14f, 150f, 7f), staminaPercent, new Color(0.18f, 0.78f, 0.32f), new Color(0.03f, 0.14f, 0.05f, 0.9f));

            string keyStatus = (_hasBronzeKey ? "<color=#F0C040>[Bronce ✓]</color> " : "") + (_hasMasterKey ? "<color=#50E0F0>[Maestra ✓]</color>" : "");
            if (string.IsNullOrEmpty(keyStatus)) keyStatus = "<color=#888888>Sin llaves</color>";
            GUI.Label(new Rect(startX, startY + 24f, 250f, 20f), $"<size=10>Pociones [Q]: <b>{_potionCount}</b> | {keyStatus}</size>");

            float pad = 12f;
            float objWidth = Mathf.Min(400f, Screen.width * 0.5f);
            float objX = (Screen.width - objWidth) / 2f;
            GUILayout.BeginArea(new Rect(objX, pad, objWidth, 38), GUI.skin.box);
            GUILayout.Label($"<color=#FFD700><b>OBJETIVO:</b></color> {_activeObjective}", GUILayout.Height(24));
            GUILayout.EndArea();

            DrawInteractionCirclePrompt();

            DrawHazardAndAlertPrompt();

            float hintW = 215f;
            float hintH = 46f;
            GUILayout.BeginArea(new Rect(pad, Screen.height - hintH - pad, hintW, hintH), GUI.skin.box);
            GUILayout.Label("<size=9>WASD: Mover | Shift: Correr | Espacio: Saltar\nCtrl: Dash | Clic Izq: Atacar | Clic Der / F: Escudo</size>");
            GUILayout.EndArea();

            DrawInventoryPanel();

            DrawEnemyHealthBars();
        }

        private void DrawEnemyHealthBars()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            var player = FindFirstObjectByType<Player.PlayerController>();
            Vector3 playerOrigin = player != null ? player.transform.position + Vector3.up * 1.5f : cam.transform.position;

            var enemies = FindObjectsByType<Enemy.EnemyHealth>(FindObjectsSortMode.None);
            for (int i = 0; i < enemies.Length; i++)
            {
                var eh = enemies[i];
                if (eh == null || !eh.IsAlive) continue;

                Vector3 enemyCenter = eh.transform.position + Vector3.up * 1.2f;
                Vector3 toEnemy = enemyCenter - playerOrigin;
                float distToPlayer = toEnemy.magnitude;

                float maxDist = eh.IsBoss ? 35f : 25f;
                if (distToPlayer > maxDist) continue;

                RaycastHit[] hits = Physics.RaycastAll(playerOrigin, toEnemy.normalized, distToPlayer - 0.5f, ~0, QueryTriggerInteraction.Ignore);
                bool occludedByWall = false;
                for (int h = 0; h < hits.Length; h++)
                {
                    var col = hits[h].collider;
                    if (col == null) continue;

                    Transform root = col.transform.root;
                    if (root == eh.transform.root) continue;
                    if (root.CompareTag("Player") || col.GetComponentInParent<Player.PlayerHealth>() != null) continue;
                    if (col.GetComponentInParent<Enemy.EnemyHealth>() != null) continue;

                    occludedByWall = true;
                    break;
                }
                if (occludedByWall) continue;

                Vector3 headWorldPos = eh.transform.position + Vector3.up * (eh.IsBoss ? 2.5f : 2.05f);
                Vector3 screenPos = cam.WorldToScreenPoint(headWorldPos);

                if (screenPos.z <= 0.4f) continue;

                float screenX = screenPos.x;
                float screenY = Screen.height - screenPos.y;

                if (screenX < 15f || screenX > Screen.width - 15f || screenY < 15f || screenY > Screen.height - 15f) continue;

                float barW = eh.IsBoss ? 120f : 66f;
                float barH = eh.IsBoss ? 10f : 7f;
                Rect barRect = new Rect(screenX - barW / 2f, screenY, barW, barH);

                float healthPct = Mathf.Clamp01((float)eh.CurrentHealth / eh.MaxHealth);

                GUI.color = new Color(0f, 0f, 0f, 0.95f);
                GUI.DrawTexture(new Rect(barRect.x - 1f, barRect.y - 1f, barW + 2f, barH + 2f), Texture2D.whiteTexture);

                GUI.color = new Color(0.12f, 0.12f, 0.14f, 0.9f);
                GUI.DrawTexture(barRect, Texture2D.whiteTexture);

                Color fillColor = eh.IsBoss ? new Color(1f, 0.45f, 0.05f) : new Color(1f, 0.88f, 0.12f);
                GUI.color = fillColor;
                float fillW = (barW - 2f) * healthPct;
                if (fillW > 0f)
                {
                    GUI.DrawTexture(new Rect(barRect.x + 1f, barRect.y + 1f, fillW, barH - 2f), Texture2D.whiteTexture);
                }

                GUI.color = Color.white;
                GUIStyle nameStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = eh.IsBoss ? 10 : 8,
                    fontStyle = FontStyle.Bold
                };
                string nameLabel = eh.IsBoss ? "<color=#FFB830>GUARDIÁN ARCANO</color>" : "<color=#FFF080>Esqueleto</color>";
                GUI.Label(new Rect(screenX - 75f, screenY - 14f, 150f, 14f), nameLabel, nameStyle);
            }
            GUI.color = Color.white;
        }

        private void DrawInteractionCirclePrompt()
        {
            var active = Interactions.InteractableBase.ActiveInteractable;
            if (active == null || active.HasInteracted) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 worldTarget = active.transform.position + active.InteractionIconWorldOffset;
            Vector3 screenPos = cam.WorldToScreenPoint(worldTarget);

            float pulse = 1f + 0.08f * Mathf.Sin(Time.time * 6f);
            float iconSize = 44f * pulse;

            if (screenPos.z > 0.4f)
            {
                float sx = screenPos.x;
                float sy = Screen.height - screenPos.y;
                Rect iconRect = new Rect(sx - iconSize / 2f, sy - iconSize / 2f, iconSize, iconSize);

                if (_iconInteractE != null)
                {
                    GUI.color = Color.white;
                    GUI.DrawTexture(iconRect, _iconInteractE);
                }
                else
                {
                    DrawFallbackCircleE(iconRect);
                }
            }
            else
            {

                Rect bRect = new Rect((Screen.width - iconSize) / 2f, Screen.height - 68f, iconSize, iconSize);
                if (_iconInteractE != null)
                {
                    GUI.color = Color.white;
                    GUI.DrawTexture(bRect, _iconInteractE);
                }
                else
                {
                    DrawFallbackCircleE(bRect);
                }
            }
        }

        private void DrawFallbackCircleE(Rect rect)
        {
            GUI.color = new Color(0.1f, 0.1f, 0.12f, 0.9f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.85f, 0.2f);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - 2, rect.width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, 2, rect.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - 2, rect.y, 2, rect.height), Texture2D.whiteTexture);

            GUI.color = Color.white;
            GUIStyle eStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            GUI.Label(rect, "E", eStyle);
        }

        private void DrawHazardAndAlertPrompt()
        {
            if (!_showPrompt || string.IsNullOrEmpty(_activePrompt)) return;

            bool isHazard = _activePrompt.IndexOf("Ácido", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                            _activePrompt.IndexOf("Peligro", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                            _activePrompt.IndexOf("Salta", System.StringComparison.OrdinalIgnoreCase) >= 0;

            if (!isHazard && Interactions.InteractableBase.ActiveInteractable != null && !Interactions.InteractableBase.ActiveInteractable.HasInteracted)
            {
                return;
            }

            float bannerW = Mathf.Min(480f, Screen.width * 0.85f);
            float bannerH = 34f;
            float bannerX = (Screen.width - bannerW) / 2f;
            float bannerY = Screen.height - 118f;

            float pulse = isHazard ? 0.75f + 0.25f * Mathf.Sin(Time.time * 8f) : 1f;

            Color boxBg = isHazard
                ? new Color(0.25f * pulse, 0.04f, 0.04f, 0.94f)
                : new Color(0.10f, 0.12f, 0.18f, 0.94f);

            Color borderCol = isHazard
                ? new Color(1f, 0.25f * pulse, 0.2f, 0.98f)
                : new Color(1f, 0.82f, 0.2f, 0.98f);

            Rect r = new Rect(bannerX, bannerY, bannerW, bannerH);
            DrawSlotFrame(r, boxBg, borderCol);

            GUIStyle promptStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };

            string prefix = isHazard ? "<color=#FF4433>⚠ </color>" : "<color=#FFDD44>★ </color>";
            string textColor = isHazard ? "#FFEAEA" : "#FFF7CC";
            GUI.Label(r, $"{prefix}<color={textColor}>{_activePrompt}</color>", promptStyle);
        }

        private void DrawInventoryPanel()
        {
            float slotW = 48f;
            float slotH = 46f;
            float spacing = 6f;
            float startX = Screen.width - slotW - 14f;
            float startY = 16f;

            Rect r1 = new Rect(startX, startY, slotW, slotH);
            bool hasPotions = _potionCount > 0;
            Color bg1 = hasPotions ? new Color(0.35f, 0.08f, 0.08f, 0.9f) : new Color(0.12f, 0.12f, 0.12f, 0.65f);
            Color border1 = hasPotions ? new Color(0.95f, 0.35f, 0.35f) : new Color(0.3f, 0.3f, 0.3f);
            DrawSlotFrame(r1, bg1, border1);

            Rect iconRect1 = new Rect(r1.x + 5f, r1.y + 4f, 38f, 38f);
            if (_iconPotion != null)
            {
                GUI.color = hasPotions ? Color.white : new Color(1f, 1f, 1f, 0.25f);
                GUI.DrawTexture(iconRect1, _iconPotion, ScaleMode.ScaleToFit);
            }

            GUI.color = Color.white;
            GUIStyle countStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerRight,
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };
            GUI.Label(new Rect(r1.x, r1.y, r1.width - 4f, r1.height - 2f), hasPotions ? $"x{_potionCount}" : "<color=#888888>0</color>", countStyle);

            GUIStyle keyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 8,
                fontStyle = FontStyle.Bold
            };
            GUI.Label(new Rect(r1.x + 4f, r1.y + 2f, 20f, 14f), "<color=#FFAAAA>[Q]</color>", keyStyle);

            Rect r2 = new Rect(startX, startY + slotH + spacing, slotW, slotH);
            Color bg2 = _hasBronzeKey ? new Color(0.35f, 0.25f, 0.05f, 0.9f) : new Color(0.12f, 0.12f, 0.12f, 0.65f);
            Color border2 = _hasBronzeKey ? new Color(1f, 0.84f, 0.15f) : new Color(0.3f, 0.3f, 0.3f);
            DrawSlotFrame(r2, bg2, border2);

            Rect iconRect2 = new Rect(r2.x + 5f, r2.y + 4f, 38f, 38f);
            if (_iconKeyBronze != null)
            {
                GUI.color = _hasBronzeKey ? Color.white : new Color(0.4f, 0.4f, 0.4f, 0.25f);
                GUI.DrawTexture(iconRect2, _iconKeyBronze, ScaleMode.ScaleToFit);
            }
            GUI.color = Color.white;
            GUI.Label(new Rect(r2.x + 4f, r2.y + 2f, 20f, 14f), "<color=#FFD700>B</color>", keyStyle);
            if (_hasBronzeKey)
            {
                GUI.Label(new Rect(r2.x, r2.y, r2.width - 4f, r2.height - 2f), "<color=#60FF60>✓</color>", countStyle);
            }

            Rect r3 = new Rect(startX, startY + 2 * (slotH + spacing), slotW, slotH);
            Color bg3 = _hasMasterKey ? new Color(0.05f, 0.28f, 0.38f, 0.9f) : new Color(0.12f, 0.12f, 0.12f, 0.65f);
            Color border3 = _hasMasterKey ? new Color(0f, 0.95f, 1f) : new Color(0.3f, 0.3f, 0.3f);
            DrawSlotFrame(r3, bg3, border3);

            Rect iconRect3 = new Rect(r3.x + 5f, r3.y + 4f, 38f, 38f);
            if (_iconKeyMaster != null)
            {
                GUI.color = _hasMasterKey ? Color.white : new Color(0.4f, 0.4f, 0.4f, 0.25f);
                GUI.DrawTexture(iconRect3, _iconKeyMaster, ScaleMode.ScaleToFit);
            }
            GUI.color = Color.white;
            GUI.Label(new Rect(r3.x + 4f, r3.y + 2f, 20f, 14f), "<color=#00FFFF>M</color>", keyStyle);
            if (_hasMasterKey)
            {
                GUI.Label(new Rect(r3.x, r3.y, r3.width - 4f, r3.height - 2f), "<color=#60FF60>✓</color>", countStyle);
            }
        }

        private void DrawSlotFrame(Rect rect, Color bgCol, Color borderCol)
        {
            GUI.color = bgCol;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

            GUI.color = borderCol;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - 1, rect.width, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, 1, rect.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - 1, rect.y, 1, rect.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawBarRect(Rect rect, float percent, Color fillCol, Color bgCol)
        {
            GUI.color = bgCol;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

            GUI.color = fillCol;
            float fillW = (rect.width - 2f) * percent;
            if (fillW > 0f)
            {
                GUI.DrawTexture(new Rect(rect.x + 1f, rect.y + 1f, fillW, rect.height - 2f), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;
        }

        private void DrawPauseMenu()
        {
            float width = 280;
            float height = 210;
            GUILayout.BeginArea(new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height), GUI.skin.box);
            GUILayout.Label("<size=18><b>JUEGO EN PAUSA</b></size>");
            GUILayout.Space(12);

            if (GUILayout.Button("Reanudar Partida", GUILayout.Height(34)))
            {
                OnResumeClicked();
            }

            GUILayout.Space(6);
            if (GUILayout.Button("Reiniciar Nivel", GUILayout.Height(34)))
            {
                OnRestartClicked();
            }

            GUILayout.Space(6);
            if (GUILayout.Button("Salir del Juego", GUILayout.Height(34)))
            {
                OnQuitClicked();
            }
            GUILayout.EndArea();
        }

        private void DrawVictoryMenu()
        {
            float width = 340;
            float height = 220;
            GUILayout.BeginArea(new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height), GUI.skin.box);
            GUILayout.Label("<size=20><color=#40FF60><b>¡VICTORIA!</b></color></size>");
            GUILayout.Label("¡Has escapado de la Cripta con la Llave Maestra!");
            GUILayout.Space(14);

            if (GUILayout.Button("Jugar de Nuevo", GUILayout.Height(38)))
            {
                OnRestartClicked();
            }

            GUILayout.Space(8);
            if (GUILayout.Button("Salir", GUILayout.Height(34)))
            {
                OnQuitClicked();
            }
            GUILayout.EndArea();
        }

        private void DrawGameOverMenu()
        {
            float width = 320;
            float height = 200;
            GUILayout.BeginArea(new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height), GUI.skin.box);
            GUILayout.Label("<size=20><color=#FF4040><b>HAS MUERTO</b></color></size>");
            GUILayout.Label("Has caído en combate en la cripta.");
            GUILayout.Space(14);

            if (GUILayout.Button("Reintentar Nivel", GUILayout.Height(38)))
            {
                OnRestartClicked();
            }

            GUILayout.Space(8);
            if (GUILayout.Button("Salir", GUILayout.Height(34)))
            {
                OnQuitClicked();
            }
            GUILayout.EndArea();
        }
    }
}

