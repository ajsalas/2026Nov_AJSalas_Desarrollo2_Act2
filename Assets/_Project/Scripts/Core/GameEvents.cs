using System;
using UnityEngine;

namespace DungeonSurvivor.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Victory,
        GameOver
    }

    public enum KeyType
    {
        None,
        BronzeKey,
        CryptMasterKey
    }

    public static class GameEvents
    {

        public static event Action<int, int> OnPlayerHealthChanged;
        public static event Action<float, float> OnPlayerStaminaChanged;
        public static event Action<Vector3> OnPlayerDamaged;

        public static event Action<KeyType> OnKeyCollected;
        public static event Action<int> OnPotionCountChanged;
        public static event Action<string> OnObjectiveUpdated;

        public static event Action<string, bool> OnInteractionPromptChanged;

        public static event Action<GameState> OnGameStateChanged;

        public static void TriggerPlayerHealthChanged(int current, int max) => OnPlayerHealthChanged?.Invoke(current, max);
        public static void TriggerPlayerStaminaChanged(float current, float max) => OnPlayerStaminaChanged?.Invoke(current, max);
        public static void TriggerPlayerDamaged(Vector3 hitPoint) => OnPlayerDamaged?.Invoke(hitPoint);
        public static void TriggerKeyCollected(KeyType keyType) => OnKeyCollected?.Invoke(keyType);
        public static void TriggerPotionCountChanged(int count) => OnPotionCountChanged?.Invoke(count);
        public static void TriggerObjectiveUpdated(string objective) => OnObjectiveUpdated?.Invoke(objective);
        public static void TriggerInteractionPrompt(string prompt, bool show) => OnInteractionPromptChanged?.Invoke(prompt, show);
        public static void TriggerGameStateChanged(GameState state) => OnGameStateChanged?.Invoke(state);
    }
}

