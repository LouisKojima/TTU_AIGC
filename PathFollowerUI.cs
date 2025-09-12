using PathCreation;
using PathCreation.Examples;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFollowerUI : PathFollower
{
    //public PathCreator pathCreator;
    //public EndOfPathInstruction endOfPathInstruction;
    //public float speed = 5;
    [MinValue(1)]
    public int copies = 1;

    [Button]
    public void ApplyCopies()
    {
        OnPathChanged();
        ClearCopies();
        ApplyPathLocation();
        for(int i = 1; i < copies; i++)
        {
            GameObject copy = Instantiate(gameObject, transform.parent);
            copy.GetComponent<PathFollowerUI>().distanceTravelled += pathCreator.path.length * i / copies;
            copy.GetComponent<PathFollowerUI>().ApplyPathLocation();
        }
    }

    [Button]
    public void ClearCopies()
    {
        List<GameObject> otherCopies = transform.parent.GetComponentsInChildren<PathFollowerUI>().Select(x => x.gameObject).Where(x => !x.Equals(gameObject)).ToList();
        otherCopies.ForEach(x => DestroyImmediate(x));
    }

    // 使用基类 PathFollower.distanceTravelled，避免与基类同名字段重复被序列化导致的告警。
    // 在编辑器中通过 OnValidate 触发刷新来替代 OnValueChanged 行为。

    void Start()
    {
        if (pathCreator != null)
        {
            // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
            pathCreator.pathUpdated += OnPathChanged;
        }
    }

    void OnValidate()
    {
        if (pathCreator != null)
        {
            ApplyPathLocation();
        }
    }

    void Update()
    {
        if (pathCreator)
        {
            distanceTravelled += speed * Time.deltaTime;

            ApplyPathLocation();
        }
    }

    void ApplyPathLocation()
    {

        Vector3 pathPos = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
        pathPos.z = transform.parent.position.z;

        Quaternion pathRot = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
        Vector3 euler = pathRot.eulerAngles;
        pathRot = Quaternion.Euler(0, 0, -euler.x);

        transform.SetPositionAndRotation(pathPos, pathRot);
    }

    // If the path changes during the game, update the distance travelled so that the follower's position on the new path
    // is as close as possible to its position on the old path
    void OnPathChanged()
    {
        distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
    }
}
