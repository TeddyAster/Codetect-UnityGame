using UnityEngine;
using System.Collections; // 引入协程

public class BGMManager : MonoBehaviour
{
    [Header("BGM Settings")]
    public AudioClip bgmClip; // 当前场景的背景音乐
    public float bgmVolume = 0.5f; // 背景音乐的音量，范围 0 到 1
    private AudioSource audioSource;

    public static BGMManager Instance;

    private AudioClip initialBGMClip; // 保存初始的背景音乐（用于主界面）
    private void Awake()
    {
        // 确保只有一个 BGMManager 实例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 防止切换场景时销毁
        }
        else
        {
            Destroy(gameObject); // 如果有另一个实例，则销毁当前实例
        }

        audioSource = gameObject.AddComponent<AudioSource>(); // 动态添加 AudioSource 组件
        audioSource.loop = true; // 设置为循环播放
        audioSource.volume = bgmVolume; // 设置音量

        // 如果这是第一次初始化，保存初始的 BGM
        if (bgmClip != null && initialBGMClip == null)
        {
            initialBGMClip = bgmClip;
        }

        PlayBGM();
    }

    private void PlayBGM()
    {
        if (bgmClip != null)
        {
            audioSource.clip = bgmClip; // 设置背景音乐
            audioSource.Play(); // 播放背景音乐
        }
        else
        {
            Debug.LogWarning("背景音乐未设置，请在 Inspector 中设置 BGM Clip！");
        }
    }

    public void StopBGM()
    {
        audioSource.Stop(); // 停止背景音乐
    }

    public void SetBGMVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp(volume, 0f, 1f); // 设置音量
    }

    // 渐变效果
    public void CrossFadeBGM(AudioClip newBGM, float fadeDuration)
    {
        StartCoroutine(FadeOutAndIn(newBGM, fadeDuration)); // 启动协程
    }

    private IEnumerator FadeOutAndIn(AudioClip newBGM, float fadeDuration)
    {
        float startVolume = audioSource.volume;

        // 渐出
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        audioSource.Stop(); // 停止当前背景音乐
        audioSource.clip = newBGM; // 设置新的背景音乐
        audioSource.Play(); // 播放新的背景音乐

        // 渐入
        while (audioSource.volume < bgmVolume)
        {
            audioSource.volume += startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }
    }

    // 恢复初始的 BGM（主界面时使用）
    public void RestoreInitialBGM()
    {
        if (initialBGMClip != null)
        {
            bgmClip = initialBGMClip; // 恢复初始背景音乐
            PlayBGM(); // 播放恢复后的背景音乐
        }
        else
        {
            Debug.LogWarning("Initial BGM is not set. Please ensure it's assigned in the Inspector.");
        }
    }
}
