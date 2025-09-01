//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;

//public class FurnitureScrollViewCtrl : MonoBehaviour, IEndDragHandler, IBeginDragHandler, IDragHandler{
//    public ScrollRect parentScrollRect;

//    void Start(){
//        if(parentScrollRect==null){
//            GameObject parentScrollRectObj=GameObject.Find("Canvas/ScreenManage/MainScreens Scroll View-Content");
//            Debug.Log(parentScrollRectObj);
//            parentScrollRect=parentScrollRectObj.GetComponent<ScrollRect>();
//        }
//    }

//    public void OnEndDrag(PointerEventData eventData){
//        if (parentScrollRect){
//            parentScrollRect.OnEndDrag(eventData);
//        }

//    }

//    public void OnBeginDrag(PointerEventData eventData){
//        if (parentScrollRect){
//            parentScrollRect.OnBeginDrag(eventData);
//        }

//    }

//    public void OnDrag(PointerEventData eventData){
//        if (parentScrollRect){
//            parentScrollRect.OnDrag(eventData);
//        }

//    }
//}