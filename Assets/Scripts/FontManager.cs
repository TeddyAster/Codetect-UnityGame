using TMPro;  // 引用 TextMeshPro 命名空间
using UnityEngine;
using UnityEngine.UI;  // 引用 UI Text 命名空间

public class FontManager : MonoBehaviour
{
    public static Font globalFont;          // 用于存储全局字体
    public static TMP_FontAsset globalTMPFont;  // 用于存储全局 TMP 字体

    void Awake()
    {
        // 加载字体（假设字体存储在 Resources/Fonts 文件夹下）
        globalFont = Resources.Load<Font>("Fonts/DottedSongtiSquareRegular");  // 替换 "YourFontName" 为你的字体名称
        globalTMPFont = Resources.Load<TMP_FontAsset>("Fonts/DottedSongtiSquareRegular");  // 对应 TMP 字体（如果有）

        if (globalFont == null)
        {
            Debug.LogError("Font not found in Resources/Fonts/");
        }

        if (globalTMPFont == null)
        {
            Debug.LogError("TMP Font not found in Resources/Fonts/");
        }

        ApplyFontToUI();
    }

    // 应用全局字体
    void ApplyFontToUI()
    {
        // 获取所有的 Text 组件，并设置字体
        Text[] textComponents = FindObjectsOfType<Text>();
        foreach (Text text in textComponents)
        {
            text.font = globalFont;  // 设置 UI Text 字体
        }

        // 获取所有的 TextMeshProUGUI 组件，并设置字体
        TextMeshProUGUI[] tmpTextComponents = FindObjectsOfType<TextMeshProUGUI>();
        foreach (TextMeshProUGUI tmpText in tmpTextComponents)
        {
            tmpText.font = globalTMPFont;  // 设置 TMP 字体
        }
    }
}
