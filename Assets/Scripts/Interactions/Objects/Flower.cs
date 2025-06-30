using UnityEngine;

public class FlowerObject : InteractionObject
{
    protected override void CompleteInteraction()
    {
        Debug.Log("꽃 채집하기");
    }
}
