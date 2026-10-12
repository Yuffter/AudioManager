using System;
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
        bool _bgmLoading;

        // Enter Play Mode Options で Domain Reload を切っているため、
        // static フィールドは明示的に初期化しないと前回実行時の破棄済み参照が残る
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Instance = null;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            _pool = new AudioPool(transform, _prewarm);
            _byId = _library.Entries.ToDictionary(e => e.Id);
            Volume = new VolumeController(_mixer);
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // ---------- 再生 ----------
        public async Awaitable<AudioHandle> PlaySe(Se id, PlayParams? param = null) =>
            await PlayInternal((int)id, _seGroup, param);

        public async Awaitable<AudioHandle> PlayBgm(Bgm id, PlayParams? param = null, float fadeOut = 1f)
        {
            // 毎フレーム呼ばれても鳴り直さない。ロード待ちの間の再要求も弾く
            if (_currentBgmId == (int)id && (_bgmLoading || _currentBgm.IsPlaying))
                return _currentBgm;

            int prevId = _currentBgmId;
            if (_currentBgm.IsValid)
                _currentBgm.FadeOut(fadeOut); // 旧BGMはフェードアウト

            var p = param ?? PlayParams.Default;
            p.Loop ??= true; // BGMは既定でループ

            _currentBgmId = (int)id;
            _bgmLoading = true;
            AudioHandle handle;
            try
            {
                handle = await PlayInternal((int)id, _bgmGroup, p);
            }
            finally
            {
                _bgmLoading = false;
            }

            ReleaseClipAfter(prevId, fadeOut);

            // ロードを待っている間に StopBgm や別BGMへの切り替えが入っていた場合は鳴らさない
            if (_currentBgmId != (int)id)
            {
                handle.Stop();
                return default;
            }

            _currentBgm = handle;
            return _currentBgm;
        }

        async Awaitable<AudioHandle> PlayInternal(int id, AudioMixerGroup group, PlayParams? param)
        {
            if (!_byId.TryGetValue(id, out var e))
            {
                Debug.LogWarning($"[AudioManager] Id={id} のエントリが AudioLibrary にありません。");
                return default;
            }

            var clip = await LoadClip(e);
            if (clip == null)
            {
                Debug.LogWarning(
                    $"[AudioManager] '{e.Key}' のロードに失敗しました。Addressables への登録を確認してください。"
                );
                return default;
            }

            // 未指定（null）の項目はライブラリのエントリ既定値で埋める
            var p = param ?? PlayParams.Default;
            p.Volume ??= e.Volume;
            p.Pitch ??= e.Pitch;
            p.SpatialBlend ??= e.SpatialBlend;
            p.Loop ??= e.Loop;

            return _pool.Rent().Play(clip, p, group);
        }

        // ---------- 停止系（要件：特定SE/BGMの停止・フェードアウト・再生中確認）----------
        public void StopBgm(float fade = 1f)
        {
            if (_currentBgm.IsValid)
                _currentBgm.FadeOut(fade);

            int prevId = _currentBgmId;
            _currentBgm = default;
            _currentBgmId = -1;
            ReleaseClipAfter(prevId, fade);
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

        // フェードアウト中のクリップを解放すると音が途中で消えるため、鳴り終わってから解放する
        async void ReleaseClipAfter(int id, float seconds)
        {
            if (id < 0)
                return;

            try
            {
                await Awaitable.WaitForSecondsAsync(seconds, destroyCancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (id == _currentBgmId)
                return; // 待機中に同じBGMへ戻っていたら解放しない

            ReleaseClipDeferred(id);
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
