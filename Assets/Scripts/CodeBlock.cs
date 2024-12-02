using UnityEngine;
using UnityEngine.EventSystems;

public class CodeBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform parentContainer;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        parentContainer = transform.parent;
        transform.SetParent(parentContainer.parent);
    }
    public float snapDistance = 1.0f; // 吸附距离
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("CodeBlock"))
        {
            Vector3 offset = other.transform.position - transform.position;

            // 如果距离足够近，自动吸附到对方
            if (offset.magnitude <= snapDistance)
            {
                transform.position = other.transform.position - offset.normalized * snapDistance;
            }
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 如果没有放在有效位置，回到原处
        if (transform.parent == parentContainer.parent)
        {
            transform.SetParent(parentContainer);
        }
    }
}
