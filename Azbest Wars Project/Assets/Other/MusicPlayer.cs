using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class MusicPlayer : MonoBehaviour
{
    public bool playOnStart = true;
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
    Slider VolumeSlider;
    [SerializeField]
    AudioSource[] themes;
    [SerializeField]
    float fadeDuration = 2f;
    AudioSource currentTheme;
    int currentIndex;

    public static int Volume = 50;
    
    void Start()
    {
        if(playOnStart)
            PlayRandomTheme();
        if(VolumeSlider != null)
            VolumeSlider.value = Volume;
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
    public void SetVolume()
    {
        if (VolumeSlider != null)
        {
            float volume = VolumeSlider.value;
            Volume = (int)volume;
            volume /= VolumeSlider.maxValue;
            foreach (AudioSource source in themes)
            {
                source.volume = volume;
            }
        }
    }
    public void StopMusic()
    {
        if (currentTheme != null)
        {
            // Start fade-out coroutine
            StartCoroutine(FadeOutAndStop(currentTheme, fadeDuration));
            currentTheme = null;
        }
    }
    private IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float time = 0f;

        while (time < duration)
        {
            source.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
        source.volume = startVolume;
    }
    public void PlayRandomTheme()
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
