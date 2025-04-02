using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField]
    AudioSource[] themes;
    AudioSource currentTheme;
    void Start()
    {
        PlayRandomTheme();
    }

    void Update()
    {
        if (currentTheme != null && !currentTheme.isPlaying)
        {
            currentTheme.Stop();
            Invoke("PlayRandomTheme", 2f);
        }
    }
    void PlayRandomTheme()
    {
        if (themes == null || themes.Length == 0)
        {
            Debug.LogWarning("No themes assigned!");
            return;
        }

        if (currentTheme != null)
            currentTheme.Stop();

        int index;
        if (themes.Length == 1)
        {
            index = 0;
        }
        else
        {
            do
            {
                index = Random.Range(0, themes.Length);
            } while (currentTheme == themes[index]);
        }
        currentTheme = themes[index];
        currentTheme.Play();
    }
}
