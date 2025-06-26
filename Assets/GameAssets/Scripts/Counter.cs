using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Counter
{
    [SerializeField] private int value;
    [SerializeField] private int min;
    [SerializeField] private int max;

    [SerializeField] private Coroutine coroutine;

    [SerializeField] private UnityEvent onTick;
    [SerializeField] private UnityEvent onDone;

    public bool IsMax => value >= max;
    public bool IsMin => value <= min;
    public int Value => value;

    public void SetMin(int? min)
    {
        this.min = min ?? this.min;
    }

    public void SetMax(int? max)
    {
        this.max = max ?? this.max;
    }

    public void SetToMax()
    {
        value = max;
    }

    public void SetToMin()
    {
        value = min;
    }

    public void StartCoroutine(MonoBehaviour behaviour)
    {
        coroutine = behaviour.StartCoroutine(Coroutine());
    }

    public void StopCoroutine(MonoBehaviour behaviour)
    {
        if (coroutine == null) return;
        behaviour.StopCoroutine(coroutine);
    }

    private IEnumerator Coroutine()
    {
        while (!IsMin)
        {
            yield return new WaitForSeconds(1f);
            value--;
            onTick?.Invoke();
        }

        onDone?.Invoke();
        yield return null;
    }


}
