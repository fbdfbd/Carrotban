using System;
using UnityEngine;
using TMPro;
using System.Collections;

public class HeartTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    private Coroutine timerRoutine;

    public static event Action OnTimerFinishedEvent;

    public void StartTimer(int seconds = 300)
    {
        if (timerRoutine != null)
            StopCoroutine(timerRoutine);
        timerRoutine = StartCoroutine(TimerRoutine(seconds));
    }

    private IEnumerator TimerRoutine(int seconds)
    {
        float end = Time.realtimeSinceStartup + seconds;

        while (Time.realtimeSinceStartup < end)
        {
            float remain = end - Time.realtimeSinceStartup;

            int secondsLeft = Mathf.FloorToInt(remain);
            int min = secondsLeft / 60;
            int sec = secondsLeft % 60;
            timerText.text = $"{min:D2}:{sec:D2}";

            yield return null;
        }

        timerText.text = "00:00";
        OnTimerFinished();
    }



    private void OnTimerFinished()
    {
        OnTimerFinishedEvent?.Invoke();
        Debug.Log("하트 리젠 완료!");
    }
}
