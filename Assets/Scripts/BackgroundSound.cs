using UnityEngine;

public class BackgroundSound : MonoBehaviour
{
    public AudioClip clip;
    public AudioSource bgSound;

    public float wait;
    public bool check;
    private float vol;
    private bool checkOnce;

    private void Start()
    {
        vol = PlayerPrefs.GetFloat("MUSICVOL", 1);
        PlayClip();
        checkOnce = true;
    }

    private void Update()
    {
        /*        if (GameManager.GameIsOver & checkOnce)
                {
                    checkOnce = false;
                    bgSound.Pause();
                }*/

        if (wait <= 0 & check)
        {
            check = false;
            PlayClip();
        }
        if (check)
            wait -= Time.deltaTime;
    }

    void PlayClip()
    {
        bgSound.clip = clip;
        bgSound.volume = vol;
        bgSound.Play();

        wait = bgSound.clip.length;
        check = true;
    }

    public void PausePlay()
    {
        if (bgSound.isPlaying)
        {
            bgSound.Pause();
            check = false;
        }
        else
        {
            bgSound.Play();
            check = true;
        }
    }
}
