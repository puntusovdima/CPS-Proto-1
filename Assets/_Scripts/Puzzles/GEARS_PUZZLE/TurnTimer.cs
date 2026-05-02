 using TMPro;
using UnityEngine;

public class TurnTimer : MonoBehaviour
{
    [Header("TIMER SETTINGS")]
    public float turnTime = 15.0f;
    public float currentTime { get; private set; }

    [Header("UI SETTINGS")]
    public TextMeshProUGUI timerText;
    public System.Action OnTimeOut;
    private bool _isRunning;

    private void Update()
    {
        if (!_isRunning) {
            return;
        }
        if (currentTime > 0f)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                _isRunning = false;
                OnTimeOut?.Invoke();
            }
            UpdateUI();
        }
    }
    public void StartTimer()
    {
        currentTime = turnTime;
        _isRunning = true;
        UpdateUI();
    }
    public void StopTimer()
    {
        _isRunning = false;
    }
    public void ResetTimer()
    {
        currentTime = turnTime;
        _isRunning = false;
        UpdateUI();
    }
    private void UpdateUI()
    {
        if (timerText == null)
        {    
            return;
        }

        float time = Mathf.Max(0f, currentTime);
        int minutes = (int)(time / 60f);
        int seconds = (int)(time % 60f);
        int hundredths = (int)((time * 100f) % 100f);
        timerText.text = $"{minutes:00}:{seconds:00}.{hundredths:00}";  
    }
}