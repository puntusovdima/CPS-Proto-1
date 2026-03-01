using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [Header("General Settings")] 
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private AudioSource sfxSource;

    [Header("Ambiance Settings")] 
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip ambiance;


    public static SoundManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMusicVolume();
            SetSfxVolume();
        }
    }

    public void PlayAmbiance()
    {
        musicSource.clip = ambiance;
        
    }

    /// <summary>
    /// Plays a sound effect once,
    /// useful for sound effects that are only played once like opening a door
    /// </summary>
    /// <param name="clip">Sound effect which will be played</param>
    /// <param name="source">AudioSource where the sound will be played from</param>
    public void PlaySound(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Plays a sound effect with a random pitch,
    /// useful for sound effects that are played repeatedly like footsteps
    /// </summary>
    /// <param name="clip">Sound effect which will be played</param>
    /// <param name="source">AudioSource where the sound will be played from</param>
    public void PlaySoundWithRandomPitch(AudioClip clip)
    {
        sfxSource.pitch = Random.Range(0.5f, 1.5f);
        sfxSource.PlayOneShot(clip);
    }

    public void SetMusicVolume()
    {
        var volume = musicSlider.value;
        mixer.SetFloat("music", Mathf.Log10(volume) * 20);

        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSfxVolume()
    {
        var volume = sfxSlider.value;
        mixer.SetFloat("sfx", Mathf.Log10(volume) * 20);

        PlayerPrefs.SetFloat("sfxVolume", volume);
    }

    public void SetMasterVolume()
    {
        var volume = masterSlider.value;
        mixer.SetFloat("master", Mathf.Log10(volume) * 20);
        
        PlayerPrefs.SetFloat("masterVolume", volume);
    }

    private void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");
        SetMusicVolume();
        SetSfxVolume();
    }
}