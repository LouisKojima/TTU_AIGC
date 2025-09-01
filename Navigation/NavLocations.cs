using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Sirenix.Utilities;
using System.Linq;

[ExecuteInEditMode]
public class NavLocations : MonoBehaviour
{
    public NavLocationTools navTools;
    public bool editingLocations = false;
#if UNITY_EDITOR
    [OnCollectionChanged(nameof(CreateMarks))]
#endif //UNITY_EDITOR
    public List<NavLocation> locations;
    [MinValue(0)]
    public float gizmoSize = 1;

    private void Reset()
    {
        if (!TryGetComponent(out navTools))
            navTools = gameObject.AddComponent<NavLocationTools>();

        navTools.Bond(this);
    }

    private void OnDestroy()
    {
        Debug.Log("destroyed");
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.blue;
        //Gizmos.DrawSphere(transform.position, 1);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        locations.ForEach(l => Gizmos.DrawSphere(l, gizmoSize));
    }

    public Transform marksParent;

    public void CreateMark(NavLocation nav)
    {
        GameObject go = new(nav.name, typeof(NavLocationMark));
        go.transform.parent = marksParent;
        go.transform.position = nav;
        var mark = go.GetComponent<NavLocationMark>();
        mark.title = nav.name;

        nav.mark = mark;
        if (!locations.Contains(nav)) locations.Add(nav);
    }

    [Button]
    public void CreateMarks()
    {
        if (!marksParent) return;
        var children = marksParent.GetComponentsInChildren<NavLocationMark>();
        children
            .Where(x => x.transform.IsChildOf(marksParent))
            .Where(x =>
                locations.All(y => y.mark != x.GetComponent<NavLocationMark>()))
            .ForEach(x => DestroyImmediate(x.gameObject));

        locations
            .Where(x => x.mark == null)
            .ForEach(x => CreateMark(x));
    }
}

[Serializable]
public struct NavLocation
{
    [ShowInInspector]
    public Vector3 position
    {
        get
        {
            if (!mark) return _position;
            return mark.transform.position;
        }
        set
        {
            if (!mark) _position = value;
            else mark.transform.position = value;
        }
    }
    private Vector3 _position;
    [ShowInInspector]
    public string name
    {
        get
        {
            if (!mark) return _name;
            return mark.title;
        }
        set
        {
            if (!mark) _name = value;
            else mark.title = value;
        }
    }
    private string _name;
    public NavLocationMark mark;

    public NavLocation(Vector3 p)
    {
        _position = p;
        _name = "Unnamed";
        mark = null;
    }
    public NavLocation(NavLocationMark p)
    {
        _position = p.transform.position;
        _name = p.title;
        mark = p;
    }

    public override string ToString()
    {
        return name + " " + position.ToString();
    }

    internal void Deconstruct(out Vector3 p, out string n, out NavLocationMark v)
    {
        p = position;
        n = name;
        v = mark;
    }

    public static implicit operator Vector3(NavLocation v)
    {
        return v.position;
    }

    public static implicit operator NavLocation(Vector3 v)
    {
        return new(v);
    }

    public static implicit operator NavLocationMark(NavLocation v)
    {
        return v.mark;
    }

    public static implicit operator NavLocation(NavLocationMark v)
    {
        return new(v);
    }
}