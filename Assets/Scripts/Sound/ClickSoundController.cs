using Scriptable;
using UnityEngine;
using UnityEngine.UI;

public class ClickSoundController : MonoBehaviour
{
    [SerializeField] private AudioClip clickClip;

    private void Awake()
    {
        var buttons = GetComponentsInChildren<Button>(true);
        foreach (var button in buttons)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null && clickClip != null)
        {
            var pos = Camera.main ? Camera.main.transform.position : Vector3.zero;
            AudioManager.Instance.PlaySfx(clickClip, pos);
        }
    }
}
