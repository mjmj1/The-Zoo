using Static;
using UnityEngine;

public class Tree : InteractionObject
{
    public GameObject FruitPrefab;
    public Transform FruitSpawnPoint;
    public InteractionController InteractionController;

    private void Start()
    {
        //InteractionController = GameObject.FindWithTag(Strings.InteractionController).GetComponent<InteractionController>();
    }

    protected override void CompleteInteraction()
    {
        Debug.Log("³ª¹« Èçµé±â + ¿­¸Å ¶³±À");


        Instantiate(FruitPrefab, FruitSpawnPoint.position, InteractionController.spawnRotation);
    }
}
