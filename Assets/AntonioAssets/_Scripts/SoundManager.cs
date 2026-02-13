using Unity.VisualScripting;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Ambiance Music Audio Source")]
    [SerializeField] private AudioSource musicSource;
    
    [Header("Ambiance Music Clips")]
    [SerializeField] private AudioClip safeZoneMusic;
    [SerializeField] private AudioClip puzzleZoneMusic;
    [SerializeField] private AudioClip battleZoneMusic;
    
    private AudioSource _source;
    
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
    
    /// <summary>
    /// Function used to set the AudioSource where the sound will be played from
    /// </summary>
    /// <param name="source">AudioSource where the sound will be played from</param>
    public void SetAudioSource(AudioSource source)
    {
        Instance._source = source; 
    }
    
    /// <summary>
    /// Plays a sound effect once,
    /// useful for sound effects that are only played once like opening a door
    /// </summary>
    /// <param name="clip">Sound effect which will be played</param>
    /// <param name="source">AudioSource where the sound will be played from</param>
    public void PlaySound(AudioClip clip, AudioSource source)
    {
        SetAudioSource(source);
        _source.PlayOneShot(clip);
    }

    /// <summary>
    /// Plays a sound effect with a random pitch,
    /// useful for sound effects that are played repeatedly like footsteps
    /// </summary>
    /// <param name="clip">Sound effect which will be played</param>
    /// <param name="source">AudioSource where the sound will be played from</param>
    public void PlaySoundWithRandomPitch(AudioClip clip, AudioSource source)
    {
        SetAudioSource(source);
        _source.pitch = Random.Range(0.5f, 1.5f);
        _source.PlayOneShot(clip);
    }

    public void SetMusic(ZoneTypeEnum zone)
    {
        switch (zone)
        {
            case ZoneTypeEnum.Safe:
                _source.clip = safeZoneMusic;
                break;
            case ZoneTypeEnum.Puzzle:
                _source.clip = puzzleZoneMusic;
                break;
            case ZoneTypeEnum.Battle:
                _source.clip = battleZoneMusic;
                break;
            default:
                Debug.LogError("ERROR: ZoneType not recognized in SoundManager SetMusic");
                break;
        }
    }
}
