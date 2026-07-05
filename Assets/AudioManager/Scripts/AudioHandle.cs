using System.Collections.Generic;
using UnityEngine;

namespace Yuffter.AudioManager
{
    public struct PlayParams
    {
        public float Volume,
            Pitch,
            FadeIn,
            Delay,
            SpatialBlend;
        public bool? Loop;
        public Vector3 Position;
        public Transform Follow; // 指定すると追従（移動する音源）

        public static PlayParams Default =>
            new()
            {
                Volume = 1,
                Pitch = 1,
                SpatialBlend = -1,
            };
        // SpatialBlend = -1 はエントリのデフォルト値を使う合図
    }

    // 再生中の1音を指す軽量ハンドル
    public readonly struct AudioHandle
    {
        readonly AudioSourceController _c;
        readonly int _gen;

        public AudioHandle(AudioSourceController c, int gen)
        {
            _c = c;
            _gen = gen;
        }

        /// <summary>
        /// 再生中の音源がまだ有効かどうか
        /// </summary>
        public bool IsValid => _c != null && _c.Generation == _gen;

        /// <summary>
        /// 再生中かどうか
        /// </summary>
        public bool IsPlaying => IsValid && _c.Source.isPlaying;

        /// <summary>
        /// 再生中の音源を停止する
        /// </summary>
        public void Stop()
        {
            if (IsValid)
                _c.StopImmediate();
        }

        /// <summary>
        /// 再生中の音源をフェードアウトして停止する
        /// </summary>
        /// <param name="seconds"></param>
        public void FadeOut(float seconds)
        {
            if (IsValid)
                _c.FadeOutAndRelease(seconds);
        }

        /// <summary>
        /// 再生中の音源のボリュームを変更する
        /// </summary>
        /// <param name="v"></param>
        public void SetVolume(float v)
        {
            if (IsValid)
                _c.Source.volume = v;
        }
    }
}
