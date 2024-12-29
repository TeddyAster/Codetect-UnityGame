using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CustomButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Button Events")]
    public UnityEvent onPress;   // 按下事件
    public UnityEvent onRelease;  // 松开事件
    public UnityEvent onClick;    // 点击事件

    // 按下按钮
    public void OnPointerDown(PointerEventData eventData)
    {
        onPress?.Invoke();
    }

    // 松开按钮
    public void OnPointerUp(PointerEventData eventData)
    {
        onRelease?.Invoke();
    }

    // 点击按钮
    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }
}