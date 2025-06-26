using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "GameData/Task/Item")]
public class Item : ScriptableObject
{
    public string Name;
    public Sprite Sprite;
}
