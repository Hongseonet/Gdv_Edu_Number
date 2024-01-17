using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    [SerializeField]
    TMP_InputField inpTxt, readTxt;

    [SerializeField]
    Transform btnList;

    bool isThemeDark, isQuit, isTTSPlay;

    int decimalNum; //limit 10

    string muteAudio = "mute_halfsec";

    AudioSource audioSource;
    Queue<AudioClip> queueAudio; //hour min min sec sec


    // Start is called before the first frame update
    void Start()
    {
        isThemeDark = true;
        queueAudio = new Queue<AudioClip>();
        audioSource = this.GetComponent<AudioSource>();

        GetJsonParse();

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

    public void BtnEvent(Button btn)
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

                            if (!strTTS[i].ToString().Equals("0"))
                            {
                                arrTTS.Push(weight.ToString());
                                arrTTS.Push(strTTS[i].ToString());
                                arrTTS.Push(" "); //insert mute 0.5sec
                            }
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
                    if(!arrTTS.Peek().Equals(" "))
                        GetTTS(Path.Combine(filePath + "/TTS/Num/narr_" + arrTTS.Pop() + ".wav"));
                    else
                    {
                        arrTTS.Pop();
                        GetTTS(Path.Combine(filePath + "/TTS/Num/" + muteAudio + ".wav"));
                    }
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
            case "Num":
                //input number manualy
                int numpad;

                if(int.TryParse(btn.name.Split('_')[2], out numpad))
                {
                    string tmpStr = decimalNum.ToString() + numpad.ToString();
                    decimalNum = int.Parse(tmpStr);

                    SetInputField();
                }
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
        //insert comma on each 3 digit
        IEnumerable groups = decimalNum.ToString().Select((c, idx) => new { Char = c, Index = idx })
        .GroupBy(x => x.Index / 3)
        .Select(g => String.Concat(g.Select(x => x.Char)));

        string result = string.Join(",", groups);

        //Debug.Log("red " + result);
        inpTxt.text = result.ToString();

        //read decimal korean
        readTxt.text = "";
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

    void GetJsonParse()
    {
        FileStream fs = new FileStream(Application.streamingAssetsPath + "/Decimal_Read.json", FileMode.Open);
        StreamReader stream = new StreamReader(fs);

        string data = stream.ReadToEnd();

        Debug.Log("raw " + data);

        JsonSerialize abc = JsonUtility.FromJson<JsonSerialize>(data);
        stream.Close();

        //Debug.Log("dd " + abc.arrdecimal.Length);
        //Debug.Log("dd " + abc.arrunit.Length);
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

class JsonSerialize
{
    public string[] arrdecimal; //1 to 9
    public string[] arrunit; //10 to 1000000000000
}