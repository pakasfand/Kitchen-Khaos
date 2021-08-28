using UnityEngine;

[CreateAssetMenu(fileName = "Dish Type", menuName = "Dirty Dishes/Dish Type", order = 0)]
public class DishType : ScriptableObject
{
    public Sprite UISprite;
    public GameObject prefab;
    public GameObject collectedDish;
}
