using UnityEngine;
using UnityEngine.Audio;

namespace Yuffter.AudioManager
{
    public sealed class VolumeController
    {
        readonly AudioMixer _mixer;

        const string MasterParam = "MasterVol";
        const string SeParam = "SeVol";
        const string BgmParam = "BgmVol";

        public float Master {get; private set; } = 1f;
        public float Se {get; private set; } = 1f;
        public float Bgm {get; private set; } = 1f;

        public VolumeController(AudioMixer mixer)
        {
            _mixer = mixer;
            Load();
            Apply();
        }

        public void SetMaster(float value)
        {
            Master = Mathf.Clamp01(value);
            SetDb(MasterParam, Master);
            Save();
        }

        public void SetSe(float value)
        {
            Se = Mathf.Clamp01(value);
            SetDb(SeParam, Se);
            Save();
        }

        public void SetBgm(float value)
        {
            Bgm = Mathf.Clamp01(value);
            SetDb(BgmParam, Bgm);
            Save();
        }

        private void Apply()
        {
            SetDb(MasterParam, Master);
            SetDb(SeParam, Se);
            SetDb(BgmParam, Bgm);
        }

        private void SetDb(string param, float linear)
        {
            float db = linear <= 0.0001f ? -80f : Mathf.Log10(linear) * 20f;
            _mixer.SetFloat(param, db);
        }

        private void Save()
        {
            PlayerPrefs.SetFloat(MasterParam, Master);
            PlayerPrefs.SetFloat(SeParam, Se);
            PlayerPrefs.SetFloat(BgmParam, Bgm);
        }

        private void Load()
        {
            Master = PlayerPrefs.GetFloat(MasterParam, 1f);
            Se = PlayerPrefs.GetFloat(SeParam, 1f);
            Bgm = PlayerPrefs.GetFloat(BgmParam, 1f);
        }
    }
}
