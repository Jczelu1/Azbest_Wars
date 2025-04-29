using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class MusicPlayer : MonoBehaviour
{
    public bool playOnStart = true;
    public bool isMenu = false;
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
    AudioSource startTheme;
    [SerializeField]
    AudioSource endTheme;
    [SerializeField]
    AudioSource menuTheme;
    [SerializeField]
    float fadeDuration = 2f;
    AudioSource currentTheme;
    int currentIndex;

    public static int Volume = 50;
    
    void Start()
    {
        if (isMenu)
        {
            menuTheme.Play();
            return;
        }
        if (playOnStart)
        {
            currentTheme = startTheme;
            currentTheme.Play();
        }
        if(VolumeSlider != null)
            VolumeSlider.value = Volume;
    }

    void Update()
    {
        if (isMenu) return;
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
            startTheme.volume = volume;
            endTheme.volume = volume;
            menuTheme.volume = volume;
        }
    }
    public void StopMusic()
    {
        if (currentTheme != null)
        {
            // Start fade-out coroutine
            StartCoroutine(FadeOutAndStop(currentTheme, fadeDuration, null));
            currentTheme = null;
        }
    }
    public void PlayEndMusic()
    {
        if (currentTheme != null)
        {
            // Start fade-out coroutine
            StartCoroutine(FadeOutAndStop(currentTheme, fadeDuration, endTheme));
        }
    }
    private IEnumerator FadeOutAndStop(AudioSource source, float duration, AudioSource nextSource)
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
        if(nextSource != null)
        {
            nextSource.Play();
        }
        currentTheme = null;
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
