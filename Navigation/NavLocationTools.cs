using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(NavLocationTools))]
public class NavLocationToolsEditor : Editor
{
    void OnSceneGUI()
    {
        NavLocationTools myTarget = (NavLocationTools)target;
        if (myTarget.editingLocations)
        {
            if (Event.current.type == EventType.MouseUp)
            {
                Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);


                if (Physics.Raycast(worldRay, out var hitInfo))
                {
                    //myTarget.locations.Add(hitInfo.point);
                    myTarget.navLocations.CreateMark(hitInfo.point);
                }

                //Event.current.Use();
            }
        }
    }
}
#endif //UNITY_EDITOR
public class NavLocationTools : MonoBehaviour
{
    public NavLocations navLocations;
    public bool editingLocations => navLocations.editingLocations;
    public List<NavLocation> locations => navLocations.locations;

    public void Bond(NavLocations nav)
    {
        navLocations = nav;
    }
}
