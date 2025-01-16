using UnityEngine;
using UnityEngine.SceneManagement;

public class WinSceneManager : MonoBehaviour
{
    public void ReplayLevel()
    {
        // Reload the current level
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        // Load the Main Menu scene
        SceneManager.LoadScene("MainMenu");
    }
}
