using UnityEngine;
using UnityEngine.AddressableAssets;
using Yuffter.AudioManager.Core;

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
            _audioPlayer.FadeOut(1);
        }
    }

    private async void Play()
    {
        var handle = Addressables.LoadAssetAsync<AudioClip>(_testAudioClip);
        AudioClip result = await handle.Task;
        _audioPlayer.Play(result, 1, 1f, true);
    }
}
