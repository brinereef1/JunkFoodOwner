using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;

    private void Awake()
    {
        // Start the game in pause mode until the player presses play.
        Time.timeScale = 0;
    }

    public void PlayGame()
    {
        // Hide the menu and start the game.
        mainMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ExitGame()
    {
        // Quit the game in the build and stop play mode in the editor.
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}