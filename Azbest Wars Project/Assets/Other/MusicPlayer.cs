using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [SerializeField]
    AudioSource[] themes;
    AudioSource currentTheme;
    int currentIndex;
    
    void Start()
    {
        PlayRandomTheme();
    }

    void Update()
    {
        if (currentTheme != null && !currentTheme.isPlaying)
        {
            currentTheme.Stop();
            PlayRandomTheme();
            //Invoke("PlayRandomTheme", 2f);
        }
    }
    public void StopMusic()
    {
        if (currentTheme != null)
        {
            currentTheme.Stop();
            currentTheme = null;
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
            } while (currentIndex == index);
        }
        currentIndex = index;
        currentTheme = themes[index];
        currentTheme.Play();
    }
}
