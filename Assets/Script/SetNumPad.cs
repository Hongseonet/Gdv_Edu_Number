using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetNumPad : MonoBehaviour
{
    Main main;
    void Start()
    {
        main = GameObject.Find("/Script").GetComponent<Main>();
        
        //copy
        for(int i=0; i<12; i++)
        {
            GameObject cpyObj;

            cpyObj = Instantiate(this.transform.GetChild(0).gameObject, this.transform);
            cpyObj.SetActive(true);
            cpyObj.name = "Btn_Num_" + (i + 1);

            if (i == 10)
                cpyObj.name = "Btn_Num_0";

            Button btn = cpyObj.GetComponent<Button>();

            if (i > 8) //bottom lastest 3
            {
                if (i == 10)
                {
                    btn.onClick.AddListener(() => main.BtnEvent(btn));
                    cpyObj.GetComponentInChildren<TextMeshProUGUI>().text = "0";
                }
                else
                {
                    cpyObj.GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Empty");
                    cpyObj.transform.GetChild(0).gameObject.SetActive(false);
                }
            }
            else
            {
                btn.onClick.AddListener(() => main.BtnEvent(btn));
                cpyObj.GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();
            }
        }
    }
}