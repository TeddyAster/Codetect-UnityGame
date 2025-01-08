using UnityEngine;

public class CanvasToScreenSize : MonoBehaviour
{
    public Canvas targetCanvas;  // 目标Canvas，用来调整大小
    private RectTransform canvasRectTransform;

    void Awake()
    {
        // 获取Canvas的RectTransform组件
        if (targetCanvas == null)
        {
            Debug.LogError("Target Canvas is not assigned.");
            return;
        }

        canvasRectTransform = targetCanvas.GetComponent<RectTransform>();

        if (canvasRectTransform == null)
        {
            Debug.LogError("Target Canvas does not have a RectTransform.");
            return;
        }

        // 调整Canvas大小使其与屏幕大小一致
        AdjustCanvasToScreen();
    }

    void AdjustCanvasToScreen()
    {
        // 获取屏幕的宽度和高度
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // 设置Canvas的宽度和高度为屏幕的宽度和高度
        canvasRectTransform.sizeDelta = new Vector2(screenWidth, screenHeight);

        // 如果Canvas是Screen Space - Camera，确保它的世界坐标被正确设置
        if (targetCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Camera camera = targetCanvas.worldCamera;
            if (camera != null)
            {
                canvasRectTransform.position = camera.transform.position + camera.transform.forward * camera.nearClipPlane;
            }
        }
        else
        {
            // 如果是Screen Space - Overlay模式，直接调整位置
            canvasRectTransform.position = new Vector3(screenWidth / 2f, screenHeight / 2f, 0);
        }
    }

    void Update()
    {
        // 每帧调整Canvas大小，确保在屏幕分辨率变化时更新
        if (canvasRectTransform != null)
        {
            AdjustCanvasToScreen();
        }
    }
}
