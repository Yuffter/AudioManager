using System.Collections.Generic;
using UnityEngine;

namespace Yuffter.AudioManager.Core
{
    public interface IAudioManager
    {
        void Initialize();
        void Play(string path);
        void Stop(string path);
        void StopAll();
        void SetVolume(float volume);
        void Release();
    }
}