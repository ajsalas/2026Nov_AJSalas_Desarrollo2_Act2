using UnityEngine;

namespace DungeonSurvivor.Core
{

    public class AcidLiquidAnimator : MonoBehaviour
    {
        [Header("UV Scrolling")]
        [SerializeField] private Vector2 _scrollSpeed = new Vector2(0.04f, 0.08f);

        [Header("Light & Emission Pulse")]
        [SerializeField] private Light _acidLight;
        [SerializeField] private float _baseIntensity = 2.4f;
        [SerializeField] private float _pulseSpeed = 3.5f;
        [SerializeField] private float _pulseAmount = 0.6f;

        private Renderer _renderer;
        private Material _materialInstance;
        private Vector2 _currentOffset;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _materialInstance = _renderer.material;
            }

            if (_acidLight == null)
            {
                _acidLight = GetComponentInChildren<Light>();
            }
        }

        private void Update()
        {

            _currentOffset += _scrollSpeed * Time.deltaTime;
            _currentOffset.x %= 1f;
            _currentOffset.y %= 1f;

            if (_materialInstance != null)
            {
                if (_materialInstance.HasProperty("_BaseMap"))
                {
                    _materialInstance.SetTextureOffset("_BaseMap", _currentOffset);
                }
                if (_materialInstance.HasProperty("_MainTex"))
                {
                    _materialInstance.SetTextureOffset("_MainTex", _currentOffset);
                }
            }

            if (_acidLight != null)
            {
                float noise = Mathf.PerlinNoise(Time.time * _pulseSpeed, 0f);
                _acidLight.intensity = _baseIntensity + (noise - 0.5f) * _pulseAmount * 2f;
            }
        }

        private void OnDestroy()
        {
            if (_materialInstance != null)
            {
                Destroy(_materialInstance);
            }
        }
    }
}

