using UnityEngine;

[CreateAssetMenu(menuName = "GameData/Skin/Item")]
public class SkinItem : ScriptableObject
{
    public enum SkinRarity
    {
        S,
        A,
        B,
        C,
        D
    }


    public string Name;
    public string CodeName;
    public Sprite StandSprite;
    public GameObject Prefab;
    public Sprite IconSprite;
    public SkinRarity Rarity;
}
