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
    
    public void QuitGame()
    {
        Debug.Log("Quit game");
        
        Application.Quit();
    }
}
