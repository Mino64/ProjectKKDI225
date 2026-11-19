using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private int _sceneNumber;



    public void PlayGame()
    {
        SceneManager.LoadScene(_sceneNumber);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
