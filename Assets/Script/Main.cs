using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI txtTime;

    [SerializeField]
    Button btnClose, btnTTS, btnTheme;

    bool isThemeDark, isQuit, isTTSPlay;

    AudioSource audioSource;
    Queue<AudioClip> queueAudio; //hour min min sec sec


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(QuitApp());
        }

    }

    IEnumerator QuitApp()
    {
        if (isQuit)
        {
            StopAllCoroutines();
            Application.Quit();
        }

        isQuit = !isQuit;
        yield return new WaitForSecondsRealtime(2f); //release after 2 sec
        isQuit = !isQuit;
    }

    void BtnEvent(Button btn)
    {

    }

    //IEnumerator GetTTS(string filePath)
    void GetTTS(string filePath)
    {
        Debug.Log("filePath : " + filePath);

        if (Application.platform.Equals(RuntimePlatform.WindowsEditor))
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning("no file exist");
                //yield break;
                //return;
            }
        }

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.UNKNOWN))
        //using (UnityWebRequest www = UnityWebRequest.Get(filePath))
        {
            //Debug.Log("psf : " + www.url);
            //yield return www.SendWebRequest();
            www.SendWebRequest();

            while (!www.isDone) { }

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                queueAudio.Enqueue(DownloadHandlerAudioClip.GetContent(www));
            }
        }
    }

    IEnumerator PlayTTS()
    {
        if (!isTTSPlay)
        {
            isTTSPlay = true; //toggle

            int loopCnt = queueAudio.Count; //for fix total count
            //Debug.Log("cnt : " + queueAudio.Count);

            for (int i = 0; i < loopCnt; i++)
            {
                audioSource.clip = queueAudio.Dequeue();
                //Debug.Log("length " + i + " / " + audioSource.clip.length);
                audioSource.Play();
                yield return new WaitForSecondsRealtime(audioSource.clip.length);
                audioSource.Stop();
            }
            queueAudio.Clear();

            isTTSPlay = false;
        }
    }

}