using UnityEngine.Events;
using UnityEngine;
using TMPro;

public class TaskListEscapable : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Escapable escapable;
    [SerializeField] private SelectableButton button;
    [SerializeField] private bool isDefault;

    [Header("Campers Counter")]
    [SerializeField] private TextMeshProUGUI campersCount;

    private void Start()
    {
        button.OnSelect.AddListener(() =>
        {
            ActionPanel.Instance.SetEscapable(escapable);
        });
        UpdateCampersCount();
        escapable.OnSomeoneEscaped.AddListener(() =>
        {
            UpdateCampersCount();
        });

        if (isDefault)
        {
            button.Select();
        }
    }

    private void UpdateCampersCount()
    {
        string of = escapable.CampersCapacity == 0 ? "" : $"/{escapable.CampersCapacity}";
        campersCount.text = $"{escapable.CampersUsed}{of}";
        campersCount.color = escapable.IsFree ? Color.white : Color.red;
    }
}
