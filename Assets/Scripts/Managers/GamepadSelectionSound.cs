using UnityEngine;
using UnityEngine.EventSystems;

public class GamepadSelectionSound : MonoBehaviour
{
    private GameObject lastSelected;
    private float lastSoundTime;
    public float soundCooldown = 0.1f;

    void Update()
    {
        GameObject current = EventSystem.current.currentSelectedGameObject;

        if (current != null && current != lastSelected)
        {
            if (Time.unscaledTime - lastSoundTime > soundCooldown)
            {
                AudioManager.Instance.PlayCursorSound();
                lastSoundTime = Time.unscaledTime;
            }
            lastSelected = current;
        }
    }
}
