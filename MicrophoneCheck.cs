using Sirenix.OdinInspector;
using UnityEngine;

public class MicrophoneCheck : MonoBehaviour
{
    public bool playback;
    public int length = 1;
    public AudioSource aud;
    [Button]
    void StartTest()
    {
        // Log all available microphone devices
        foreach (var device in Microphone.devices)
        {
            Debug.Log("Microphone device: " + device);
        }

        if (Microphone.devices.Length > 0)
        {
            // Start recording using the first available microphone device
            string deviceName = Microphone.devices[0];
            AudioClip audioClip = Microphone.Start(deviceName, true, 1, 16000);
            if (audioClip == null)
            {
                Debug.LogError("Failed to start recording with microphone: " + deviceName);
            }
            else
            {
                Debug.Log("Recording started with microphone: " + deviceName);
                if(playback && aud != null)
                {
                    aud.clip = Microphone.Start(null, true, length, 4400);
                    aud.loop = true;
                    while (!(Microphone.GetPosition(null) > 0)) { }
                    aud.Play();
                }
            }
        }
        else
        {
            Debug.LogError("No microphone devices found.");
        }
    }

    [Button]
    void Stop()
    {
        Microphone.End(null);
        if (aud != null)
        {
            aud.Stop();
        }
    }

    [Button]
    void GetMicPos()
    {
        Debug.Log("MicPos: " + Microphone.GetPosition(null));
        Debug.Log("ClipPos: " + aud.clip.length + "");
    }
}
