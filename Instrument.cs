//using System.Collections;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;
//using UnityEngine;
//using UnityEngine.UIElements;

//public class Instrument : MonoBehaviour
//{
//    public float speed;
//    public RectTransform Index;
//    private float currentAngel;
//    // Start is called before the first frame update
//    void Start()
//    {
//        speed = 0;
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        float time = 1f;
//        float count = 0;
//        while (count < time)
//        {
//            count += Time.unscaledDeltaTime;
//            Index.transform.localRotation.Set(0, 0, Mathf.Lerp(speed/360, 1, count / time),0);
//        }
    
//    }

//}
