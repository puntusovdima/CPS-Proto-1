using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class GearPuzzleWhitTime : MonoBehaviour
{
    public static event Action OnTimedPuzzleComplete;
    public static event Action OnTimedPuzzleFailed;

    [Header("GEAR SETTINGS")]
    [SerializeField] private ChainGear motorGear;
    [SerializeField] private ChainGear finalGear;
    [SerializeField] private float gearTolerance = 800f;
    [SerializeField] private float testDuration = 1.5f;

    [Header("TIMER")]
    public TurnTimer turnTimer;
    public float timeLimit = 30f;

    [Header("FAIL")]
    public Neutral_Robot neutralRobot;
    public bool neutralRobKillsPlayer = true;

    [Header("WIN")]
    public GameObject wallToRemove;

    [Header("REFERENCES")]
    [SerializeField] private PuzzleInteractLogic puzzleLogicRef;
    [SerializeField] private Transform puzzleRoot;

    [Header("TIMER UI")]
    public GameObject timerPanel;

    private bool isSolved = false;
    private bool isFailed = false;
    private bool isChecking = false;
    private bool isRunning = false;
    private PuzzleInteractLogic puzzleLogic;

    private void Start()
    {
        isSolved = false;
        isFailed = false;
        isChecking = false;
        isRunning = false;

        if (puzzleLogicRef != null)
            puzzleLogic = puzzleLogicRef;
        else
        {
            puzzleLogic = GetComponent<PuzzleInteractLogic>();
            if (puzzleLogic == null)
                puzzleLogic = GetComponentInChildren<PuzzleInteractLogic>();
        }

        ChainGear[] allGears;
        if (puzzleRoot != null)
            allGears = puzzleRoot.GetComponentsInChildren<ChainGear>();
        else
            allGears = FindObjectsOfType<ChainGear>();

        List<ChainGear> motors = new List<ChainGear>();
        foreach (var gear in allGears)
        {
            if (gear != null && gear.isMotor) motors.Add(gear);
        }
        if (motorGear == null && motors.Count > 0)
            motorGear = motors[0];
        if (finalGear == null)
        {
            foreach (var gear in allGears)
            {
                if (gear != null && gear != motorGear && gear.name.ToLower().Contains("final"))
                {
                    finalGear = gear;
                    break;
                }
            }
        }

        if (turnTimer != null)
        {
            turnTimer.turnTime = timeLimit;
            turnTimer.OnTimeOut += OnTimeExpired;
        }
    }

    private void OnDestroy()
    {
        if (turnTimer != null)
            turnTimer.OnTimeOut -= OnTimeExpired;
    }

    public void InitializePuzzle()
    {
        isSolved = false;
        isFailed = false;
        isChecking = false;
        isRunning = false;
    }

    public bool IsPuzzleSolved() => isSolved;

    public void StartPuzzle()
    {
        Debug.Log($"[GearPuzzleWhitTime] StartPuzzle! puzzleLogic={(puzzleLogic != null ? "OK" : "NULL")}");
        if (isSolved || isFailed) return;
        isSolved = false;
        isFailed = false;
        isChecking = false;
        isRunning = true;
        if (turnTimer != null)
        {
            turnTimer.turnTime = timeLimit;
            turnTimer.StartTimer();
        }
        if (timerPanel != null)
            timerPanel.SetActive(true);
    }

    public void OnGearPlaced(GearSlotPuzzle slot, GearDragSystem gear)
    {
        if (isSolved || isFailed) return;
        if (isChecking) return;

        GearSlotPuzzle[] allSlots = FindObjectsOfType<GearSlotPuzzle>();
        foreach (var s in allSlots)
        {
            if (s.isPossibleFinalSlot && s.IsOccupied())
            {
                StartCoroutine(PerformGearTestSync(s.myChainGear));
                break;
            }
        }
    }

    public void OnGearRemoved(GearSlotPuzzle slot, GearDragSystem gear)
    {
    }

    private IEnumerator PerformGearTestSync(ChainGear placedGear)
    {
        isChecking = true;
        if (motorGear != null)
            motorGear.isMotor = true;

        yield return new WaitForSeconds(testDuration);

        if (isSolved || isFailed)
        {
            isChecking = false;
            yield break;
        }

        CheckGearPuzzleCompletion(placedGear);
        isChecking = false;
    }

    private void CheckGearPuzzleCompletion(ChainGear placedGear)
    {
        Debug.Log($"[GearPuzzleWhitTime] CheckGearPuzzleCompletion - finalGear={(finalGear != null ? finalGear.name : "NULL")}, placedGear={(placedGear != null ? placedGear.name : "NULL")}");

        if (finalGear == null || placedGear == null)
        {
            Debug.LogWarning($"[GearPuzzleWhitTime] NULL - returning");
            return;
        }

        float placedTeethVelocity = placedGear.currentSpeed * placedGear.teeth;
        float finalTeethVelocity = finalGear.currentSpeed * finalGear.teeth;
        float diff = Mathf.Abs(Mathf.Abs(placedTeethVelocity) - Mathf.Abs(finalTeethVelocity));

        Debug.Log($"[GearPuzzleWhitTime] placedSpeed={placedGear.currentSpeed:F2}, diff={diff:F2}, tolerance={gearTolerance}");

        if (Mathf.Abs(placedGear.currentSpeed) < 0.1f)
        {
            Debug.LogWarning($"[GearPuzzleWhitTime] Speed too low - returning");
            return;
        }

        if (diff < gearTolerance)
        {
            Debug.Log($"[GearPuzzleWhitTime] COMPLETANDO!");
            CompleteGearsPuzzle(placedGear);
        }
        else
        {
            Debug.LogWarning($"[GearPuzzleWhitTime] diff > tolerance - not completing");
        }
    }

    private void CompleteGearsPuzzle(ChainGear lastGearInChain)
    {
        if (isSolved) return;
        isSolved = true;
        isRunning = false;

        Debug.Log($"[GearPuzzleWhitTime] RESOLVIENDO! puzzleLogic={(puzzleLogic != null ? "OK" : "NULL")}");

        if (finalGear != null && lastGearInChain != null)
        {
            finalGear.inputGear = lastGearInChain;
            lastGearInChain.RegisterFollower(finalGear);
            finalGear.isMotor = false;
        }
        if (motorGear != null)
            motorGear.isMotor = true;
        if (turnTimer != null)
            turnTimer.StopTimer();
        if (wallToRemove != null)
            Destroy(wallToRemove);
        if (timerPanel != null)
            timerPanel.SetActive(false);
        if (neutralRobot != null)
            neutralRobot.StartFollowing();
        if (puzzleLogic != null)
        {
            Debug.Log($"[GearPuzzleWhitTime] LLAMANDO ClosePuzzle!");
            puzzleLogic.ClosePuzzle();
        }
        OnTimedPuzzleComplete?.Invoke();
    }

    private void OnTimeExpired()
    {
        if (isSolved || isFailed) return;
        isFailed = true;
        isRunning = false;
        if (timerPanel != null)
            timerPanel.SetActive(false);
        if (neutralRobot != null && neutralRobKillsPlayer)
            neutralRobot.KillPlayer();
        if (puzzleLogic != null)
            puzzleLogic.ClosePuzzle();
        OnTimedPuzzleFailed?.Invoke();
    }

    public bool IsFailed() => isFailed;
}