using System.Collections.Generic;
using UnityEngine;
using Yuffter.AudioManager;
using Yuffter.AudioManager.Core;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using Yuffter.AudioManager.Settings;

namespace Yuffter.AudioManager.SE
{
    public sealed class SEManager : Core.SingletonMonoBehaviour<SEManager>, Core.IAudioManager
    {
        private List<AudioPlayer> _audioPlayerList = new();
        private Dictionary<string, AudioClip> _audioClipCache = new();
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

        public void Play(string path, float volume = 1f, float pitch = 1f, bool loop = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("Audio path is null or empty.");
                return;
            }
            _Play(path, volume, pitch, loop);
        }
        
        
        public async void _Play(string path, float volume = 1f, float pitch = 1f, bool loop = false)
        {
            AudioClip audioClip = await GetAudioClip(path);
            if (audioClip == null)
            {
                Debug.LogWarning($"Failed to load AudioClip at path: {path}");
                return;
            }

            AudioPlayer audioPlayer = _audioPlayerList.Find(player => !player.IsPlaying());
            if (audioPlayer == null)
            {
                Debug.LogWarning("No available AudioPlayer found. Consider increasing the pool size.");
                return;
            }

            audioPlayer.Play(audioClip, volume, pitch, loop);
        }


        private async Task<AudioClip> GetAudioClip(string path)
        {
            if (_audioClipCache.TryGetValue(path, out AudioClip clip))
            {
                return clip;
            }
            else
            {
                var handle = Addressables.LoadAssetAsync<AudioClip>(path);
                AudioClip result = await handle.Task;
                _audioClipCache[path] = result;
                return result;
            }
        }

        public void Release()
        {
            foreach (var kvp in _audioClipCache)
            {
                Addressables.Release(kvp.Value);
            }
            _audioClipCache.Clear();
        }

        public void Stop(string path)
        {
            /* AudioPlayerのうち、指定されたAudioClipを再生しているものを探す */
            AudioPlayer audioPlayer = _audioPlayerList.Find(player => player.IsPlaying() && player.AudioSource.clip.name == _audioClipCache[path].name);
            if (audioPlayer != null)
            {
                audioPlayer.Stop();
            }
            else
            {
                Debug.LogWarning($"No AudioPlayer found playing the clip at path: {path}");
            }
        }

        public void StopAll()
        {
            foreach (var audioPlayer in _audioPlayerList)
            {
                if (audioPlayer.IsPlaying())
                {
                    audioPlayer.Stop();
                }
            }
        }

        public void PauseAll()
        {
            foreach (var audioPlayer in _audioPlayerList)
            {
                if (audioPlayer.IsPlaying())
                {
                    audioPlayer.AudioSource.Pause();
                }
            }
        }

        public void ResumeAll()
        {
            foreach (var audioPlayer in _audioPlayerList)
            {
                if (!audioPlayer.IsPlaying())
                {
                    audioPlayer.AudioSource.UnPause();
                }
            }
        }

        public void SetVolume(float volume)
        {
            if (volume < 0f || volume > 1f)
            {
                Debug.LogWarning("Volume must be between 0 and 1.");
                return;
            }
            Settings.AudioSettings.Instance.SetSEVolume(volume);
            foreach (var audioPlayer in _audioPlayerList)
            {
                audioPlayer.SetVolume(audioPlayer.BaseVolume * Settings.AudioSettings.Instance.SEVolume);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }
        public void Initialize()
        {
            /* AudioSourceをプールするために生成する */
            _audioPlayerList = new();
            _audioClipCache = new();
            int poolSize = Settings.AudioSettings.Instance.SEAudioSourcePoolSize;
            GameObject seAudioSourceContainer = new GameObject("SEAudioSourceContainer");
            DontDestroyOnLoad(seAudioSourceContainer);
            for (int i = 0; i < poolSize; i++)
            {
                AudioSource audioSource = seAudioSourceContainer.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                AudioPlayer audioPlayer = new AudioPlayer(audioSource);
                _audioPlayerList.Add(audioPlayer);
            }
        }
    }
}