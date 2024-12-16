using UnityEngine;
using UnityEngine.EventSystems;

public class CodeBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;

    public float snapDistance = 50f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        originalParent = transform.parent;
        transform.SetParent(originalParent.parent);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        GameObject closestTarget = FindClosestTarget();
        if (closestTarget != null)
        {
            transform.SetParent(closestTarget.transform);
            rectTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
            transform.SetParent(originalParent);
        }
    }

    private GameObject FindClosestTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("CodeBlockTarget");
        GameObject closest = null;
        float minDistance = float.MaxValue;

        foreach (GameObject target in targets)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < minDistance && distance <= snapDistance)
            {
                minDistance = distance;
                closest = target;
            }
        }

        return closest;
    }
}
