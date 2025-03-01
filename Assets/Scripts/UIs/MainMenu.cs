using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //check if the game is not save so that continue button is not appear
    public GameObject Information;
    public GameObject ContinueButton;

    private void Start()
    {
        if (SaveSystem.IsSaveFileEmpty())
        {
            ContinueButton.SetActive(false);
        }
    }

    public void PlayGame()
    {
        SaveSystem.DeleteSaveFileIfExists();

        SceneManager.LoadSceneAsync(1);
    }

    public void NewGame()
    {
        if (!SaveSystem.IsSaveFileEmpty())
        {
            Information.SetActive(true);
        }
        else
        {
            SceneManager.LoadSceneAsync(1);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Continue()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void Close()
    {
        if (SaveSystem.IsSaveFileEmpty())
        {
            ContinueButton.SetActive(false);

        }
        else
        {
            ContinueButton.SetActive(true);
        }
    }
}
