using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class NavigationCtrls : MonoBehaviour
{
    [FoldoutGroup("Refs"), Required]
    public NavLocations navLocations;
    [FoldoutGroup("Refs"), Required]
    public NavLineDrawer navDrawer;
    [FoldoutGroup("Refs"), Required]
    public NavListCtrl navList;
    [FoldoutGroup("Refs"), Required]
    public NavCarLink navCar;
    [FoldoutGroup("Refs")]
    public TMP_InputField navSearchField;

    [MinValue(0)]
    public float stoppingDist = 2f;

    private float remainingDistSqr;
    private Vector3 carPosition => navCar.transform.position;

    private void Reset()
    {
        navLocations = FindObjectOfType<NavLocations>();
        navDrawer = FindObjectOfType<NavLineDrawer>();
        navList = FindObjectOfType<NavListCtrl>();
        navCar = FindObjectOfType<NavCarLink>();
    }
    
    [Button]
    public void ApplyDefaultOrder()
    {
        navList.listItems.Sort((a,b) => a.name.CompareTo(b.name));

        for (int i = 0; i < navList.listItems.Count; i++)
        {
            navList.listItems[i].transform.SetSiblingIndex(i);
        }
    }

    [Button]
    public void ApplySearch(string keyword)
    {
        keyword = keyword.Trim();
        if (keyword == "")
        {
            ApplyDefaultOrder();
            return;
        }

        navList.listItems.Sort((a, b) => SearchScore(a.name, keyword).CompareTo(SearchScore(b.name, keyword)));

        for (int i = 0; i < navList.listItems.Count; i++)
        {
            navList.listItems[i].transform.SetSiblingIndex(i);
        }
    }

    public float SearchScore(string input, string keyword)
    {
        string trimmed = TrimCoordinates(input).ToLower();

        if (keyword.Split(
            " ",
            System.StringSplitOptions.RemoveEmptyEntries).Length == 1)
            return trimmed.Split(" ", System.StringSplitOptions.RemoveEmptyEntries)
                .ToList().Select(s => LengthedScore(s, keyword)).Min();
        return LengthedScore(trimmed, keyword);
    }

    public float LengthedScore(string input, string keyword)
    {
        return (float)StringDiff(input, keyword.ToLower()) / input.Length;
    }

    string TrimCoordinates(string name)
    {
        int index = name.LastIndexOf("(");
        if (index == -1) return name;
        return name.Substring(0, index);
    }

    /// <summary>
    /// Compute the distance between two strings.
    /// </summary>
    public static int StringDiff(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        // Step 1
        if (n == 0)
        {
            return m;
        }

        if (m == 0)
        {
            return n;
        }

        // Step 2
        for (int i = 0; i <= n; d[i, 0] = i++)
        {
        }

        for (int j = 0; j <= m; d[0, j] = j++)
        {
        }

        // Step 3
        for (int i = 1; i <= n; i++)
        {
            //Step 4
            for (int j = 1; j <= m; j++)
            {
                // Step 5
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                // Step 6
                d[i, j] = Mathf.Min(
                    Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        // Step 7
        return d[n, m];
    }

    // Start is called before the first frame update
    void Start()
    {
        ApplyDefaultOrder();
    }

    // Update is called once per frame
    void Update()
    {
        remainingDistSqr = (carPosition - navDrawer.targetPos).sqrMagnitude;
        if (remainingDistSqr < stoppingDist * stoppingDist) navDrawer.StopDrawing();
    }
}
