using System.Collections.Generic;
using UnityEngine;

namespace Yuffter.AudioManager
{
    public struct PlayParams
    {
        // null は AudioLibrary のエントリ既定値を使う合図。
        // 構造体は new PlayParams() で全フィールドが既定値になるため、
        // 「未指定」を 0 や -1 で表すと無音・強制2Dの事故になる
        public float? Volume;
        public float? Pitch;
        public float? SpatialBlend;
        public bool? Loop;

        public float FadeIn;
        public float Delay;
        public Vector3 Position;
        public Transform Follow; // 指定すると追従（移動する音源）

        /// <summary>全ての値を AudioLibrary の既定値に任せる。</summary>
        public static PlayParams Default => new();
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
