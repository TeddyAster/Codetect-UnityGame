using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;  // 引入SceneManagement命名空间

public class EndingTrigger : MonoBehaviour
{
    [Header("Ending Settings")]
    private string endingKey; // 当前结局的键值（由当前场景名称自动设置）

    private string filePath; // 文件路径

    void Start()
    {
        // 获取当前场景的名称作为 endingKey
        endingKey = SceneManager.GetActiveScene().name;

        // 获取项目根目录路径（与 Assets 同级）
        string projectRootPath = Directory.GetParent(Application.dataPath).FullName;
        filePath = Path.Combine(projectRootPath, "PlayerEndings.txt");

        // 确保文件存在
        EnsureFileExists();

        // 更新结局记录
        UpdateEndingRecord();
    }

    void EnsureFileExists()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Endings file not found. Creating a new one at: {filePath}");
            string defaultData = "Credits_Fin=0\nEnding_Caught=0\nEnding_Destroy=0\nEnding_Did=0\nEnding_Fired=0\nEnding_Hero=0\nEnding_KickedOut=0\nEnding_RealGangster=0\nEnding_Runaway=0\nEnding_Waiting=0\nEnding_Worker=0\n";
            File.WriteAllText(filePath, defaultData);
        }
    }

    void UpdateEndingRecord()
    {
        if (string.IsNullOrEmpty(endingKey))
        {
            Debug.LogError("Ending key is not set!");
            return;
        }

        // 读取文件内容
        string[] lines = File.ReadAllLines(filePath);
        bool updated = false;
        bool foundKey = false;

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith(endingKey + "="))
            {
                foundKey = true;
                string[] parts = lines[i].Split('=');
                if (parts.Length == 2 && int.TryParse(parts[1], out int value))
                {
                    if (value == 0)
                    {
                        lines[i] = $"{endingKey}=1"; // 更新值为 1
                        updated = true;
                        Debug.Log($"Ending {endingKey} updated to 1.");
                    }
                    else
                    {
                        Debug.Log($"Ending {endingKey} already completed.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Invalid record format for key: {endingKey}");
                }
                break;
            }
        }

        // 如果文件中未找到键值，则追加新记录
        if (!foundKey)
        {
            using (StreamWriter writer = File.AppendText(filePath))
            {
                writer.WriteLine($"{endingKey}=1");
                Debug.Log($"Ending {endingKey} added to the file.");
            }
        }
        else if (updated)
        {
            // 将更新后的内容写回文件
            File.WriteAllLines(filePath, lines);
        }
    }
}
