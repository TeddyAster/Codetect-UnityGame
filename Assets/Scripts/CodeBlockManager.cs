using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CodeBlockManager : MonoBehaviour
{
    public Transform targetContainer;
    public Transform blockContainer;
    public GameObject codeBlockPrefab;
    public Button submitButton;
    public Text feedbackText;

    private int currentLevel;
    private LevelData currentLevelData;

    private void Start()
    {
        currentLevel = 1; // 初始关卡
        LoadLevel();
        submitButton.onClick.AddListener(ValidateSolution);
    }

    private void LoadLevel()
    {
        currentLevelData = LevelManager.Instance.GetLevelData(currentLevel);
        if (currentLevelData == null)
        {
            Debug.LogError("Level data not found!");
            return;
        }

        GenerateBlocksAndTargets();
    }

    private void GenerateBlocksAndTargets()
    {
        foreach (Transform child in targetContainer) Destroy(child.gameObject);
        foreach (Transform child in blockContainer) Destroy(child.gameObject);

        foreach (var block in currentLevelData.Blocks)
        {
            GameObject target = new GameObject($"Target_{block.Key}", typeof(RectTransform), typeof(Image));
            target.transform.SetParent(targetContainer);
            target.GetComponent<Image>().color = Color.gray;
            target.tag = "CodeBlockTarget";

            GameObject blockObj = Instantiate(codeBlockPrefab, blockContainer);
            blockObj.GetComponentInChildren<Text>().text = block.Value;
        }
    }

    private void ValidateSolution()
    {
        List<string> playerOrder = new List<string>();
        foreach (Transform target in targetContainer)
        {
            if (target.childCount > 0)
            {
                playerOrder.Add(target.GetChild(0).GetComponentInChildren<Text>().text);
            }
            else
            {
                feedbackText.text = "Incomplete!";
                return;
            }
        }

        foreach (var preset in currentLevelData.OrderPresets)
        {
            if (string.Join("", playerOrder) == preset.Value)
            {
                feedbackText.text = "Correct!";
                currentLevel = currentLevelData.GoToLevels[preset.Key];
                LoadLevel();
                return;
            }
        }

        feedbackText.text = "Incorrect!";
    }
}
