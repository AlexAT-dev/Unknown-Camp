using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public static ProgressBar Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [Header("Components")]
    [SerializeField] private GameObject progressBar;
    [SerializeField] private Image progressMask;
    private Coroutine fillCoroutine;
    private float fillTime;

    public void StartProgress(float fillTime, Vector3 position, bool stopOnMove, Action onFinish)
    {
        if (fillTime <= 0)
        {
            onFinish?.Invoke();
            return;
        }

        progressBar.SetActive(true);
        transform.position = position;

        if (fillCoroutine != null)
            StopCoroutine(fillCoroutine);

        this.fillTime = fillTime;
        fillCoroutine = StartCoroutine(FillProgressBar(onFinish, stopOnMove));
    }

    public void StopProgress()
    {
        if (fillCoroutine != null) StopCoroutine(fillCoroutine);
        
        progressBar.SetActive(false);
        progressMask.fillAmount = 0f;
    }

    private IEnumerator FillProgressBar(Action onFinish, bool stopOnMove)
    {
        float elapsedTime = 0f;
        float fillPercentage = 0f;

        while (fillPercentage < 1f)
        {
            if (GameController.Instance && ((stopOnMove && GameController.LocalPlayer.Movement.IsMoving) || 
                (GameController.IsCamper && GameController.LocalCamper.State != PlayerCamper.StateTypes.Default)))
            {
                StopProgress();
                GameController.LocalPlayer.TriggerRefresh();
                yield break;
            }

            elapsedTime += Time.deltaTime;
            fillPercentage = elapsedTime / fillTime;
            progressMask.fillAmount = fillPercentage;
            yield return null;
        }

        progressMask.fillAmount = 1f;
        onFinish?.Invoke();
        StopProgress();
    }
}
