using UnityEngine;
using UnityEngine.EventSystems;

public class CodeBlockDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private int blockID;
    private string blockName;

    public void Initialize(int id, string name)
    {
        blockID = id;
        blockName = name;
        Debug.Log($"Initialized Block ID: {blockID}, Name: {blockName}");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"Dragging Block {blockName}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"Dropped Block {blockName}");
    }
}
