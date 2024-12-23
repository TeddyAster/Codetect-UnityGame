using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro; // 导入 TextMeshPro 的命名空间

namespace MG_BlocksEngine2.DragDrop
{
    public class LevelManager : MonoBehaviour
    {
        public GameObject blockPrefab; // 代码块的预制体
        public Transform blockParentTransform; // 代码块的父对象
        public string levelFilePath; // 关卡文件路径

        private Dictionary<int, string> blockData; // 存储块的ID和文本内容
        private string[] lines; // 存储关卡文件的所有行

        void Start()
        {
            // 读取关卡文件
            ReadLevelFile();

            // 解析关卡数据
            ParseLevelData();
        }

        void ReadLevelFile()
        {
            // 读取关卡文件内容
            if (File.Exists(levelFilePath))
            {
                lines = File.ReadAllLines(levelFilePath);
                Debug.Log("Level file loaded successfully.");
            }
            else
            {
                Debug.LogError($"Level file not found at: {levelFilePath}");
            }
        }

        void ParseLevelData()
        {
            bool isBlockSection = false; // 标记是否在 [Block] 部分
            blockData = new Dictionary<int, string>(); // 初始化块数据

            foreach (string line in lines)
            {
                // 检查是否进入 [Block] 部分
                if (line.StartsWith("[Block]"))
                {
                    isBlockSection = true;
                    continue;
                }
                // 检查是否退出 [Block] 部分
                else if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    isBlockSection = false;
                    continue;
                }

                // 处理 [Block] 部分的内容
                if (isBlockSection && line.Contains('='))
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int blockId))
                    {
                        blockData.Add(blockId, parts[1]); // 保存块ID和文本内容
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid block data: {line}");
                    }
                }
            }

            // 根据 blockData 生成代码块
            GenerateCodeBlocks();
        }

        void GenerateCodeBlocks()
        {
            // 计算生成代码块的最大高度
            float maxHeight = -100f; // 设置最大高度（根据需要调整）
            float minHeight = -200f;  // 设置最小高度

            // 设置生成范围的宽度
            float minWidth = -300f;  // 设置横坐标的最小值
            float maxWidth = 300f;   // 设置横坐标的最大值

            // 计算每一行的代码块数目（可以基于块的数量决定）
            int totalBlocks = blockData.Count;
            int rowCount = Mathf.CeilToInt(totalBlocks / 5f); // 每行最多生成5个代码块（根据需要调整）

            int blockIndex = 0; // 块索引

            foreach (var block in blockData)
            {
                int blockId = block.Key;       // 块ID
                string blockText = block.Value; // 块上的文本

                // 计算当前块的行号（行号决定纵坐标 y）
                int rowIndex = blockIndex / 5;  // 每行最多5个块，按行分配

                // 计算当前块的纵坐标（y轴），模拟山形效果
                float yPos = maxHeight - (rowIndex * (maxHeight - minHeight) / rowCount);

                // 计算当前块的横坐标（x轴），可以随机生成
                float xPos = Random.Range(minWidth, maxWidth);

                // 实例化代码块
                GameObject newBlock = Instantiate(blockPrefab, blockParentTransform);
                newBlock.name = blockId.ToString(); // 使用块ID命名代码块

                // 设置代码块文本（使用 TextMeshProUGUI）
                TextMeshProUGUI blockTextComponent = newBlock.GetComponentInChildren<TextMeshProUGUI>();
                if (blockTextComponent != null)
                {
                    blockTextComponent.text = blockText; // 设置文本
                }
                else
                {
                    Debug.LogWarning($"Block {blockId} has no TextMeshProUGUI component!");
                }

                // 设置随机生成的位置（x, y 坐标）
                RectTransform rectTransform = newBlock.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(xPos, yPos); // 设置块的位置
                }

                blockIndex++; // 递增块索引
            }
        }


        public bool IsCorrectOrder(string[] correctOrder)
        {
            // 验证当前代码块排列是否正确
            for (int i = 0; i < blockParentTransform.childCount; i++)
            {
                if (blockParentTransform.GetChild(i).name != correctOrder[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
