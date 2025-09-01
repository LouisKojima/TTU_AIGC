using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NAudio.Wave;
using System.Linq;
using UnityEngine.Networking;

public class AudioPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioClip myAudioClip;
    public string audioFilePath = "";

    void Awake()
    {
        // Here we check for the AudioSource component on the current GameObject
        // If it doesn't exist, we add it
        if (!TryGetComponent<AudioSource>(out audioSource))
        {
            audioSource = this.gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        if (!TryGetComponent<AudioSource>(out audioSource))
        {
            audioSource = this.gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayAudioClip(string path)
    {
        audioFilePath = path;
        StartCoroutine(LoadAudio());
    }

    public bool IsAudioPlaying()
    {
        return audioSource.isPlaying;
    }

    public IEnumerator LoadAudio()
    {
        if (audioFilePath == "")
        {
            Debug.LogError("No audio file path provided.");
            yield break;
        }

        if (audioSource == null)
        {
            Debug.LogError("audioSource is null");
            yield break;
        }

        AudioClip audioClip;

        using (var www = new UnityWebRequest(audioFilePath))
        {
            www.downloadHandler = new DownloadHandlerAudioClip(audioFilePath, AudioType.WAV);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(www.error);
                yield break;
            }

            audioClip = DownloadHandlerAudioClip.GetContent(www);
        }

        audioSource.clip = audioClip;
        yield return new WaitUntil(() => audioClip.loadState == AudioDataLoadState.Loaded); // wait until clip is loaded
        audioSource.Play();
    }


}
