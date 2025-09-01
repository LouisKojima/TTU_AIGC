using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static VehicleStatusData;
/// <summary>
/// Controller for displaying gears on the FPK
/// </summary>
public class GearsCtrl : MonoBehaviour
{
    [FoldoutGroup("Gears Data")]
    public GameObject mark;
    [FoldoutGroup("Gears Data")]
    public GameObject P;
    [FoldoutGroup("Gears Data")]
    public GameObject R;
    [FoldoutGroup("Gears Data")]
    public GameObject N;
    [FoldoutGroup("Gears Data")]
    public GameObject D;
    [FoldoutGroup("Gears Data")]
    public TextMeshProUGUI[] texts;
    [FoldoutGroup("Gears Data")]
    public TextMeshProUGUI DNum;

    //Font sizes
    public int activeSize = 60;
    public int idleSize = 48;

    [EnumToggleButtons]
    [OnValueChanged("UpdateGears")]
    public Transmission transmission;
    [OnValueChanged("UpdateGears")]
    public int number;
    [OnValueChanged("UpdateGears")]
    public bool addOne = false;
    [OnValueChanged("UpdateGears")]
    public bool currentOnly = false;

    public void UpdateGears()
    {
        if (texts[0] == null) OnValidate();
        switchGear(transmission, number);
    }
    /// <summary>
    /// Switch display accordingly
    /// </summary>
    /// <param name="transmission"></param>
    /// <param name="gearNum"></param> 
    public void switchGear(Transmission transmission, int gearNum = 0)
    {
        if (DNum)
        {
            if (transmission != Transmission.D || gearNum < 0)
            {
                DNum.gameObject.SetActive(false);
            }
            else
            {
                DNum.text = gearNum + (addOne ? 1 : 0) + "";
                DNum.gameObject.SetActive(true);
            }
        }
        switch (transmission)
        {
            case Transmission.P:
                mark.transform.position = new Vector3(
                    P.transform.position.x, 
                    mark.transform.position.y, 
                    mark.transform.position.z);
                for (int i = 0; i < texts.Length; i++)
                {
                    if (i == 0) texts[i].fontSize = activeSize;
                    else texts[i].fontSize = idleSize;
                    if (currentOnly) texts[i].gameObject.SetActive(i == 0);
                }
                break;
            case Transmission.R:
                mark.transform.position = new Vector3(
                    R.transform.position.x,
                    mark.transform.position.y,
                    mark.transform.position.z);
                for (int i = 0; i < texts.Length; i++)
                {
                    if (i == 1) texts[i].fontSize = activeSize;
                    else texts[i].fontSize = idleSize;
                    if (currentOnly) texts[i].gameObject.SetActive(i == 1);
                }
                break;
            case Transmission.N:
                mark.transform.position = new Vector3(
                    N.transform.position.x,
                    mark.transform.position.y,
                    mark.transform.position.z);
                for (int i = 0; i < texts.Length; i++)
                {
                    if (i == 2) texts[i].fontSize = activeSize;
                    else texts[i].fontSize = idleSize;
                    if (currentOnly) texts[i].gameObject.SetActive(i == 2);
                }
                break;
            case Transmission.D:
                mark.transform.position = new Vector3(
                    D.transform.position.x,
                    mark.transform.position.y,
                    mark.transform.position.z);
                for (int i = 0; i < texts.Length; i++)
                {
                    if (i == 3) texts[i].fontSize = activeSize;
                    else texts[i].fontSize = idleSize;
                    if (currentOnly) texts[i].gameObject.SetActive(i == 3);
                }
                break;
            default:
                break;
        }
    }

    public void OnValidate()
    {
        texts = new TextMeshProUGUI[4];
        texts[0] = P.GetComponent<TextMeshProUGUI>();
        texts[1] = R.GetComponent<TextMeshProUGUI>();
        texts[2] = N.GetComponent<TextMeshProUGUI>();
        texts[3] = D.GetComponent<TextMeshProUGUI>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGears();
    }
}
