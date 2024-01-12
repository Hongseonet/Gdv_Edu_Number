using System;
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
    TMP_InputField inpTxt;

    [SerializeField]
    Transform btnList;

    bool isThemeDark, isQuit, isTTSPlay;

    int decimalNum; //limit 10

    AudioSource audioSource;
    Queue<AudioClip> queueAudio; //hour min min sec sec

    // Start is called before the first frame update
    void Start()
    {
        queueAudio = new Queue<AudioClip>();
        audioSource = this.GetComponent<AudioSource>();

        inpTxt.onValueChanged.AddListener((x) => InputEvernt(inpTxt));
        foreach (Transform btnEle in btnList)
        {
            if(btnEle.GetComponent<Button>() != null)
            {
                Button btn = btnEle.GetComponent<Button>();
                btn.onClick.AddListener(() => BtnEvent(btn));
            }
        }
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
        switch (btn.name.Split('_')[1]) //serialized button name
        {
            case "Close":
                StartCoroutine(QuitApp());
                break;
            case "TTSF": //tts full
                Stack<string> arrTTS = new Stack<string>();
                double weight = 0;
                int loopIdx = 0;
                char[] strTTS = decimalNum.ToString().ToCharArray();

                for(int i = strTTS.Length - 1; i > -1; --i)
                {
                    //Debug.Log("arr " + strTTS[i]);
                    switch (loopIdx)
                    {
                        case 0:
                            arrTTS.Push(strTTS[i].ToString());
                            break;
                        default:
                            for (int z = 0; z < loopIdx; z++)
                                weight = Math.Pow(10, z + 1);
                            arrTTS.Push(weight.ToString());
                            arrTTS.Push(strTTS[i].ToString());
                            break;
                    }
                    loopIdx++;
                }

                loopIdx = arrTTS.Count;
                string filePath = "";

                if (Application.platform.Equals(RuntimePlatform.Android))
                {
                    //filePath = "jar:file://" + Application.dataPath + "!/assets/";
                    filePath = "jar:file://" + Application.streamingAssetsPath;
                }
                else if (Application.platform.Equals(RuntimePlatform.WindowsEditor))
                {
                    filePath = Application.streamingAssetsPath;
                }

                for (int i=0; i<loopIdx; i++)
                {
                    //Debug.Log("final TTS " + arrTTS.Pop());
                    GetTTS(Path.Combine(filePath + "/TTS/Num/narr_" + arrTTS.Pop() + ".wav"));
                }
                StartCoroutine(PlayTTS());

                break;
            case "TTSS": //tts simple
                Debug.Log("not yet");
                break;
            case "Random":
                System.Random rand = new System.Random();
                decimalNum = rand.Next(0, 1000000000);
                SetInputField();
                break;
            case "Theme":

                break;
            case "Plus":
                IncreaseInput(true);
                SetInputField();
                break;
            case "Minus":
                IncreaseInput(false);
                SetInputField();
                break;
            case "Reset":
                inpTxt.text = "0"; //clear to zero
                break;
        }
    }
    void InputEvernt(TMP_InputField inp)
    {
        decimalNum = int.Parse(inp.text.ToString());
        //Debug.Log("dd " + decimalNum);
    }

    void IncreaseInput(bool isIncrease)
    {
        if (isIncrease)
            decimalNum++;
        else if(!isIncrease && decimalNum > 0)
            decimalNum--;
    }

    void SetInputField()
    {
        inpTxt.text = decimalNum.ToString();
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