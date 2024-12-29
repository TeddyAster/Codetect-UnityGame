using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class GenerateButtonWhenCountReached : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject buttonPrefab; // 按钮预制体
    public Transform parentTransform; // 按钮生成的父对象
    public string dataFileName = "PlayerEndings.txt"; // 玩家数据文件名

    [Header("Threshold Settings")]
    public int targetCount = 10; // 达成按钮生成的目标计数

    private string filePath; // 文件路径

    void Start()
    {
        // 获取项目根目录路径（与 Assets 同级）
        string projectRootPath = Directory.GetParent(Application.dataPath).FullName;
        filePath = Path.Combine(projectRootPath, dataFileName);

        // 检查计数并生成按钮
        CheckAndGenerateButton();
    }

    void CheckAndGenerateButton()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Data file not found at: {filePath}");
            return;
        }

        int totalCount = 0;
        string[] lines = File.ReadAllLines(filePath);

        // 计算总计数
        foreach (string line in lines)
        {
            if (line.Contains("="))
            {
                string[] parts = line.Split('=');
                if (parts.Length == 2 && int.TryParse(parts[1], out int value))
                {
                    totalCount += value;
                }
            }
        }

        // 如果计数达到目标值，生成按钮
        if (totalCount >= targetCount && buttonPrefab != null && parentTransform != null)
        {
            Debug.Log("Target count reached. Generating button...");
            GameObject newButton = Instantiate(buttonPrefab, parentTransform);
            newButton.GetComponentInChildren<Text>().text = "[你已经获得了所有结局，最后问你最后一个问题(点击进入)]"; // 设置按钮文本
            newButton.GetComponent<Button>().onClick.AddListener(ResetRecords); // 绑定重置记录方法
        }
        else
        {
            Debug.Log("Target count not reached. Button will not be generated.");
        }
    }

    // 删除重置记录文件的操作（已移除文件写入部分）
    void ResetRecords()
    {
        Debug.Log("Reset Button Clicked.");
        // 你可以选择在此加入其他逻辑，比如重新开始游戏或者其他操作
    }
}
