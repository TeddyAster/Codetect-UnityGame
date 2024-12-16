using UnityEngine;

public class GameLogic : MonoBehaviour
{
    private LevelData currentLevel;

    private void Start()
    {
        int levelID = 1; // 示例：假设从关卡1开始
        currentLevel = LevelManager.Instance.GetLevelData(levelID);

        if (currentLevel == null)
        {
            Debug.LogError($"Level {levelID} data not found!");
            return;
        }

        // 示例使用 Blocks
        foreach (var block in currentLevel.Blocks)
        {
            Debug.Log($"Block ID: {block.Key}, Name: {block.Value}");
        }

        // 示例使用 StayBlocks
        Debug.Log($"StayBlocks: {currentLevel.StayBlocks}");

        // 示例使用 OrderPresets
        foreach (var preset in currentLevel.OrderPresets)
        {
            Debug.Log($"OrderPreset Key: {preset.Key}, Value: {preset.Value}");
        }

        // 示例使用 GoToLevels
        foreach (var goTo in currentLevel.GoToLevels)
        {
            Debug.Log($"GoToLevel Key: {goTo.Key}, Value: {goTo.Value}");
        }
    }
}
