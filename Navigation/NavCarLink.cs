using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavCarLink : MonoBehaviour
{
    public RCC_CarControllerV3 linkedCar;
    public Transform referenceWorld;
    public Transform referenceMap;
    [ShowInInspector]
    Vector3 bias;

    // Start is called before the first frame update
    [Button]
    void Start()
    {
        bias = referenceMap.position - referenceWorld.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!referenceMap || !referenceWorld) return;
        if (!linkedCar) return;

        Vector3 position = linkedCar.transform.position + bias;
        Quaternion rotation = linkedCar.transform.localRotation;
        rotation.x = 0;
        rotation.z = 0;

        transform.SetPositionAndRotation(position, rotation);
    }

    public void UpdateLinkedCar()
    {
        linkedCar = null;

        RCC_CarControllerV3[] activeVehicles = FindObjectsOfType<RCC_CarControllerV3>();

        if (activeVehicles == null || activeVehicles.Length == 0) 
            return;

        foreach (RCC_CarControllerV3 rcc in activeVehicles)
        {
            if (!rcc.AIController && rcc.canControl)
            {
                linkedCar = rcc;
                return;
            }
        }
    }
}
