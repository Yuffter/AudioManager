using UnityEngine;

namespace AudioManager.Settings
{
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "AudioManager/AudioSettings", order = 1)]
    public class AudioSettings : ScriptableObject
    {
        private static AudioSettings _instance;
        public static AudioSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<AudioSettings>("AudioSettings");
                    if (_instance == null)
                    {
                        Debug.LogError("AudioSettings not found. Please create an AudioSettings asset.");
                    }
                }
                return _instance;
            }
        }
        [SerializeField, Header("SE音量"), Range(0f, 1f)]
        private float _seVolume;
        public float SEVolume => _seVolume;
        [SerializeField, Header("BGM音量"), Range(0f, 1f)]
        private float _bgmVolume;
        public float BGMVolume => _bgmVolume;

        [SerializeField, Header("SEの同時再生可能数")]
        private int _maxSECount = 10;

        [SerializeField, Header("BGMの同時再生可能数")]
        private int _maxBGMCount = 1;

        public void SetSEVolume(float value)
        {
            _seVolume = Mathf.Clamp01(value);
        }

        public void SetBGMVolume(float value)
        {
            _bgmVolume = Mathf.Clamp01(value);
        }
    }
}
