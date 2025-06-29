using UnityEngine;

namespace AudioManager.Core
{
    public abstract class AudioManager : SingletonMonoBehaviour<AudioManager>
    {
        protected override void Awake()
        {
            base.Awake();
        }

        public abstract void Play(string path);

        public abstract void Release();

        public abstract void SetVolume(float volume);

        public abstract void Stop(string path);

        public abstract void StopAll();
    }
}