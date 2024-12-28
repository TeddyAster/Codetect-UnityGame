using UnityEngine;
using UnityEngine.UI;

public class GifPlayer : MonoBehaviour
{
    public string gifPath; // GIF 文件路径或 URL
    public Image targetImage; // 用于显示 GIF 的 Image 组件
    public bool shouldSaveFromWeb = false; // 是否保存来自网络的 GIF
    public bool enableOptimization = true; // 是否启用内存优化
    public bool reverseMode = false; // 是否启用反向播放模式

    void Start()
    {
        // 检查目标 Image 是否已设置
        if (targetImage == null)
        {
            Debug.LogError("Target Image is not assigned!");
            return;
        }

        // 检查 GIF 路径
        if (string.IsNullOrEmpty(gifPath))
        {
            Debug.LogError("Gif path is not set!");
            return;
        }

        // 检查播放器是否存在
        if (!ProGifManager.Instance)
        {
            Debug.LogError("ProGifManager instance is missing or not initialized!");
            return;
        }

        // 启用内存优化
        ProGifManager.Instance.SetPlayerOptimization(enableOptimization);

        // 设置反向播放模式（如果需要）
        if (reverseMode)
        {
            ProGifManager.Instance.m_GifPlayer.Reverse();
        }

        // 设置加载回调
        ProGifManager.Instance.SetPlayerOnLoading(progress =>
        {
            Debug.Log($"Loading Progress: {progress * 100}%");
        });

        // 设置播放回调
        ProGifManager.Instance.SetPlayerOnPlaying(gifTexture =>
        {
            if (gifTexture != null)
            {
                Debug.Log("Playing GIF frame.");
            }
            else
            {
                Debug.LogWarning("GifTexture is null during playback.");
            }
        });

        // 播放 GIF
        ProGifManager.Instance.PlayGif(
            gifPath,
            targetImage,
            progress => Debug.Log($"Loading: {progress * 100}%"),
            shouldSaveFromWeb
        );
    }

    public void PauseGif()
    {
        ProGifManager.Instance.PausePlayer();
        Debug.Log("GIF paused.");
    }

    public void ResumeGif()
    {
        ProGifManager.Instance.ResumePlayer();
        Debug.Log("GIF resumed.");
    }

    public void StopGif()
    {
        ProGifManager.Instance.StopPlayer();
        Debug.Log("GIF stopped.");
    }
}
