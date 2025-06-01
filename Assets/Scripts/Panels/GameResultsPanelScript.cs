using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameResultsPanelScript : MonoBehaviour
{
    public Transform contentContainer;
    private string logFilePath;
    public Button backButton;

    void Start()
    {
        logFilePath = Path.Combine(Application.persistentDataPath, "GameResults.txt");

        SetupContentContainer();
        LoadResults();
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackPresssed);
        }
    }

    void SetupContentContainer()
    {
        var layout = contentContainer.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = contentContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.spacing = 10f;
        }

        var fitter = contentContainer.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = contentContainer.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }
    }

    void LoadResults()
    {
        if (!File.Exists(logFilePath))
        {
            Debug.LogWarning("GameResults.txt not found!");
            return;
        }

        string[] lines = File.ReadAllLines(logFilePath);
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine))
                continue;

            string[] parts = trimmedLine.Split(';');
            if (parts.Length < 3)
            {
                Debug.LogWarning("Formato inválido, línea ignorada: " + trimmedLine);
                continue;
            }

            GameObject entry = CreateStyledEntry(trimmedLine);
            entry.transform.SetParent(contentContainer, false);
        }
    }

    GameObject CreateStyledEntry(string line)
    {
        GameObject container = new GameObject("ResultLine");
        RectTransform rect = container.AddComponent<RectTransform>();
        HorizontalLayoutGroup hLayout = container.AddComponent<HorizontalLayoutGroup>();
        hLayout.childAlignment = TextAnchor.MiddleLeft;
        hLayout.spacing = 20f;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = false;

        LayoutElement layoutElement = container.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 35;

        string[] parts = line.Split(';');
        if (parts.Length < 3)
        {
            Debug.LogWarning("Formato incorrecto en línea: " + line);
            return container;
        }

        string result = parts[0];
        string date = parts[1];
        string time = parts[2];

        AddTextTo(container.transform, result, result.Contains("Win") ? new Color(0.2f, 0.6f, 0.2f) : new Color(0.8f, 0.2f, 0.2f));
        AddTextTo(container.transform, $"Fecha: {date}", Color.black);
        AddTextTo(container.transform, $"Tiempo: {time} s", Color.black);

        return container;
    }

    void AddTextTo(Transform parent, string text, Color color)
    {
        GameObject go = new GameObject("TextPart");
        go.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = color;
        tmp.enableAutoSizing = false;

        LayoutElement layout = go.AddComponent<LayoutElement>();
        layout.preferredWidth = 300;
        layout.flexibleWidth = 0;
    }
    
    private void OnBackPresssed()
    {
        PanelManager.Instance.GoBack();
    }
}
