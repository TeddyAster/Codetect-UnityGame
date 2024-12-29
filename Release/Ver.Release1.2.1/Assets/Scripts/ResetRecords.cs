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
        // 获取项目根目录路径（与 Assets 同级）
        string projectRootPath = Directory.GetParent(Application.dataPath).FullName;
        string filePath = Path.Combine(projectRootPath, "PlayerEndings.txt");

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
