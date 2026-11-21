using System;
using System.Collections.Generic;
using UnityEngine;
using DungeonSurvivor.Core;

namespace DungeonSurvivor.Audio
{
    public enum SoundType
    {
        Footstep,
        PlayerSwing,
        PlayerHit,
        PlayerDodge,
        EnemyDetect,
        EnemyAttack,
        EnemyDeath,
        DoorOpen,
        ChestOpen,
        PotionDrink,
        Victory,
        Defeat
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [System.Serializable]
        public struct SoundEntry
        {
            public SoundType Type;
            public AudioClip Clip;
            [Range(0f, 1f)] public float Volume;
            [Range(0.5f, 1.5f)] public float Pitch;
        }

        [Header("Sound Configurations")]
        [SerializeField] private List<SoundEntry> _soundEntries = new List<SoundEntry>();
        [SerializeField] private int _poolSize = 10;

        private Dictionary<SoundType, SoundEntry> _soundLookup = new Dictionary<SoundType, SoundEntry>();
        private Queue<AudioSource> _sfxSourcePool = new Queue<AudioSource>();
        private AudioSource _musicSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            InitializeSources();
            BuildLookup();
        }

        private void OnEnable()
        {
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void InitializeSources()
        {

            for (int i = 0; i < _poolSize; i++)
            {
                var sfxObj = new GameObject($"SFX_Source_{i}");
                sfxObj.transform.SetParent(transform);
                var src = sfxObj.AddComponent<AudioSource>();
                src.playOnAwake = false;
                _sfxSourcePool.Enqueue(src);
            }

            var musicObj = new GameObject("Music_Source");
            musicObj.transform.SetParent(transform);
            _musicSource = musicObj.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
        }

        private void BuildLookup()
        {
            _soundLookup.Clear();
            foreach (var entry in _soundEntries)
            {
                if (!_soundLookup.ContainsKey(entry.Type))
                {
                    _soundLookup.Add(entry.Type, entry);
                }
            }
        }

        public void PlaySound(SoundType type, Vector3 position = default)
        {
            AudioSource src = GetAvailableSource();
            if (src == null) return;

            if (_soundLookup.TryGetValue(type, out var entry) && entry.Clip != null)
            {
                src.clip = entry.Clip;
                src.volume = entry.Volume > 0f ? entry.Volume : 1f;
                src.pitch = UnityEngine.Random.Range(0.95f, 1.05f) * (entry.Pitch > 0f ? entry.Pitch : 1f);
            }
            else
            {

                src.clip = GenerateProceduralTone(type);
                src.volume = 0.35f;
                src.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            }

            if (position != default)
            {
                src.transform.position = position;
                src.spatialBlend = 1f;
                src.minDistance = 2f;
                src.maxDistance = 25f;
            }
            else
            {
                src.spatialBlend = 0f;
            }

            src.Play();
        }

        private AudioSource GetAvailableSource()
        {
            if (_sfxSourcePool.Count == 0) return null;
            AudioSource src = _sfxSourcePool.Dequeue();
            _sfxSourcePool.Enqueue(src);
            return src;
        }

        private void HandleGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Victory:
                    PlaySound(SoundType.Victory);
                    break;
                case GameState.GameOver:
                    PlaySound(SoundType.Defeat);
                    break;
            }
        }

        private AudioClip GenerateProceduralTone(SoundType type)
        {
            int sampleRate = 44100;
            float duration = 0.15f;
            float baseFreq = 440f;

            switch (type)
            {
                case SoundType.PlayerSwing: baseFreq = 260f; duration = 0.12f; break;
                case SoundType.PlayerHit: baseFreq = 140f; duration = 0.2f; break;
                case SoundType.PlayerDodge: baseFreq = 380f; duration = 0.15f; break;
                case SoundType.EnemyAttack: baseFreq = 180f; duration = 0.2f; break;
                case SoundType.EnemyDeath: baseFreq = 100f; duration = 0.35f; break;
                case SoundType.ChestOpen: baseFreq = 620f; duration = 0.25f; break;
                case SoundType.DoorOpen: baseFreq = 120f; duration = 0.4f; break;
                case SoundType.PotionDrink: baseFreq = 540f; duration = 0.2f; break;
                case SoundType.Victory: baseFreq = 780f; duration = 0.5f; break;
                case SoundType.Defeat: baseFreq = 95f; duration = 0.5f; break;
            }

            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (float)i / sampleCount;
                samples[i] = Mathf.Sin(2 * Mathf.PI * baseFreq * t) * envelope;
            }

            AudioClip clip = AudioClip.Create($"Tone_{type}", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}

