using UnityEngine;

namespace DungeonSurvivor.Core
{

    [RequireComponent(typeof(Light))]
    public class TorchFlicker : MonoBehaviour
    {
        [Header("Flicker Parameters")]
        [SerializeField] private float _minIntensity = 1.6f;
        [SerializeField] private float _maxIntensity = 2.8f;
        [SerializeField] private float _flickerSpeed = 6.0f;

        private Light _light;
        private float _noiseOffset;

        private void Awake()
        {
            _light = GetComponent<Light>();
            _noiseOffset = Random.Range(0f, 100f);

            Transform rootTorch = transform.parent != null ? transform.parent : transform;
            Vector3 p = rootTorch.position;
            if (p.x > 1.2f && p.x < 4.5f && p.z > 14.5f && p.z < 25.5f)
            {
                rootTorch.position = new Vector3(2.85f, 2.2f, 11.5f);
                rootTorch.rotation = Quaternion.Euler(0f, -90f, 0f);
            }
        }

        private void Update()
        {
            if (_light == null) return;
            float noise = Mathf.PerlinNoise((Time.time * _flickerSpeed) + _noiseOffset, 0f);
            _light.intensity = Mathf.Lerp(_minIntensity, _maxIntensity, noise);
        }
    }
}

