using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MaintainAspectRatioForAll : MonoBehaviour
{
    [Header("目标宽高比 (宽 / 高)")]
    public float targetAspectRatio = 16f / 9f;

    [Header("需要对齐的对象")]
    public Transform[] objectsToAlign; // 需要对齐的对象列表
    public Transform canvasTransform; // Canvas 的 Transform

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        // 调整摄像机视口并初始化对象位置
        AdjustCameraViewport();
        AlignWorldObjects();
    }

    void AdjustCameraViewport()
    {
        // 当前屏幕的宽高比
        float windowAspect = (float)Screen.width / Screen.height;

        // 高度缩放因子
        float scaleHeight = windowAspect / targetAspectRatio;

        if (scaleHeight < 1.0f)
        {
            // 如果窗口宽度较大，出现上下黑边
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
        else
        {
            // 如果窗口高度较大，出现左右黑边
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }
    }

    void AlignWorldObjects()
    {
        if (objectsToAlign == null || canvasTransform == null)
            return;

        // 获取 Canvas 的宽高尺寸
        RectTransform canvasRect = canvasTransform.GetComponent<RectTransform>();
        if (canvasRect == null)
        {
            Debug.LogWarning("Canvas Transform does not have a RectTransform!");
            return;
        }

        Vector2 canvasSize = canvasRect.sizeDelta;

        foreach (Transform obj in objectsToAlign)
        {
            if (obj == null) continue;

            // 将对象的屏幕坐标转换为相对于 Canvas 的位置
            Vector3 screenPosition = cam.WorldToScreenPoint(obj.position);

            // 计算屏幕坐标相对于 Canvas 的比例
            Vector2 canvasPosition = new Vector2(
                screenPosition.x / Screen.width * canvasSize.x,
                screenPosition.y / Screen.height * canvasSize.y
            );

            // 设置对象的新世界坐标
            obj.position = cam.ScreenToWorldPoint(new Vector3(canvasPosition.x, canvasPosition.y, obj.position.z));
        }
    }

    void OnPreCull()
    {
        // 渲染前清除屏幕，以显示黑边
        GL.Clear(true, true, Color.black);
    }
}
