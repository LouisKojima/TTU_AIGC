using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class AvatarCamCtrl : MonoBehaviour
{
    public Camera avatarCam;
    [ShowInInspector,PropertyRange(0,2)]
    public float aspect { 
        get { return avatarCam.aspect; }
        set { avatarCam.aspect = value; }
    }

    public void SetCamAspect()
    {
        //avatarCam.aspect
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
