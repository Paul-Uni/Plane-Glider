using UnityEngine;
using UnityEngine.SceneManagement;

// Allgemeiner Level-Button: im OnClick den Szenennamen als Parameter eintragen.
public class LevelSelect : MonoBehaviour
{
    public void Load(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
