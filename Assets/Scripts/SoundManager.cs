using UnityEngine;

public static class SoundManager
{
    public enum Sound
    {
        Move,
        SimpleMatch,
        BombMatch,
        BombExplode,
        Win,
        Lose,
        Click,
    }

    private static GameObject oneShotGameObject;
    private static AudioSource oneShotAudioSource;
    public static void PlaySound(Sound sound)
    {
        if (oneShotGameObject == null)
        {
            oneShotGameObject = new GameObject("One Shot Sound");
            oneShotAudioSource = oneShotGameObject.AddComponent<AudioSource>();
        }
        oneShotAudioSource.PlayOneShot(GetAudioClip(sound));
        oneShotAudioSource.volume = PlayerPrefs.GetFloat("SOUNDVOL", 1);

        // to play sond
        //SoundManager.PlaySound(SoundManager.Sound.PlayerJump1);
    }

    private static AudioClip GetAudioClip(Sound sound)
    {
        foreach (GameAssets.SoundAudioClip soundAudioClip in GameAssets.i.soundAudioClipArray)
        {
            if (soundAudioClip.sound == sound)
            {
                return soundAudioClip.audioClip;
            }
        }
        Debug.LogError("Sound" + sound + "not found!");
        return null;
    }
}
