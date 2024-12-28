using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100; // 最大血量
    public int minHealth = 0;   // 最小血量
    public int healthGainOnPresetMatch = 10; // 匹配 Preset 后增加的血量
    public int healthLossPerMistake = 10; // 每次错误扣血量
    public int healthLossOverTime = 10; // 定时扣血量
    public int currentHealth; // 当前血量

    [Header("Timer Settings")]
    public float timeInterval = 10f; // 每次扣血的时间间隔（秒）
    private float countdownTimer; // 倒计时时间

    [Header("UI Components")]
    public Slider healthSlider; // 血量进度条
    public TextMeshProUGUI healthPercentageText; // 血量百分比显示
    public TextMeshProUGUI timerText; // 倒计时显示

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip healthLossClip;
    public AudioClip healthGainClip; // 增加一个健康增加的音效

    [Header("Game Over Scene")]
    public string gameOverSceneName = "GameOver";

    public static HealthManager Instance;

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ResetState(); // 启动时初始化
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 场景加载完成后重新绑定 UI 引用并重置倒计时
        RebindUIReferences();
        ResetCountdownTimer();
    }

    public void ResetState()
    {
        Debug.Log("Resetting HealthManager state.");
        currentHealth = maxHealth; // 恢复满血
        ResetCountdownTimer(); // 重置倒计时
        UpdateHealthUI();
    }

    void Update()
    {
        // 只有在场景名称为数字时，才执行倒计时扣血逻辑
        if (int.TryParse(SceneManager.GetActiveScene().name, out int sceneNumber))
        {
            HandleTimer(); // 处理倒计时逻辑
        }
    }

    public void TakeDamage()
    {
        currentHealth -= healthLossPerMistake;
        PlayHealthLossSound();
        CheckHealth();
        UpdateHealthUI();
    }

    public void GainHealth()
    {
        currentHealth += healthGainOnPresetMatch;
        currentHealth = Mathf.Clamp(currentHealth, minHealth, maxHealth);
        PlayHealthGainSound();
        UpdateHealthUI();
    }

    void HandleTimer()
    {
        countdownTimer -= Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = $"倒计时：{Mathf.Ceil(countdownTimer)} 秒";
        }

        if (countdownTimer <= 0)
        {
            TakeDamageOverTime();
            ResetCountdownTimer(); // 重置倒计时
        }
    }

    void TakeDamageOverTime()
    {
        currentHealth -= healthLossOverTime;
        PlayHealthLossSound();
        CheckHealth();
        UpdateHealthUI();
    }

    void PlayHealthLossSound()
    {
        if (audioSource && healthLossClip)
        {
            audioSource.PlayOneShot(healthLossClip);
        }
    }

    void PlayHealthGainSound()
    {
        if (audioSource && healthGainClip)
        {
            audioSource.PlayOneShot(healthGainClip);
        }
    }

    void CheckHealth()
    {
        currentHealth = Mathf.Clamp(currentHealth, minHealth, maxHealth);

        if (currentHealth <= 0)
        {
            TriggerGameOver();
        }
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
        }

        if (healthPercentageText != null)
        {
            healthPercentageText.text = $"KPI跟进程度：{currentHealth}%";
        }
    }

    void TriggerGameOver()
    {
        Debug.Log("Game Over! Switching to GameOver scene.");
        SceneManager.LoadScene(gameOverSceneName);
    }

    private void RebindUIReferences()
    {
        // 重新查找 UI 组件
        healthSlider = GameObject.Find("Slider")?.GetComponent<Slider>();
        healthPercentageText = GameObject.Find("BloodCount")?.GetComponent<TextMeshProUGUI>();
        timerText = GameObject.Find("CountDown")?.GetComponent<TextMeshProUGUI>();

        if (healthSlider == null || healthPercentageText == null || timerText == null)
        {
            Debug.LogWarning("Failed to rebind one or more UI components.");
        }

        UpdateHealthUI(); // 确保 UI 及时更新
    }

    private void ResetCountdownTimer()
    {
        countdownTimer = timeInterval; // 重置倒计时为初始值
    }
}
