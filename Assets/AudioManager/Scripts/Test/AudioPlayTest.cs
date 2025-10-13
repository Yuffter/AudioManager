using UnityEngine;
using UnityEngine.SceneManagement;
using Yuffter.AudioManager.BGM;

public class AudioPlayTest : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Play();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            SceneManager.LoadScene("Test");
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            BGMManager.Instance.ResumeAll();
        }
    }

    private async void Play()
    {
        BGMManager.Instance.Play(BGMPath.Test, 1f, 1f, true, true);
    }
}
