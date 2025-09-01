using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Controls when the Devs panel will show up
/// </summary>
public class DevsFPKCtrl : MonoBehaviour
{
    public GameObject FPKCtrlPanel;
    public Button btnA;
    public Button btnB;
    
    public int countA = 3;
    public int countB = 3;

    private int a = 0;
    private int b = 0;

    private void BtnAClicked()
    {
        a++;
    }
    private void BtnBClicked()
    {
        b++;
    }
    // Start is called before the first frame update
    void Start()
    {
        btnA.onClick.AddListener(BtnAClicked);
        btnB.onClick.AddListener(BtnBClicked);
    }

    private void OnDestroy()
    {
        btnA.onClick.RemoveAllListeners();
        btnB.onClick.RemoveAllListeners();
    }

    // Update is called once per frame
    void Update()
    {
        if (!FPKCtrlPanel.activeSelf && a >= countA && b >= countB)
        {
            FPKCtrlPanel.SetActive(true);
            a = 0;
            b = 0;
        }
    }
}
