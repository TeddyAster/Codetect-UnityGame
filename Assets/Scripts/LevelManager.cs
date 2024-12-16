using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    private Dictionary<int, LevelData> levels = new Dictionary<int, LevelData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLevels();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadLevels()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "mainstory.levelmanagement");
        if (!File.Exists(filePath))
        {
            Debug.LogError("Level management file not found!");
            return;
        }

        string[] lines = File.ReadAllLines(filePath);
        LevelData currentLevel = null;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.StartsWith("{Level="))
            {
                int levelID = int.Parse(line.Split('=')[1].Replace("}", ""));
                currentLevel = new LevelData { LevelID = levelID };
                levels[levelID] = currentLevel;
            }
            else if (line.StartsWith("[Block]"))
            {
                continue;
            }
            else if (line.StartsWith("[StayBlock]"))
            {
                currentLevel.StayBlocks = lines[++i].Trim(); // 下一行保存 StayBlocks 数据
            }
            else if (line.StartsWith("[OrderPreset="))
            {
                string presetKey = line.Split('=')[1].Replace("]", "");
                string presetValue = lines[++i].Trim(); // 下一行保存 presetValue
                currentLevel.OrderPresets[presetKey] = presetValue;
            }
            else if (line.StartsWith("[GoToLevel]"))
            {
                i++; // 跳过 [GoToLevel]
                while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
                {
                    string[] parts = lines[i].Split('=');
                    currentLevel.GoToLevels[parts[0]] = int.Parse(parts[1]);
                    i++;
                }
                i--; // 回到非空行处理逻辑
            }
            else if (currentLevel != null && line.Contains("="))
            {
                string[] parts = line.Split('=');
                currentLevel.Blocks[int.Parse(parts[0])] = parts[1];
            }
        }
    }

    public LevelData GetLevelData(int levelID)
    {
        return levels.ContainsKey(levelID) ? levels[levelID] : null;
    }
}

[System.Serializable]
public class LevelData
{
    public int LevelID;
    public Dictionary<int, string> Blocks = new Dictionary<int, string>();
    public string StayBlocks;
    public Dictionary<string, string> OrderPresets = new Dictionary<string, string>();
    public Dictionary<string, int> GoToLevels = new Dictionary<string, int>();
}
