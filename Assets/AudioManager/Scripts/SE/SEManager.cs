using System.Collections.Generic;
using UnityEngine;
using Yuffter.AudioManager;

namespace Yuffter.AudioManager.SE
{
    public sealed class SEManager : Core.SingletonMonoBehaviour<SEManager>, Core.IAudioManager
    {
        private Queue<AudioSource> _audioSourceQueue;
        [RuntimeInitializeOnLoadMethod]
        private static void InitializeStatic()
        {
            if (Instance == null)
            {
                GameObject seManagerObject = new GameObject("SEManager");
                DontDestroyOnLoad(seManagerObject);
                seManagerObject.AddComponent<SEManager>();
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

        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }
        public void Initialize()
        {
            _audioSourceQueue = new();
            int poolSize = Settings.AudioSettings.Instance.SEAudioSourcePoolSize;
            GameObject seAudioSourceContainer = new GameObject("SEAudioSourceContainer");
            DontDestroyOnLoad(seAudioSourceContainer);
            for (int i = 0; i < poolSize; i++)
            {
                AudioSource audioSource = seAudioSourceContainer.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                _audioSourceQueue.Enqueue(audioSource);
            }
        }
    }
}