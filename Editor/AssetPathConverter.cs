using UnityEditor;  // 引入 UnityEditor 命名空间以访问编辑器 API
using UnityEditor.Callbacks; // 引入 PostProcessBuild 相关的命名空间
using UnityEngine; // 引入 UnityEngine 命名空间以使用 Debug

using System.IO;

public class AssetPathConverter : Editor
{
    // 需要转换的文件夹
    private const string SourceFolder = "Assets/StreamingAssets";
    private const string DestinationFolder = "Assets/Resources"; // 目标是 Resources 文件夹

    // 使用正确的签名：接收 BuildTarget 和 String 参数
    [PostProcessBuild(1)]
    public static void OnPostBuild(BuildTarget target, string pathToBuiltProject)
    {
        // 这里可以放置转换的代码
        ConvertAssetsToResources();
    }

    [MenuItem("Tools/Convert Assets to Resources")]
    public static void ConvertAssetsToResources()
    {
        // 获取源文件夹中所有文件
        string[] files = Directory.GetFiles(SourceFolder, "*", SearchOption.AllDirectories);
        
        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            string destinationPath = Path.Combine(DestinationFolder, fileName);

            // 确保目标文件夹存在
            string destinationDir = Path.GetDirectoryName(destinationPath);
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            // 将文件从源路径复制到目标路径
            if (!File.Exists(destinationPath)) // 如果目标文件夹中没有该文件
            {
                File.Copy(file, destinationPath);
                AssetDatabase.Refresh(); // 刷新资源数据库
                Debug.Log($"Copied file: {file} -> {destinationPath}"); // 使用 Debug.Log 打印信息
            }
            else
            {
                Debug.LogWarning($"File already exists at {destinationPath}. Skipping.");
            }
        }

        // 刷新所有资源
        AssetDatabase.Refresh();

        // 提示
        EditorUtility.DisplayDialog("Asset Conversion", "Assets have been successfully moved to Resources!", "OK");
    }
}
