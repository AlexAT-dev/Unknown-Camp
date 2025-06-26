using TMPro;
using UnityEngine;

public class TaskListNode : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI taskName;
    [SerializeField] private TextMeshProUGUI taskProgress;

    private Task task;
    public Task Task => task;

    private Color colorDone => GameController.IsCamper ? Color.green : Color.red;

    public void Initialize(Task task)
    {
        this.task = task;
        taskName.text = task.TaskName;
        taskName.ForceMeshUpdate();
        RectTransform rt = GetComponent<RectTransform>();
        float height = rt.sizeDelta.y + (taskName.textInfo.lineCount - 1) * 30;
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        UpdateProgress();
    }

    public void Initialize(string header)
    {
        taskName.text = header;
        taskName.color = new Color32(0, 180, 255, 255);
    }

    public void UpdateProgress()
    {
        taskProgress.text = $"{task.Progress}/{task.MaxProgress}";

        Color color = task.IsDone ? colorDone : task.InProgress? Color.yellow : Color.white;
        taskName.color = color;
        taskProgress.color = color;
    }
}
