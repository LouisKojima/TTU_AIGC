using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavTest : MonoBehaviour
{
    [InlineEditor]
    public NavLocations navLocations;
    public NavMeshAgent navMeshAgent;
    public Transform target;
    [Button]
    public void GoToTarget()
    {
        navMeshAgent.SetDestination(target.position);
    }

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
}
