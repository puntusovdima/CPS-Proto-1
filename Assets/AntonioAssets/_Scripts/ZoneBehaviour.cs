using UnityEngine;

public class ZoneBehaviour : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private ZoneTypeEnum zoneType;

    private AudioSource _source;
    
    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        SoundManager.Instance.SetAudioSource(_source);
        SoundManager.Instance.SetMusic(zoneType);
    }
}
