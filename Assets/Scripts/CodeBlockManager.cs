using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CodeBlockManager : MonoBehaviour
{
    [Header("Code Block Configuration")]
    public Transform codeBlockContainer; // 代码块容器
    public GameObject codeBlockPrefab;   // 代码块预制体

    [Header("Debugging")]
    public Text feedbackText; // 提示文本

    private List<GameObject> spawnedBlocks = new List<GameObject>(); // 存储生成的代码块

    [System.Serializable]
    public class CodeSequence
    {
        public List<string> correctOrder; // 正确的代码块顺序
        public string targetScene;       // 对应跳转的场景名称
    }

    [Header("Code Block Settings")]
    [Tooltip("输入代码块的自定义标签，用逗号分隔：Block1, Block2, Block3")]
    [TextArea]
    public string codeBlockTags; // 文本输入框，定义每个代码块的自定义标签

    [Tooltip("代码块的起始位置")]
    public Vector3 startPosition = new Vector3(0, 0, 0); // 起始位置

    [Tooltip("代码块之间的水平间距")]
    public float horizontalSpacing = 2.0f; // 每个代码块的水平间距

    [Tooltip("父级对象，用于存放生成的代码块")]
    public Transform parentTransform; // 父级对象，用于管理生成的代码块

    [Header("Sequences")]
    public List<CodeSequence> sequences; // 存储不同的代码块顺序

    private List<CodeBlock> codeBlocks = new List<CodeBlock>();

    void Start()
    {
        SetupCodeBlocks();
    }

    private void SetupCodeBlocks()
    {
        if (codeBlockPrefab == null)
        {
            Debug.LogError("CodeBlockPrefab is not assigned.");
            return;
        }

        // 解析输入的标签
        string[] blockTags = codeBlockTags.Split(',');
        Vector3 currentPosition = startPosition;

        foreach (string tag in blockTags)
        {
            string trimmedTag = tag.Trim(); // 去除多余空格

            // 生成代码块
            GameObject block = Instantiate(codeBlockPrefab, currentPosition, Quaternion.identity, parentTransform);

            // 设置自定义标签
            block.name = trimmedTag;

            // 将生成的代码块添加到列表中
            spawnedBlocks.Add(block);

            // 更新位置
            currentPosition.x += horizontalSpacing;
        }

        if (spawnedBlocks.Count == 0)
        {
            Debug.LogError("No CodeBlocks were created. Check your input.");
        }
    }

    public void CheckSolution()
    {
        List<string> playerOrder = new List<string>();

        // 收集玩家摆放的顺序
        foreach (GameObject block in spawnedBlocks)
        {
            playerOrder.Add(block.name); // 使用名字作为顺序标识
        }

        foreach (CodeSequence sequence in sequences)
        {
            if (IsCorrectOrder(playerOrder, sequence.correctOrder))
            {
                feedbackText.text = "Correct! Loading " + sequence.targetScene;
                LoadScene(sequence.targetScene);
                return;
            }
        }

        feedbackText.text = "Incorrect! Try again.";
    }

    private bool IsCorrectOrder(List<string> playerOrder, List<string> correctOrder)
    {
        if (playerOrder.Count != correctOrder.Count)
            return false;

        for (int i = 0; i < correctOrder.Count; i++)
        {
            if (playerOrder[i] != correctOrder[i])
                return false;
        }

        return true;
    }

    private void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    private void ShuffleList(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            string temp = list[i];
            int randomIndex = Random.Range(0, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
