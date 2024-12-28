using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSwitcher : MonoBehaviour
{
    [Header("Target Scene")] 
    [Tooltip("Enter the name of the scene to load when the button is pressed.")]
    public string targetSceneName; // 目标场景名称

    [Header("UI Button")]
    [Tooltip("Drag the UI button here.")]
    public Button sceneSwitchButton; // 按钮引用

    void Start()
    {
        // 检查是否设置了按钮
        if (sceneSwitchButton != null)
        {
            // 绑定按钮点击事件
            sceneSwitchButton.onClick.AddListener(SwitchScene);
        }
        else
        {
            Debug.LogError("Scene switch button is not assigned in the Inspector!");
        }
    }

    // 切换场景
    private void SwitchScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log($"Switching to scene: {targetSceneName}");
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("Target scene name is empty or not set!");
        }
    }
}
