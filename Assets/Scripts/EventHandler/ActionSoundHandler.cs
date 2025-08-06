using UnityEngine;

public class ActionSoundHandler : MonoBehaviour
{
    //[SerializeField] private AudioSource walkSound;
    //[SerializeField] private AudioSource sprintSound;
    //[SerializeField] private AudioSource jumpSound;
    //[SerializeField] private AudioSource attackSound;
    //[SerializeField] private AudioSource hitSound;
    //[SerializeField] private AudioSource spinSound;

    private AudioSource current;

    public void PlaySound(AudioSource target)
    {
        if (current != null && current != target)
        {
            current.Stop(); // �ٸ� ����� �ڵ� ����
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
        foreach (var s in GetComponents<AudioSource>())
        {
            if (s.isPlaying)
                s.Pause();
        }
    }

    public void ResumeAll()
    {
        foreach (var s in GetComponents<AudioSource>())
        {
            if (!s.isPlaying)
                s.UnPause();
        }
    }
}
