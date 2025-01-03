using UnityEngine;
using UnityEngine.UI;

public class ApplyCRTShaderToPanel : MonoBehaviour
{
    public Material crtMaterial; // 需要应用的 CRT 材质
    public Image panelImage;     // Panel 的 Image 组件

    void Start()
    {
        if (panelImage != null && crtMaterial != null)
        {
            // 设置材质到 Image 组件
            panelImage.material = crtMaterial;
        }
        else
        {
            Debug.LogError("Panel Image or CRT Material is not assigned.");
        }
    }
}
