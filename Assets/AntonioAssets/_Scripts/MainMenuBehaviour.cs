using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuBehaviour : MonoBehaviour
{
    public void ToggleVisibility(GameObject obj)
    {
        obj.SetActive(!obj.activeSelf);
    }

    public void StartNewGame(int index)
    {
        Debug.Log("Start new game");
        SceneManager.LoadScene(index);
    }
    /*
    public void Resume()
    {
        if (pausePanel != null){
            pausePanel.SetActive(false);
        }
        if (inGamePauseButton != null){
            inGamePauseButton.gameObject.SetActive(true);
        }
        
        Time.timeScale = 1f;
        _isPaused = false;
        PlayerFSM.IsPaused = false;
    }
    */
    public void QuitGame()
    {
        Debug.Log("Quit game");
        
        Application.Quit();
    }
}
