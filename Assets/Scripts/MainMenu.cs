using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private void Start()
    {
        int oneShot = PlayerPrefs.GetInt("oneShot", 0);
        if (oneShot == 0)
        {

        }
    }
}
