using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Yuffter.AudioManager
{
    public enum AudioCategory { Se, Bgm }

    [System.Serializable]
    public sealed class AudioEntry
    {
        public int Id; // enum値, 並び替え対策
        public string Key; // enum名
        public AudioCategory Category;
        public AssetReferenceT<AudioClip> Clip; // 汎用的にどんなアセットでの入れられるAssetReferenceとは違い、型を指定するAssetReferenceTを使うことで、AudioClip以外のアセットを入れられないようにする+型安全を担保

        [Header("デフォルト設定")]
        public float Volume = 1f;
        public float Pitch = 1f;
        public bool Loop;
        [Range(0, 1)] public float SpatialBlend; // 0:2D, 1:3D
    }
    [CreateAssetMenu(fileName = "AudioLibrary", menuName = "Yuffter/AudioManager/Audio Library")]
    public sealed class AudioLibrary : ScriptableObject
    {
        public List<AudioEntry> Entries = new();
        public int NextId = 1;

        private void OnValidate()
        {
            // 追加されたエントリに未割り当てのIDを割り当てる
            foreach (var e in Entries)
            {
                if (e.Id == 0)
                {
                    e.Id = NextId++;
                }
            }
        }
    }
}
