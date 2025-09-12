using PathCreation.Examples;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFollowerCopies : PathFollower
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
        for (int i = 1; i < copies; i++)
        {
            GameObject copy = Instantiate(gameObject, transform.parent);
            copy.GetComponent<PathFollowerCopies>().distanceTravelled += pathCreator.path.length * i / copies;
            copy.GetComponent<PathFollowerCopies>().ApplyPathLocation();
        }
    }

    [Button]
    public void ClearCopies()
    {
        List<GameObject> otherCopies = transform.parent.GetComponentsInChildren<PathFollowerCopies>().Select(x => x.gameObject).Where(x => !x.Equals(gameObject)).ToList();
        otherCopies.ForEach(x => DestroyImmediate(x));
    }

    [OnValueChanged(nameof(ApplyPathLocation))]
    public float distanceTravelled;

    void Start()
    {
        if (pathCreator != null)
        {
            // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
            pathCreator.pathUpdated += OnPathChanged;
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
        transform.SetPositionAndRotation(pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction), pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction));
    }

    // If the path changes during the game, update the distance travelled so that the follower's position on the new path
    // is as close as possible to its position on the old path
    void OnPathChanged()
    {
        distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
    }
}
