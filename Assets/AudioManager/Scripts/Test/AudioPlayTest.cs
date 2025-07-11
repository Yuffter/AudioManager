using UnityEngine;
using UnityEngine.AddressableAssets;
using Yuffter.AudioManager.BGM;
using Yuffter.AudioManager.Core;
using Yuffter.AudioManager.SE;

public class AudioPlayTest : MonoBehaviour
{
    [SerializeField]
    private AssetReference _testAudioClip;
    private AudioSource _audioSource;
    private AudioPlayer _audioPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioPlayer = new AudioPlayer(_audioSource);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Play();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            BGMManager.Instance.PauseAll();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            BGMManager.Instance.ResumeAll();
        }
    }

    private async void Play()
    {
        BGMManager.Instance.Play(BGMPath.Test, 1f, 1f, true);
    }
}
