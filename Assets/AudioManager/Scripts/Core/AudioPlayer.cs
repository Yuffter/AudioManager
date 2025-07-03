using UnityEngine;

namespace Yuffter.AudioManager.Core
{
    using Yuffter.AudioManager.Settings;
    public sealed class AudioPlayer
    {
        private readonly AudioSource _audioSource;
        private float _baseVolume = 1f;
        public AudioPlayer(AudioSource audioSource)
        {
            _audioSource = audioSource;
        }

        public bool IsPlaying()
        {
            return _audioSource.isPlaying;
        }

        public void Play(AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false)
        {
            if (clip == null)
            {
                Debug.LogWarning("AudioClip is null, cannot play.");
                return;
            }
            _audioSource.clip = clip;
            SetVolume(volume);
            SetPitch(pitch);
            SetLoop(loop);
            _audioSource.Play();
        }

        public void Stop()
        {
            if (IsPlaying())
            {
                _audioSource.Stop();
            }
        }

        public void SetVolume(float volume)
        {
            if (volume < 0f || volume > 1f)
            {
                Debug.LogWarning("Volume must be between 0 and 1.");
                return;
            }
            _baseVolume = volume;
            _audioSource.volume = _baseVolume;
        }

        public void SetLoop(bool loop)
        {
            _audioSource.loop = loop;
        }

        public void SetPitch(float pitch)
        {
            if (pitch < -3f || pitch > 3f)
            {
                Debug.LogWarning("Pitch must be between 0.1 and 3.");
                return;
            }
            _audioSource.pitch = pitch;
        }

        public async void FadeIn(float duration)
        {
            if (duration <= 0)
            {
                Debug.LogWarning("Duration must be greater than 0.");
                return;
            }

            float startVolume = 0f;
            _audioSource.volume = startVolume;
            _audioSource.Play();

            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                _audioSource.volume = Mathf.Lerp(startVolume, 1f, elapsedTime / duration);
                await System.Threading.Tasks.Task.Yield();
            }
        }

        public async void FadeOut(float duration)
        {
            if (duration <= 0)
            {
                Debug.LogWarning("Duration must be greater than 0.");
                return;
            }

            float startVolume = _audioSource.volume;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                _audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
                await System.Threading.Tasks.Task.Yield();
            }

            _audioSource.Stop();
        }
    }
}