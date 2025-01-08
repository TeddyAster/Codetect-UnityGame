using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class GifPlayer : MonoBehaviour
{
    public TextAsset gifFile; // 用于拖动 .gif 文件到 Inspector
    public Image targetImage; // 用于显示 GIF 的 Image 组件
    public bool shouldSaveFromWeb = false; // 是否保存来自网络的 GIF
    public bool enableOptimization = true; // 是否启用内存优化
    public bool reverseMode = false; // 是否启用反向播放模式

    private string gifPath; // 临时保存路径

    void Start()
    {
        // 检查目标 Image 是否已设置
        if (targetImage == null)
        {
            Debug.LogError("Target Image is not assigned!");
            return;
        }

        // 检查 GIF 文件
        if (gifFile == null)
        {
            Debug.LogError("Gif file is not assigned!");
            return;
        }

        // 将 GIF 文件写入到临时路径
        gifPath = GetGifPath();
        if (string.IsNullOrEmpty(gifPath))
        {
            Debug.LogError("Failed to generate gif path!");
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

    private string GetGifPath()
    {
        // 确定临时路径
        string tempPath = Path.Combine(Application.persistentDataPath, $"{gifFile.name}.gif");

        try
        {
            // 将 TextAsset 中的内容写入临时路径
            File.WriteAllBytes(tempPath, gifFile.bytes);
            Debug.Log($"GIF saved to: {tempPath}");
            return tempPath;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving GIF file: {e.Message}");
            return null;
        }
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