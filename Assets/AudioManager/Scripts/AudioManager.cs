using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using System.Linq;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Yuffter.AudioManager
{
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField]
        AudioLibrary _library;

        [SerializeField]
        AudioMixer _mixer;

        [SerializeField]
        AudioMixerGroup _seGroup,
            _bgmGroup;

        [SerializeField]
        int _prewarm = 8;

        public VolumeController Volume { get; private set; }

        AudioPool _pool;
        Dictionary<int, AudioEntry> _byId;

        // クリップキャッシュ（Addressables ハンドル＋参照カウント）
        readonly Dictionary<int, AsyncOperationHandle<AudioClip>> _loaded = new();

        AudioHandle _currentBgm;
        int _currentBgmId = -1;

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _pool = new AudioPool(transform, _prewarm);
            _byId = _library.Entries.ToDictionary(e => e.Id);
            Volume = new VolumeController(_mixer);
        }

        // ---------- 再生 ----------
        public async Awaitable<AudioHandle> PlaySe(Se id, PlayParams? param = null) =>
            await PlayInternal((int)id, _seGroup, param);

        public async Awaitable<AudioHandle> PlayBgm(Bgm id, PlayParams? param = null)
        {
            if (_currentBgmId == (int)id && _currentBgm.IsPlaying)
                return _currentBgm;

            if (_currentBgm.IsValid)
                _currentBgm.FadeOut(1f); // 旧BGMはフェードアウト
            ReleaseClipDeferred(_currentBgmId); // 旧BGMの実体を解放

            var p = param ?? PlayParams.Default;
            if (p.Loop == null)
                p.Loop = true; // BGMは既定でループ
            _currentBgm = await PlayInternal((int)id, _bgmGroup, p);
            _currentBgmId = (int)id;
            return _currentBgm;
        }

        async Awaitable<AudioHandle> PlayInternal(int id, AudioMixerGroup group, PlayParams? param)
        {
            if (!_byId.TryGetValue(id, out var e))
                return default;
            var clip = await LoadClip(e);
            if (clip == null)
                return default;

            var p = param ?? PlayParams.Default;
            if (p.Volume == 0 && param == null)
                p.Volume = e.Volume;
            if (p.Pitch == 0)
                p.Pitch = e.Pitch;

            return _pool.Rent().Play(clip, e, p, group);
        }

        // ---------- 停止系（要件：特定SE/BGMの停止・フェードアウト・再生中確認）----------
        public void StopBgm(float fade = 1f)
        {
            if (_currentBgm.IsValid)
                _currentBgm.FadeOut(fade);
            ReleaseClipDeferred(_currentBgmId);
            _currentBgmId = -1;
        }

        public bool IsBgmPlaying(Bgm id) => _currentBgmId == (int)id && _currentBgm.IsPlaying;

        public void SetMasterVolume(float value) => Volume.SetMaster(value);
        public void SetSeVolume(float value) => Volume.SetSe(value);
        public void SetBgmVolume(float value) => Volume.SetBgm(value);

        // ---------- Addressables ロード／キャッシュ ----------
        async Awaitable<AudioClip> LoadClip(AudioEntry e)
        {
            if (_loaded.TryGetValue(e.Id, out var h))
                return h.IsDone ? h.Result : await h.Task;

            var handle = e.Clip.LoadAssetAsync();
            _loaded[e.Id] = handle;
            await handle.Task;
            return handle.Result;
        }

        void ReleaseClipDeferred(int id)
        {
            if (id < 0 || !_loaded.TryGetValue(id, out var h))
                return;
            _loaded.Remove(id);
            Addressables.Release(h); // BGM切替時に旧クリップを解放
        }

        // SEは鳴らし終わっても基本キャッシュ保持。明示解放はこれで
        public void UnloadSeBank()
        {
            foreach (var e in _library.Entries.Where(e => e.Category == AudioCategory.Se))
                ReleaseClipDeferred(e.Id);
        }

        // よく使うSEの事前ロード
        public async Awaitable Preload(params Se[] ids)
        {
            foreach (var id in ids)
                if (_byId.TryGetValue((int)id, out var e))
                    await LoadClip(e);
        }
    }
}
