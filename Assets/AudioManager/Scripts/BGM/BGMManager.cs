using UnityEngine;
using System.Collections.Generic;
using Yuffter.AudioManager;

namespace Yuffter.AudioManager.BGM
{
    public sealed class BGMManager : Core.SingletonMonoBehaviour<BGMManager>, Core.IAudioManager
    {
        private Queue<AudioSource> _audioSourceQueue;
        [RuntimeInitializeOnLoadMethod]
        private static void InitializeStatic()
        {
            if (Instance == null)
            {
                GameObject bgmManagerObject = new GameObject("BGMManager");
                DontDestroyOnLoad(bgmManagerObject);
                bgmManagerObject.AddComponent<BGMManager>();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }

        public void Initialize()
        {
            _audioSourceQueue = new();
            int poolSize = Settings.AudioSettings.Instance.BGMAudioSourcePoolSize;
            GameObject bgmAudioSourceContainer = new GameObject("BGMAudioSourceContainer");
            DontDestroyOnLoad(bgmAudioSourceContainer);
            for (int i = 0; i < poolSize; i++)
            {
                AudioSource audioSource = bgmAudioSourceContainer.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = true;
                _audioSourceQueue.Enqueue(audioSource);
            }
        }

        public void Play(string path)
        {
            throw new System.NotImplementedException();
        }

        public void Release()
        {
            throw new System.NotImplementedException();
        }

        public void SetVolume(float volume)
        {
            throw new System.NotImplementedException();
        }

        public void Stop(string path)
        {
            throw new System.NotImplementedException();
        }

        public void StopAll()
        {
            throw new System.NotImplementedException();
        }
    }
}