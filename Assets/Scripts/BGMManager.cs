using UnityEngine;

public class BGMManager : MonoBehaviour
{
    [Header("BGM Settings")]
    public AudioClip bgmClip; // 背景音乐的音效
    public float bgmVolume = 0.5f; // 背景音乐的音量，范围 0 到 1
    private AudioSource audioSource;

    public static BGMManager Instance;

    private void Awake()
    {
        // 确保只有一个 BGMManager 存在
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
}
