using System.Collections.Generic;
using UnityEngine;

namespace Yuffter.AudioManager
{
    public sealed class AudioPool
    {
        readonly Stack<AudioSourceController> _pool = new();
        readonly Transform _root;

        public AudioPool(Transform root, int prewarm)
        {
            _root = root;
            for (int i = 0; i < prewarm; i++)
                _pool.Push(Create());
        }

        AudioSourceController Create()
        {
            var go = new GameObject("AudioSource");
            go.transform.SetParent(_root);
            var c = go.AddComponent<AudioSourceController>();
            c.Init(this);
            go.SetActive(false);
            return c;
        }

        public AudioSourceController Rent()
        {
            var c = _pool.Count > 0 ? _pool.Pop() : Create();
            c.gameObject.SetActive(true);
            return c;
        }

        public void Return(AudioSourceController c)
        {
            c.gameObject.SetActive(false);
            _pool.Push(c);
        }
    }
}
