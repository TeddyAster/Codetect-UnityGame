using UnityEngine;
using UnityEngine.SceneManagement; // 用于管理场景

public class SceneBGMManager : MonoBehaviour
{
    [Header("New BGM Settings")]
    public AudioClip newBGMClip; // 要切换的新的背景音乐剪辑
    public float fadeDuration = 2f; // 渐变持续时间（秒）

    void Start()
    {
        // 检查是否已存在 BGMManager 实例
        if (BGMManager.Instance != null)
        {
            // 如果 BGMManager 实例存在，切换到新的 BGM 并播放
            BGMManager.Instance.CrossFadeBGM(newBGMClip, fadeDuration);
        }
        else
        {
            Debug.LogWarning("BGMManager 实例未找到，确保场景中有 BGMManager 跨场景存在！");
        }
    }

    // 当场景切换时恢复初始 BGM（返回主界面时）
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu") // 假设主界面场景的名字是 "MainMenu"
        {
            BGMManager.Instance.RestoreInitialBGM(); // 恢复初始背景音乐
        }
    }

    // 注册场景加载事件
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // 注销场景加载事件
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
