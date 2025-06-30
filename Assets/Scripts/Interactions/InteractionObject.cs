using UnityEngine;
using System.Collections;

public abstract class InteractionObject : MonoBehaviour
{
    public float InteractionTime = 5f;
    protected bool _isInteracting = false;

    public virtual void StartInteraction()
    {
        if (!_isInteracting)
        {
            _isInteracting = true;
            StartCoroutine(InteractionRoutine());
        }
    }


    protected virtual IEnumerator InteractionRoutine()
    {
        float timer = 0f;
        while (timer < InteractionTime)
        {
            if (!_isInteracting) yield break;
            timer += Time.deltaTime;
            yield return null;
        }

        CompleteInteraction();
    }

    protected abstract void CompleteInteraction();

    public virtual void CancelInteraction()
    {
        _isInteracting = false;
    }
}
