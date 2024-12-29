using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMainMenuHandler : MonoBehaviour
{
    [Header("Main Menu Scene Name")]
    public string mainMenuSceneName = "MainMenu"; // 主菜单场景名称

    void Update()
    {
        // 检测 R 键回到主菜单
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGameState();
            ReturnToMainMenu();
        }
    }

    private void ReturnToMainMenu()
    {
        Debug.Log("Returning to Main Menu...");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void ResetGameState()
    {
        Debug.Log("Resetting game state...");

        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.ResetState();
        }
        else
        {
            Debug.LogWarning("HealthManager instance not found.");
        }
    }
}
