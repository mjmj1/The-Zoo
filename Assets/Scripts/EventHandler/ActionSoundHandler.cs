using UnityEngine;

public class ActionSoundHandler : MonoBehaviour
{
    [SerializeField] internal AudioSource walkSound;
    [SerializeField] internal AudioSource sprintSound;
    [SerializeField] internal AudioSource jumpSound;
    [SerializeField] internal AudioSource attackSound;
    [SerializeField] internal AudioSource hitSound;
    [SerializeField] internal AudioSource spinSound;

    private AudioSource current;

    public void PlaySound(AudioSource target)
    {
        if (current != null && current != target)
        {
            current.Stop();
        }

        current = target;

        if (!current.isPlaying)
        {
            current.Play();
        }
    }

    public void StopCurrent()
    {
        if (current != null)
        {
            current.Stop();
            current = null;
        }
    }

    public void PauseAll()
    {
        if (walkSound != null) walkSound.Pause();
        if (sprintSound != null) sprintSound.Pause();
        if (jumpSound != null) jumpSound.Pause();
        if (attackSound != null) attackSound.Pause();
        if (hitSound != null) hitSound.Pause();
        if (spinSound != null) spinSound.Pause();
    }

    public void StopAll()
    {
        if (walkSound != null) walkSound.Stop();
        if (sprintSound != null) sprintSound.Stop();
        if (jumpSound != null) jumpSound.Stop();
        if (attackSound != null) attackSound.Stop();
        if (hitSound != null) hitSound.Stop();
        if (spinSound != null) spinSound.Stop();
    }
}
