using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuNavigationInitializer : MonoBehaviour
{
    public GameObject firstSelected;

    void OnEnable()
    {
        if (firstSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // Limpiar primero
            EventSystem.current.SetSelectedGameObject(firstSelected); // Asignar nuevo
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            PanelManager.Instance.GoBack();
        }
    }

}
