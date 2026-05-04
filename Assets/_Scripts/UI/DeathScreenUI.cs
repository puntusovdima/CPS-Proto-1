using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathScreenUI : MonoBehaviour
{
    public Image blackFade;
    public GameObject deathPanel;

    private void Awake()
    {
        blackFade.color = new Color(0, 0, 0, 1);
        deathPanel.SetActive(false);
    }

    public void PlayDeathSequence()
    {
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return StartCoroutine(FadeImage(blackFade, 0, 1, 1f));
        deathPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private IEnumerator FadeImage(Image img, float from, float to, float duration)
    {
        float t = 0;
        Color c = img.color;
        while (t < duration)
        {
            float a = Mathf.Lerp(from, to, t / duration);
            img.color = new Color(c.r, c.g, c.b, a);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        img.color = new Color(c.r, c.g, c.b, to);
    }

    public void OnRestart()
    {
        deathPanel.SetActive(false);
        blackFade.color = new Color(0, 0, 0, 0);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayerController.Instance.RespawnCoroutine();
        PlayerController.Instance.SetPause(false);
    }
}