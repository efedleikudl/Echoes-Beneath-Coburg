using UnityEngine;

public class StartMenuController : MonoBehaviour
{
    public void OnStartButtonClicked()
    {
        // Load the game scene when the start button is clicked
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
    public void OnExitButtonClicked()
    {
        // Exit the application when the exit button is clicked
        Application.Quit();

        // If running in the editor, stop playing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}