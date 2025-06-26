using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionPanel : MonoBehaviour
{
    public static ActionPanel Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public enum ActionPanelType
    {
        None,
        SoulsShop,
        Tasks
    }

    [SerializeField] private Transform panel;
    [SerializeField, ReadOnly] private ActionPanelType currentPanel;

    [Header("Tasks")]
    [SerializeField] private Transform tasksPanel;
    [SerializeField] private Button tasksButton;
    [SerializeField] private Escapable escapable;
    [SerializeField] private TaskListNode nodePrefab;
    [SerializeField] private TaskListNode headerPrefab;
    [SerializeField] private Transform content;
    [SerializeField] private List<TaskListNode> nodes;

    [Header("Shop")]
    [SerializeField] private Transform shopPanel;
    [SerializeField] private Button shopButton;
    [SerializeField] private TextMeshProUGUI soulsCount;
    [SerializeField] private int souls;
    [SerializeField] private Button speedButton;
    [SerializeField] private Button phaselessButton;

    public static Button TasksButton => Instance.tasksButton;
    public static Button ShopButton => Instance.shopButton;

    public void ChangePanel(int panelIndex)
    {
        ActionPanelType newPanel = (ActionPanelType)panelIndex;

        if (newPanel == currentPanel || newPanel == ActionPanelType.None)
        {
            currentPanel = ActionPanelType.None;
            ShowPanel(false);
        }
        else
        {
            ShowPanel(true);
            SetPanel(newPanel);
        }
    }

    public void SetPanel(ActionPanelType type)
    {
        currentPanel = type;
        tasksPanel.gameObject.SetActive(type == ActionPanelType.Tasks);
        shopPanel.gameObject.SetActive(type == ActionPanelType.SoulsShop);
    }

    private void ShowPanel(bool show)
    {
        panel.gameObject.SetActive(show);
    }

    #region Tasks
    public void RenderTasks()
    {
        if (escapable == null) return;

        List<Task> personalTasks = new List<Task>();
        foreach(Task task in escapable.Tasks)
        {
            if (task.IsPersonal)
            {
                personalTasks.Add(task);
                continue;
            }

            RenderTask(task);
        }
        if (personalTasks.Count == 0 || GameController.LocalRole == Character.CharacterType.Catcher) return;

        RenderHeader("= Personal tasks =");
        foreach (Task task in personalTasks)
        {
            RenderTask(task);
        }
    }

    private void RenderTask(Task task)
    {
        TaskListNode node = Instantiate(nodePrefab, content);
        if (node == null) return;

        node.Initialize(task);
        nodes.Add(node);
    }

    private void RenderHeader(string header)
    {
        TaskListNode node = Instantiate(headerPrefab, content);
        if (node == null) return;

        node.Initialize(header);
        nodes.Add(node);
    }

    public void Clear()
    {
        nodes.Clear();
        foreach (Transform obj in content)
        {
            Destroy(obj.gameObject);
        }
    }

    public void SetEscapable(Escapable escapable)
    {
        this.escapable = escapable;
        Clear();
        RenderTasks();
    }

    public void UpdateNode(Task task)
    {
        if (task.Escapable != escapable) return;
        foreach (TaskListNode node in nodes)
        {
            if(node.Task == task)
            {
                node.UpdateProgress();
            }
        }
    }
    #endregion

    #region SoulsShop
    public void AddSouls(int add)
    {
        souls += add;
        UpdateSoulsShop();
    }

    private void UpdateSoulsShop()
    {
        soulsCount.text = $"{souls}";
    }

    public void InstantAttack()
    {
        if(souls >= 200 && GameController.Phase == GameController.PhaseState.Prepare)
        {
            souls -= 200;
            InteractionCatcherManager.Instance.InstantAttack();
            UpdateSoulsShop();
        }
    }

    public void LessPrepare()
    {
        if (souls >= 80)
        {
            souls -= 80;
            InteractionCatcherManager.PrepareCounter.SetMax(30);
            UpdateSoulsShop();
            phaselessButton.interactable = false;
        }
    }

    public void IncreaseSpeed()
    {
        if (souls >= 30)
        {
            souls -= 30;
            GameController.LocalCatcher.Movement.AddSpeedMultiplayer("Bonus", 1f);
            UpdateSoulsShop();
            speedButton.interactable = false;
        }
    }
    #endregion
}
