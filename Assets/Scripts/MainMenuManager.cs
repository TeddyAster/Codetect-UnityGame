using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void LoadLevel1()
    {
        SceneManager.LoadScene(1); // 确保 "Level1" 已添加到 Build Settings
    }
}
