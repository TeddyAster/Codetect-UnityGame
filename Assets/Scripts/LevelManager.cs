using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro; // 引入 TextMeshPro 命名空间
using UnityEngine.UI;  // 引入 UI 命名空间
using System.Collections;
using UnityEngine.SceneManagement;

namespace MG_BlocksEngine2.DragDrop
{
    public class LevelManager : MonoBehaviour
    {
        public GameObject validateParentObject; // 用于验证代码块顺序的父对象
        public GameObject blockPrefab; // 代码块的预制体
        public Transform blockParentTransform; // 代码块的父对象
        public Button submitButton;  // 用于提交的按钮
        public TextMeshProUGUI storyText;  // 用于显示剧情文本的 TextMeshProUGUI
        public GameObject TransitionOut; // 场景过渡效果
        public TextAsset levelFile; // 用于拖入 `.levelmanagement` 文件
        public static LevelManager Instance; // 单例实例
        public string nextLevel; // 改为 string 类型，支持字符类型的关卡

        private Dictionary<int, string> blockData; // 存储块的ID和文本内容
        private string[] lines; // 存储关卡文件的所有行

        private Dictionary<string, List<int>> orderPresets = new Dictionary<string, List<int>>();  // 存储 OrderPreset
        private Dictionary<string, string> storyTexts = new Dictionary<string, string>();  // 存储 StoryText
        private Dictionary<string, string> goToLevels = new Dictionary<string, string>();  // 存储 GoToLevel 映射
        private List<int> currentOrder = new List<int>();  // 玩家当前的顺序

        void Start()
        {
            // 读取关卡文件
            ReadLevelFile();

            // 解析关卡数据
            ParseLevelData();

            // 生成代码块
            GenerateCodeBlocks();

            // 为按钮绑定点击事件
            submitButton.onClick.AddListener(OnSubmit);
        }

        void ReadLevelFile()
        {
            if (levelFile != null)
            {
                // 使用 TextAsset 的 `text` 属性读取内容
                lines = levelFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                Debug.Log("Level file loaded successfully.");
            }
            else
            {
                Debug.LogError("Level file is not assigned in the Inspector!");
            }
        }

        void ParseLevelData()
        {
            bool isBlockSection = false; // 标记是否在 [Block] 部分
            blockData = new Dictionary<int, string>(); // 初始化块数据
            bool inOrderPreset = false; // 解析 OrderPreset
            bool inStoryText = false; // 解析 StoryText
            bool inGoToLevel = false; // 解析 GoToLevel

            foreach (string line in lines)  // 这里用 lines 变量进行处理
            {
                // 检查是否进入 [Block] 部分
                if (line.StartsWith("[Block]"))
                {
                    isBlockSection = true;
                    Debug.Log("Read Blocks Start.");
                    continue;
                }
                // 检查是否退出 [Block] 部分
                else if (line.StartsWith("[/Block]"))
                {
                    isBlockSection = false;
                    Debug.Log("Read Blocks Finish.");
                    continue;
                }

                // 解析 [Block] 部分的内容
                if (isBlockSection && line.Contains('='))
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int blockId))
                    {
                        blockData.Add(blockId, parts[1]); // 保存块ID和文本内容
                    }
                    else if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        isBlockSection = false;
                        continue;
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid block data: {line}");
                    }
                }

                // 解析 OrderPreset 部分
                if (line.StartsWith("[OrderPreset]"))
                {
                    inOrderPreset = true;
                    Debug.Log("Read Orderpreset Start");
                    continue;
                }
                else if (line.StartsWith("[/OrderPreset]"))
                {
                    Debug.Log("Read Orderpreset Finish");
                    inOrderPreset = false;
                    continue;
                }

                if (inOrderPreset && line.Contains('='))
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        string presetName = parts[0].Trim();
                        string[] order = parts[1].Split(',');

                        List<int> orderList = new List<int>();
                        foreach (var item in order)
                        {
                            if (int.TryParse(item.Trim(), out int value)) // 确保转换为整数
                            {
                                orderList.Add(value);
                            }
                            else
                            {
                                Debug.LogWarning($"Invalid order value: {item.Trim()} in preset {presetName}");
                            }
                        }
                        orderPresets.Add(presetName, orderList);
                        Debug.Log($"Added preset {presetName}: {string.Join(", ", orderList)}");
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid OrderPreset line: {line}");
                    }
                }

                // 解析 StoryText 部分
                if (line.StartsWith("[StoryText]"))
                {
                    inStoryText = true;
                    Debug.Log("Read Storytext Start");
                    continue;
                }
                else if (line.StartsWith("[/StoryText]"))
                {
                    Debug.Log("Read Storytext Finish");
                    inStoryText = false;
                    continue;
                }
                if (inStoryText && line.Contains('='))
                {
                    string[] parts = line.Split('=');
                    string presetName = parts[0].Trim();
                    string story = parts[1].Trim();

                    storyTexts[presetName] = story;
                }
                // 解析 GoToLevel 部分
                if (line.StartsWith("[GoToLevel]"))
                {
                    inGoToLevel = true;
                    Debug.Log("Read GotoLevel Start.");
                    continue;
                }
                else if (line.StartsWith("[/GoToLevel]"))
                {
                    Debug.Log("Read GotoLevel Finish.");
                    inGoToLevel = false;
                    continue;
                }
                if (inGoToLevel && line.Contains('='))
                {
                    string[] parts = line.Split('=');
                    string presetName = parts[0].Trim();
                    string nextLevelName = parts[1].Trim();  // 使用字符串类型
                    goToLevels[presetName] = nextLevelName;
                }
            }
        }

        void GenerateCodeBlocks()
        {
            if (blockData.Count == 0)
            {
                Debug.LogError("No blocks data found!");
                return;
            }

            // 计算生成代码块的最大高度
            float maxHeight = -700f; // 设置最大高度（根据需要调整）
            float minHeight = -1000f;  // 设置最小高度

            // 设置生成范围的宽度
            float minWidth = 300f;  // 设置横坐标的最小值
            float maxWidth = 700f;   // 设置横坐标的最大值

            // 计算每一行的代码块数目（可以基于块的数量决定）
            int totalBlocks = blockData.Count;
            int rowCount = Mathf.CeilToInt(totalBlocks / 3f); // 每行最多生成3个代码块（根据需要调整）

            int blockIndex = 0; // 块索引

            foreach (var block in blockData)
            {
                int blockId = block.Key;       // 块ID
                string blockText = block.Value; // 块上的文本

                // 计算当前块的行号（行号决定纵坐标 y）
                int rowIndex = blockIndex / 3;  // 每行最多4个块，按行分配

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

        void OnSubmit()
        {
            if (validateParentObject == null)
            {
                Debug.LogError("Validate parent object is not assigned!");
                return;
            }

            if (orderPresets.Count > 0)
            {
                foreach (var preset in orderPresets)
                {
                    Debug.Log($"Existing preset: {preset.Key}, Order: {string.Join(", ", preset.Value)}");
                }
            }
            else
            {
                Debug.LogWarning("No existing presets found.");
            }

            Transform parentTransform = validateParentObject.transform; // 获取验证对象的 Transform
            currentOrder.Clear();

            // 获取当前代码块的排列顺序
            foreach (Transform child in parentTransform)
            {
                if (int.TryParse(child.name, out int blockId))
                {
                    currentOrder.Add(blockId);
                }
                else
                {
                    Debug.LogWarning($"Skipping invalid block name: {child.name}");
                }
            }

            Debug.Log("Current Order: " + string.Join(", ", currentOrder));

            // 新增功能：检查是否包含 `20`
            if (currentOrder.Contains(20))
            {
                Debug.Log("Match found with Y level");
                StartCoroutine(DisplayText(storyTexts["Y"], "Y"));
                return;
            }

            // 新增功能：检查顺序长度为 19
            if (currentOrder.Count == 19)
            {
                Debug.Log("Match found with X level");
                StartCoroutine(DisplayText(storyTexts["X"], "X"));
                return;
            }

            // 验证当前顺序是否与某个 OrderPreset 匹配
            foreach (var preset in orderPresets)
            {
                Debug.Log($"Checking against preset {preset.Key}: {string.Join(", ", preset.Value)}");

                if (CompareOrder(currentOrder, preset.Value))
                {
                    Debug.Log($"Match found with preset {preset.Key}");
                    StartCoroutine(DisplayText(storyTexts[preset.Key], preset.Key)); // 传递 presetKey
                    return;
                }
            }

            // 如果顺序不是预设中的任意一种，检查是否是 Z
            if (IsOrderZ(currentOrder))
            {
                Debug.Log("Match found with other presets");
                StartCoroutine(DisplayText(storyTexts["Z"], "Z")); // 触发Z的剧情
                return;
            }

            // 如果顺序不正确，显示反馈
            StartCoroutine(DisplayText("无法运行，请检查你的排序！"));
            FindObjectOfType<HealthManager>().TakeDamage();
        }

        bool IsOrderZ(List<int> order)
        {
            // 检查首尾是否为1和8，其他元素不限制
            if (order.Count > 1 && order[0] == 1 && order[order.Count - 1] == 8)
            {
                Debug.Log("Order matches Z");
                return true;
            }
            return false;
        }

        bool CompareOrder(List<int> order1, List<int> order2)
        {
            if (order1.Count != order2.Count)
            {
                Debug.Log("Order lengths do not match.");
                return false;
            }

            for (int i = 0; i < order1.Count; i++)
            {
                if (order1[i] != order2[i])
                {
                    Debug.Log($"Mismatch at index {i}: {order1[i]} != {order2[i]}");
                    return false;
                }
            }

            return true;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject); // 确保单例唯一
            }
        }

        IEnumerator DisplayText(string message, string presetKey = null)
        {
            storyText.text = "";
            foreach (char letter in message.ToCharArray())
            {
                storyText.text += letter;
                yield return new WaitForSeconds(0.05f); // 打字机效果的时间间隔
            }

            // 等待3秒后加载对应的关卡（如果presetKey不为空）
            if (!string.IsNullOrEmpty(presetKey) && goToLevels.ContainsKey(presetKey))
            {
                FindObjectOfType<HealthManager>().GainHealth();
                // 设置实例变量 nextLevel
                nextLevel = goToLevels[presetKey];
                Debug.Log($"Next level is set to: {nextLevel}");

                yield return new WaitForSeconds(3f); // 等待3秒
                TransitionOut.SetActive(true); // 加载场景
                SceneManager.LoadScene(nextLevel); // 直接加载字符串类型的场景
            }
        }
    }
}
