using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance;

    [System.Serializable]
    public class PanelInfo
    {
        public MenuPanel id;
        public GameObject panelObject;
    }

    public List<PanelInfo> panels;

    private Stack<MenuPanel> history = new();
    private MenuPanel? currentPanel;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        ShowPanel(MenuPanel.Main, false);
    }

    public void ShowPanel(MenuPanel id, bool addToHistory = true)
    {
        if (currentPanel != null && addToHistory)
            history.Push(currentPanel.Value);

        foreach (var panel in panels)
            panel.panelObject.SetActive(panel.id == id);

        currentPanel = id;
    }

    public void GoBack()
    {
        if (history.Count > 0)
        {
            MenuPanel lastPanel = history.Pop();
            ShowPanel(lastPanel, false);
        }
    }
}
