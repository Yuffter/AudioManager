using System.Collections.Generic;
using UnityEngine;

namespace Yuffter.AudioManager.Core
{
    public interface IAudioManager
    {
        void Initialize();
        void Play(string path, float volume = 1f, float pitch = 1f, bool loop = false);
        void Stop(string path);
        void StopAll();
        // void SetVolume(float volume);
        void Release();
        void PauseAll();
        void ResumeAll();
    }
}