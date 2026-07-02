using UnityEngine;
using Yuffter.AudioManager;

public class PlayTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await AudioManager.I.PlaySe(Se.Test);
        Debug.Log("SE再生完了");
        AudioManager.I.PlayBgm(Bgm.Test, PlayParams.Default);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
