using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;

namespace Yuffter.AudioManager
{
    public sealed class AudioSourceController : MonoBehaviour
    {
        public AudioSource Source { get; private set; }
        public int Generation { get; private set; }

        Transform _follow;
        CancellationTokenSource _fadeCts;
        AudioPool _pool;

        public void Init(AudioPool pool)
        {
            _pool = pool;
            Source = gameObject.AddComponent<AudioSource>();
            Source.playOnAwake = false;
        }

        public AudioHandle Play(
            AudioClip clip,
            AudioEntry e,
            in PlayParams p, // 読み取り専用の参照渡し
            AudioMixerGroup group
        )
        {
            Generation++;
            CancelFade();

            Source.clip = clip;
            Source.pitch = p.Pitch;
            Source.loop = p.Loop ?? e.Loop;
            Source.outputAudioMixerGroup = group;
            Source.spatialBlend = p.SpatialBlend < 0 ? e.SpatialBlend : p.SpatialBlend;

            _follow = p.Follow;
            transform.position = p.Follow ? p.Follow.position : p.Position;

            if (p.FadeIn > 0f)
            {
                Source.volume = 0f;
                _ = FadeTo(p.Volume, p.FadeIn);
            }
            else
                Source.volume = p.Volume;

            Source.PlayDelayed(Mathf.Max(0, p.Delay));

            if (!Source.loop)
                _ = AutoReleaseWhenDone(clip, p);
            return new AudioHandle(this, Generation);
        }

        void LateUpdate()
        {
            if (_follow)
                transform.position = _follow.position;
        }

        async Awaitable FadeTo(float target, float duration)
        {
            CancelFade();
            _fadeCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            var token = _fadeCts.Token;
            try
            {
                float start = Source.volume,
                    t = 0;
                while (t < duration)
                {
                    t += Time.unscaledDeltaTime;
                    Source.volume = Mathf.Lerp(start, target, t / duration);
                    await Awaitable.NextFrameAsync(token);
                }
                Source.volume = target;
            }
            catch (OperationCanceledException) { }
        }

        public async void FadeOutAndRelease(float seconds)
        {
            int gen = Generation;
            await FadeTo(0f, seconds);
            if (Generation == gen)
                Release();
        }

        async Awaitable AutoReleaseWhenDone(AudioClip clip, PlayParams p)
        {
            int gen = Generation;
            float wait = p.Delay + clip.length / Mathf.Max(0.01f, Mathf.Abs(p.Pitch));
            try
            {
                await Awaitable.WaitForSecondsAsync(wait, destroyCancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            if (Generation == gen)
                Release();
        }

        public void StopImmediate()
        {
            CancelFade();
            Release();
        }

        void Release()
        {
            Generation++; // 既存ハンドルを無効化
            CancelFade();
            Source.Stop();
            Source.clip = null;
            _follow = null;
            _pool.Return(this);
        }

        void CancelFade()
        {
            _fadeCts?.Cancel();
            _fadeCts?.Dispose();
            _fadeCts = null;
        }
    }
}
