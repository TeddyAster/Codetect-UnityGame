using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CodeBlockManager : MonoBehaviour
{
    [Header("Code Block Configuration")]
    public Transform codeBlockContainer;
    public GameObject codeBlockPrefab;
    public List<string> correctOrder;

    [Header("Next Level Settings")]
    public string nextSceneName; // 下一关卡名

    private List<CodeBlock> codeBlocks = new List<CodeBlock>();

    void Start()
    {
        SetupCodeBlocks();
    }

    private void SetupCodeBlocks()
    {
        foreach (Transform child in codeBlockContainer)
        {
            Destroy(child.gameObject);
        }
        codeBlocks.Clear();

        // 创建新的代码块
        List<string> shuffledOrder = new List<string>(correctOrder);
        ShuffleList(shuffledOrder);

        foreach (string blockText in shuffledOrder)
        {
            GameObject newBlock = Instantiate(codeBlockPrefab, codeBlockContainer);
            newBlock.GetComponentInChildren<Text>().text = blockText;
            codeBlocks.Add(newBlock.GetComponent<CodeBlock>());
        }
    }

    public void CheckSolution()
    {
        List<string> playerOrder = new List<string>();

        foreach (CodeBlock block in codeBlocks)
        {
            playerOrder.Add(block.GetComponentInChildren<Text>().text);
        }

        if (IsCorrectOrder(playerOrder))
        {
            Debug.Log("Proceeding to the next level.");
            LoadNextLevel();
        }
        else
        {
            Debug.Log("Incorrect! Try again.");
        }
    }

    private bool IsCorrectOrder(List<string> playerOrder)
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

    private void LoadNextLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
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
