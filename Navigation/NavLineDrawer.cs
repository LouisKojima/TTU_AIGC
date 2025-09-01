using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NavLineDrawer : MonoBehaviour
{
    [Required]
    public List<LineRenderer> lineRenderers;
    [Required]
    public NavMeshAgent agent;
    public Selectable stopBtn;
    public Transform start, finish;
    public Vector3 targetPos;

    public float lineHeightOffset = 10;
    [MinValue(0)]
    public float offMeshSearchDistance = 20;
    [MinValue(0)]
    public float updateInterval = 0.1f;
    [MinValue(0)]
    public float updateIntervalOffMesh = 0.2f;

    NavMeshPath path;
    Coroutine drawingCoroutine;

    void Start()
    {
        finish = null;

        StopDrawing();
    }

    public void StartDrawing()
    {
        StopDrawing();
        drawingCoroutine = StartCoroutine(Draw());
        if (stopBtn)
            stopBtn.gameObject.SetActive(true);
    }

    public void StopDrawing()
    {
        lineRenderers.ForEach(x => x.positionCount = 0);
        path = new();
        if (drawingCoroutine != null)
            StopCoroutine(drawingCoroutine);
        Debug.Log("Stopped Drawing Navi");
        if (stopBtn)
            stopBtn.gameObject.SetActive(false);
    }

    //void Update()
    //{
    //    Draw();
    //}

    public IEnumerator Draw()
    {
        while (start)
        {
            if (finish != null) targetPos = finish.position;

            WaitForSeconds wait = new(updateInterval);
            if (!NavMesh.CalculatePath(start.position, targetPos, agent.areaMask, path))
            {
                wait = new(updateIntervalOffMesh);
                if (NavMesh.SamplePosition(start.position, out var hit, offMeshSearchDistance, NavMesh.AllAreas))
                    NavMesh.CalculatePath(hit.position, targetPos, agent.areaMask, path);
                else
                    yield return wait;
            }
            DrawPath(path);
            yield return wait;
        }
    }

    public PathCreation.PathCreator pathCreator;

    public void DrawPath(NavMeshPath path)
    {
        int corners = path.corners.Length;
        lineRenderers.ForEach(x => x.positionCount = corners);
        //lineRenderer.SetPosition(0, navMeshAgent.transform.position);

        //if (corners < 2) return;

        for (int i = 0; i < corners; i++)
        {
            Vector3 pos = path.corners[i];
            pos += new Vector3(0, lineHeightOffset, 0);
            lineRenderers.ForEach(x => x.SetPosition(i, pos));
        }
        if (pathCreator)
        {
            pathCreator.bezierPath = new(path.corners.Select(x => pathCreator.transform.worldToLocalMatrix.MultiplyPoint(x)));
            pathCreator.bezierPath.AutoControlLength = 0;
        }
    }
}
