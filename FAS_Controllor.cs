using FAS_BASE;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FAS_Controllor : SerializedMonoBehaviour
{
    [FoldoutGroup("Refs")]
    [FolderPath(ParentFolder = "Resources/Images/FAS/Focus")]
    public string objectPath = "Images/FAS/Focus";

    [FoldoutGroup("Refs")]
    public Image leftImage, midImage, rightImage;

    [FoldoutGroup("Data")]
    [Button]
    public void LoadData()
    {
        if (rightData == null) rightData = new();
        if (midData == null) midData = new();
        if (leftData == null) leftData = new();
        LoadData(PathSide.Left);
        LoadData(PathSide.Mid);
        LoadData(PathSide.Right);
    }
    private void LoadData(PathSide pathSide)
    {
        if (pathSide == PathSide.Right && rightData.Count != 0) { Debug.Log("right already loaded"); return; }
        if (pathSide == PathSide.Mid && midData.Count != 0) { Debug.Log("mid already loaded"); return; }
        if (pathSide == PathSide.Left && leftData.Count != 0) { Debug.Log("left already loaded"); return; }
        foreach (string isActive in new[] { "active", "passive" })
        {
            foreach (StreetObject objectType in Enum.GetValues(typeof(StreetObject)))
            {
                for (int distance = 1; distance <= 57; distance++)
                {
                    LoadObject(objectType, distance, isActive, pathSide);
                }
            }
        }
    }

    [TabGroup("Data/Tabs", "Left")]
    [DictionaryDrawerSettings]
    public Dictionary<string, Sprite> leftData = new();

    [TabGroup("Data/Tabs", "Middle")]
    public Dictionary<string, Sprite> midData = new();

    [TabGroup("Data/Tabs", "Right")]
    public Dictionary<string, Sprite> rightData = new();

    [TitleGroup("Controls")]
    [EnumPaging]
    public StreetObject leftObject, midObject, rightObject;

    [TitleGroup("Controls")]
    [Range(1, 57)]
    public int leftDistance, midDistance, rightDistance;

    [HorizontalGroup("Controls/Actives")]
    [ToggleLeft, LabelWidth(30)]
    public bool leftActive, midActive, rightActive;

    private void Start()
    {
        LoadData();
    }

    private void OnValidate()
    {
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateObject(rightObject, rightDistance, rightActive ? "active" : "passive", PathSide.Right);
        UpdateObject(midObject, midDistance, midActive ? "active" : "passive", PathSide.Mid);
        UpdateObject(leftObject, leftDistance, leftActive ? "active" : "passive", PathSide.Left);
    }

    [HorizontalGroup("Controls/Previews")]
    [PreviewField(Alignment =ObjectFieldAlignment.Center, Height = 80), HideLabel]
    public Sprite leftPrev, midPrev, rightPrev;

    private string targetPath;
    private string keyCode;
    private string distanceString;

    private void LoadObject(StreetObject objectType, int distance, string isActive, PathSide pathSide)
    {
        if (objectType == StreetObject.None) return;
        distanceString = distance < 10 ? "0" + distance : "" + distance;
        keyCode = objectType + "." + distanceString + "." + isActive;
        if (pathSide == PathSide.Mid)
        {
            targetPath = objectPath + "/Objects/" + objectType + "/ACC_" + isActive + "_Distance" + distanceString;
            midData.Add(keyCode, Resources.Load<Sprite>(targetPath) as Sprite);
        }
        else if (pathSide == PathSide.Right)
        {
            targetPath = objectPath + "/Nebenspur_Objects/" + objectType + "/ACC_" + isActive + "_Distance" + distanceString + "_r";
            rightData.Add(keyCode, Resources.Load<Sprite>(targetPath) as Sprite);
        }
        else if (pathSide == PathSide.Left)
        {
            targetPath = objectPath + "/Nebenspur_Objects/" + objectType + "/ACC_" + isActive + "_Distance" + distanceString + "_l";
            leftData.Add(keyCode, Resources.Load<Sprite>(targetPath) as Sprite);
        }
    }

    private void UpdateObject(StreetObject objectType, int distance, string isActive, PathSide pathSide)
    {
        distanceString = distance < 10 ? "0" + distance : "" + distance;
        keyCode = objectType + "." + distanceString + "." + isActive;
        if (pathSide == PathSide.Mid) 
        {
            if (midImage == null) return;
            if (objectType == StreetObject.None || !midData.TryGetValue(keyCode, out midPrev))
            {
                midImage.gameObject.SetActive(false);
            }
            else
            {
                midImage.gameObject.SetActive(true);                
                midImage.sprite = midPrev;
            }
        }
        else if (pathSide == PathSide.Right)
        {
            if (rightImage == null) return;
            if (objectType == StreetObject.None || !rightData.TryGetValue(keyCode, out rightPrev))
            {
                rightImage.gameObject.SetActive(false);
            }
            else
            {
                rightImage.gameObject.SetActive(true);
                rightImage.sprite = rightPrev;
            }
        }
        else if (pathSide == PathSide.Left)
        {
            if (leftImage == null) return;
            if (objectType == StreetObject.None || !leftData.TryGetValue(keyCode, out leftPrev))
            {
                leftImage.gameObject.SetActive(false);
            }
            else
            {
                leftImage.gameObject.SetActive(true);
                leftImage.sprite = leftPrev;
            }
        }
    }
}
