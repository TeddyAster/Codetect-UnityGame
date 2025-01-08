using UnityEngine;
using System.IO;
using TMPro;

public class EndingManager : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI endingsDisplayText; // 显示结局数量的 UI 文本

    private string filePath; // 玩家数据文件路径
    private int totalEndings = 0; // 达成的总结局数量

    void Start()
    {
        // 获取 Unity 默认的持久化数据路径
        filePath = Path.Combine(Application.persistentDataPath, "PlayerEndings.txt");
        Debug.Log($"Player data file located at: {filePath}");

        // 加载或创建玩家数据文件
        LoadOrCreateEndingsFile();

        // 更新主页显示
        UpdateEndingDisplay();
    }

    void LoadOrCreateEndingsFile()
    {
        if (!File.Exists(filePath))
        {
            Debug.Log("No endings file found. Creating a new one.");

            // 创建文件并初始化默认结局信息
            string defaultData = "Credits_Fin=0\nEnding_Caught=0\nEnding_Destroy=0\nEnding_Did=0\nEnding_Fired=0\nEnding_Hero=0\nEnding_KickedOut=0\nEnding_RealGangster=0\nEnding_Runaway=0\nEnding_Waiting=0\nEnding_Worker=0\n";
            File.WriteAllText(filePath, defaultData);
        }
        else
        {
            Debug.Log("Endings file found. Loading data.");
        }
    }

    void UpdateEndingDisplay()
    {
        totalEndings = 0; // 重置计数

        // 读取文件内容并计算达成的结局数量
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (line.Contains("="))
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int value))
                    {
                        totalEndings += value;
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Endings file not found during update display.");
        }

        // 在 UI 上显示达成的结局数量
        if (endingsDisplayText != null)
        {
            endingsDisplayText.text = new string('★', totalEndings);
        }
        else
        {
            Debug.LogError("Endings display text is not assigned in the Inspector.");
        }
    }
}
