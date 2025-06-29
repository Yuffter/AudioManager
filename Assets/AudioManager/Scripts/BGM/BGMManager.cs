using UnityEngine;

namespace AudioManager.BGM
{
    public class BGMManager : AudioManager.Core.AudioManager
    {
        public override void Play(string path)
        {
            throw new System.NotImplementedException();
        }

        public override void Release()
        {
            throw new System.NotImplementedException();
        }

        public override void SetVolume(float volume)
        {
            throw new System.NotImplementedException();
        }

        public override void Stop(string path)
        {
            throw new System.NotImplementedException();
        }

        public override void StopAll()
        {
            throw new System.NotImplementedException();
        }

        protected override void Awake()
        {
            base.Awake();
        }
    }
}