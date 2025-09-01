using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public string input;
    public string key;
    public string[] resultDef;
    public System.StringSplitOptions option;
    public string[] result;

    [Button]
    public void Split()
    {
        resultDef = input.Split(key);
        result = input.Split(key, option);
    }

    public List<int> indices;

    [Button]
    public void Indices()
    {
        indices.Clear();
        for (int i = input.IndexOf(key); i > -1; i = input.IndexOf(key, i + 1))
        {
            // for loop end when i=-1 ('a' not found)
            indices.Add(i);
        }
    }

    //    public NavMeshAgent navMeshAgent;
    //    public Transform target;
    //    [Button]
    //    public void GoToTarget()
    //    {
    //        navMeshAgent.SetDestination(target.position);
    //    }

    public MeshFilter meshFilter;
    public MeshCollider meshCollider;

    [Button]
    public void GetMeshFromNav()
    {
        var navMesh = NavMesh.CalculateTriangulation();
        Mesh mesh = new();
        mesh.vertices = navMesh.vertices;
        mesh.triangles = navMesh.indices;

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    //public Material material;
    //[ShowInInspector]
    //string[] names = null;

    //[Button]
    //public void DoTest()
    //{
    //    //names = material.shader.keywordSpace.keywords.ToList()
    //    //    //.Where(x => x.type.GetType() == typeof(Color))
    //    //    .Select(x => x.name).ToArray();
    //}

    //[Button]
    //public void GetColor(string name)
    //{
    //    Debug.Log(material.GetColor(name));
    //}



    //public HorizontalLayoutGroup layoutGroup;
    //public RectTransform rectTransform;

    //[Button]
    //public void DoTest(int x)
    //{
    //    rectTransform.SetSiblingIndex(x);
    //}




    //[ShowInInspector]
    //public RangeInt range = new();
    //[ShowInInspector]
    //public int end => range.end;



    //public RectMask2D rectMask;

    //[ShowInInspector]
    //public Vector4 vector => rectMask.padding;






    //public Vector2 toDisplay;
    //public Vector2 changed;
    //public Vector2 changedDist;
    //public void DisplayVector2(Vector2 input)
    //{
    //    toDisplay = input;
    //    changed = scrollRect.content.pivot - scrollRect.normalizedPosition;
    //    changedDist = changed * scrollRect.content.rect.size * scrollRect.content.localScale;
    //    Debug.Log(input + ";" + changed + ";" + changedDist);
    //}
    //public ScrollRect scrollRect;
    //[Button]
    //public void SetMapPivot()
    //{
    //    scrollRect.content.pivot = scrollRect.normalizedPosition;
    //    scrollRect.content.localPosition -= new Vector3(changedDist.x, changedDist.y);
    //} 









    //public void ShowConfirmCancelMessageBox()
    //{//参数：（text-提示信息，UnityAction-按下确认后执行的操作，UnityAction-按下取消后执行的操作，bool-激活关闭按钮）
    //    MessageBox.instance.ShowMessageBox("Popup-确认、取消", new UnityEngine.Events.UnityAction(() => { print("我点了确认"); }), new UnityEngine.Events.UnityAction(() => { print("我点了取消"); }),true);
    //}
    //public void ShowConfirmMessageBox()
    //{
    //    MessageBox.instance.ShowMessageBox("Popup-确认", new UnityEngine.Events.UnityAction(() => { print("我点了确认"); }));
    //}
    //public void ShowMessageBox()
    //{
    //    MessageBox.instance.ShowMessageBox("Popup-1S自动消失", 1f);
    //}

    //public int n = 100;
    //public int fpn = 10;
    //public int d = 5;

    //[Button]
    //public void DoTest()
    //{
    //    int thisN = n;
    //    int lastD = 0;
    //    result.Clear();
    //    int day = 0;
    //    int food = 0;
    //    while (thisN > 0)
    //    {
    //        int currentn = thisN;

    //        if (day - lastD >= d) food = 0;

    //        result.Add("Day " + day + ": num: " + thisN + ", food:" + food + ", lastD: " + lastD);
    //        bool thisX = false;
    //        for(int check = 1; check <= currentn; check++)
    //        {
    //            string x = " ";
    //            if (food <= 0)
    //            {
    //                thisN--;
    //                food += 10;
    //                x = "X";
    //                thisX = true;
    //            }
    //            else
    //            {
    //                food--;
    //            }
    //            //result.Add(check + x + ": num: " + thisn + ", food:" + food);
    //        }
    //        if (thisX) lastD = day;
    //        day++;
    //        //if (day >= 1) break;
    //    }

    //    result.Add("Day " + day + ": num: " + thisN + ", food:" + food + ", lastD: " + lastD);
    //}

    //public List<string> result = new();
}
