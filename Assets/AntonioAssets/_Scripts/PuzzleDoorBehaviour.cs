using UnityEngine;

public class PuzzleDoorBehaviour : MonoBehaviour
{
    [Header("SFX References")] 
    [SerializeField] private AudioClip doorOpenSfx;
    
    public void OpenDoor()
    {
        Destroy(gameObject);
        SoundManager.Instance.PlaySound(doorOpenSfx);
    }
}
