using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using static BaiduTTS;

public class NLPResponse : MonoBehaviour
{
    public string reply = "New Ambient color has been set to purple";
    private BaiduTTS baiduTTS;
    private AudioPlayer audioPlayer;


    // Start is called before the first frame update
    IEnumerator Start()
    {
        baiduTTS = GetComponent<BaiduTTS>();
        audioPlayer = GetComponent<AudioPlayer>();
        if (baiduTTS != null)
        {
            string resultFilePath = baiduTTS.SendTextToBaiduAPI(reply);
            Debug.Log("Result file path: " + resultFilePath);

            if (audioPlayer != null)
            {
                audioPlayer.PlayAudioClip(resultFilePath);
                // Wait until audio has finished playing
                yield return new WaitUntil(() => !audioPlayer.IsAudioPlaying());

            }
            else
            {
                Debug.Log("AudioPlayer component not found on this GameObject.");
            }
        }
        else
        {
            Debug.Log("BaiduTTS component not found on this GameObject.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
