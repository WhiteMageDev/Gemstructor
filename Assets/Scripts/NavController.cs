using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SceneFader))]
public class NavController : MonoBehaviour
{
    private SceneFader fader;
    private void Awake()
    {
        fader = GetComponent<SceneFader>();
    }
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
        Time.timeScale = ui.activeSelf ? 0 : 1;
    }
    public void PlayClickSound()
    {
        SoundManager.PlaySound(SoundManager.Sound.Click);
    }
}
