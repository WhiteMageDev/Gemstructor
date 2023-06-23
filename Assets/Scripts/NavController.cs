using UnityEngine;
using UnityEngine.SceneManagement;

public class NavController : MonoBehaviour
{
    public SceneFader fader;
    public void gotoSceneA(string a)
    {
        Time.timeScale = 1;
        fader.FadeTo(a);
    }
    public void Quit()
    {
        Application.Quit();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1;
        fader.FadeTo(SceneManager.GetActiveScene().name);
    }

    public void TogglePause(GameObject ui)
    {
        ui.SetActive(!ui.activeSelf);
        Time.timeScale = ui.activeSelf == true ? 0 : 1;
    }
    public void PlayClickSound()
    {
        SoundManager.PlaySound(SoundManager.Sound.Click);
    }
}
