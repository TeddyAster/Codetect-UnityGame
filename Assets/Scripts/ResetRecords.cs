using UnityEngine;
using System.IO;

public class ResetRecords : MonoBehaviour
{
    private void Start()
    {
        ResetAllRecords();
    }

    private void ResetAllRecords()
    {
        // 获取 Unity 的持久化数据路径
        string filePath = Path.Combine(Application.persistentDataPath, "PlayerEndings.txt");

        // 删除文件
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"Player records file has been deleted from: {filePath}");
        }
        else
        {
            Debug.Log($"No records file found at: {filePath}");
        }
    }
}
