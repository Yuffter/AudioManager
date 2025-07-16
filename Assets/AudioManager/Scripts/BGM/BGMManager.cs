using UnityEngine;
using System.Collections.Generic;
using Yuffter.AudioManager;
using Yuffter.AudioManager.Core;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using Yuffter.AudioManager.Settings;

namespace Yuffter.AudioManager.BGM
{
    public sealed class BGMManager : Core.SingletonMonoBehaviour<BGMManager>, Core.IAudioManager
    {
        private List<AudioPlayer> _audioPlayerList = new();
        private Dictionary<string, AudioClip> _audioClipCache = new();
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
            /* AudioSourceをプールするために生成する */
            _audioPlayerList = new();
            _audioClipCache = new();
            int poolSize = Settings.AudioSettings.Instance.BGMAudioSourcePoolSize;
            GameObject bgmAudioSourceContainer = new GameObject("BGMAudioSourceContainer");
            DontDestroyOnLoad(bgmAudioSourceContainer);
            for (int i = 0; i < poolSize; i++)
            {
                AudioSource audioSource = bgmAudioSourceContainer.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = true; // BGMはループ再生
                AudioPlayer audioPlayer = new AudioPlayer(audioSource);
                _audioPlayerList.Add(audioPlayer);
            }
        }

        /// <summary>
        /// 指定したBGMを再生する
        /// </summary>
        /// <param name="path">BGMのパス</param>
        /// <param name="volume">音量</param>
        /// <param name="pitch">ピッチ</param>
        /// <param name="loop">ループ再生フラグ</param>
        public void Play(string path, float volume = 1f, float pitch = 1f, bool loop = true)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("Audio path is null or empty.");
                return;
            }
            // BGMはループ再生なので、loopは常にtrue
            _Play(path, volume, pitch, loop);
        }

        public async void _Play(string path, float volume = 1f, float pitch = 1f, bool loop = true)
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

        /// <summary>
        /// キャッシュしているAudioClipを解放する
        /// </summary>
        public void Release()
        {
            foreach (var kvp in _audioClipCache)
            {
                Addressables.Release(kvp.Value);
            }
            _audioClipCache.Clear();
        }

        /// <summary>
        /// 指定したBGMを停止する
        /// </summary>
        /// <param name="path">BGMのパス</param>
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

        /// <summary>
        /// 全てのBGMを停止する
        /// </summary>
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

        /// <summary>
        /// 全てのBGMを一時停止する
        /// </summary>
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

        /// <summary>
        /// 全てのBGMを再開する
        /// </summary>
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

        /// <summary>
        /// 音量を設定する
        /// </summary>
        /// <param name="volume">音量</param>
        public void SetVolume(float volume)
        {
            if (volume < 0f || volume > 1f)
            {
                Debug.LogWarning("Volume must be between 0 and 1.");
                return;
            }
            Settings.AudioSettings.Instance.SetBGMVolume(volume);
            foreach (var audioPlayer in _audioPlayerList)
            {
                audioPlayer.SetVolume(audioPlayer.BaseVolume * Settings.AudioSettings.Instance.BGMVolume);
            }
        }
    }
}