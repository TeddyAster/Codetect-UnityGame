using UnityEngine;

public class ExitGameHandler : MonoBehaviour
{
    void Update()
    {
        // 检测 ESC 键退出游戏
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    // 退出游戏
    private void QuitGame()
    {
        Debug.Log("Exiting game...");
#if UNITY_EDITOR
        // 在编辑器模式下退出
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 在发布版退出
        Application.Quit();
#endif
    }
}
