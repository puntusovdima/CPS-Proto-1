using UnityEngine;

public class PuzzleDoorBehaviour : MonoBehaviour
{
    [Header("SFX References")] 
    [SerializeField] private AudioClip doorOpenSfx;
    
    public void OpenDoor()
    {
        // 1. Check for the SoundManager Singleton
        if (SoundManager.Instance == null)
        {
            Debug.LogError($"[PuzzleDoorBehaviour] SoundManager.Instance is NULL on {gameObject.name}. Make sure a SoundManager exists in your scene.");
        }
        // 2. Check if the audio clip was actually assigned in the Inspector
        else if (doorOpenSfx == null)
        {
            Debug.LogWarning($"[PuzzleDoorBehaviour] doorOpenSfx is missing on {gameObject.name}. No sound will play.");
        }
        // 3. Only if both are valid, play the sound
        else
        {
            SoundManager.Instance.PlaySound(doorOpenSfx);
        }

        // 4. Finally, destroy the door object
        Destroy(gameObject);
    }
}
