using UnityEngine;

public class Draggable : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 initialPosition;
    private int blockID;

    public void Initialize(int id)
    {
        rectTransform = GetComponent<RectTransform>();
        blockID = id;
        initialPosition = rectTransform.anchoredPosition;
    }

    void OnMouseDown()
    {
        initialPosition = rectTransform.anchoredPosition;
    }

    void OnMouseDrag()
    {
        Vector2 mousePos = Input.mousePosition;
        rectTransform.anchoredPosition = mousePos;
    }

    void OnMouseUp()
    {
        // 可根据需要处理代码块是否回到原位
    }
}
