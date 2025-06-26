using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Task : MonoBehaviour
{
    [SerializeField] private string taskName;
    [SerializeField] private Escapable escapable;
    [SerializeField] private bool isPersonal;
    [SerializeField, ReadOnly] private int current;
    [SerializeField] private List<InteractiveTask> subtasks;
    [SerializeField] private UnityEvent onDone;

    public string TaskName => taskName;
    public bool IsPersonal => isPersonal;
    public Escapable Escapable => escapable;
    public int Progress
    {
        get
        {
            int count = 0;
            foreach(var subtask in subtasks)
            {
                count += subtask.Progress;
            }
            return count;
        }
    }

    public int MaxProgress
    {
        get
        {
            int count = 0;
            foreach (var subtask in subtasks)
            {
                count += subtask.MaxProgress;
            }
            return count;
        }
    }

    public bool IsDone
    {
        get
        {
            foreach(var subtask in subtasks)
            {
                if(!subtask.IsDone) return false;
            }
            return true;
        }
    }

    public bool InProgress => Progress > 0 && Progress < MaxProgress;

    public InteractiveTask CurrentSubtask => (current >= subtasks.Count) ? null : subtasks[current];

    public void NextSubtask()
    {
        current++;
        if(current >= subtasks.Count) 
        {
            Done();
        }
    }

    private void Done()
    {
        onDone?.Invoke();
        escapable.TaskDone(this);
    }
}
