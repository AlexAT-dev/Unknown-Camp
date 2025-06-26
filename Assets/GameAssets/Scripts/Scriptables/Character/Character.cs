using System.Collections.Generic;
using UnityEngine;

public abstract class Character : ScriptableObject
{
    public enum CharacterType
    {
        Camper,
        Catcher
    }

    public string Name;
    public Sprite StandSprite;
    public GameObject Prefab;
    public Sprite IconSprite;
    public List<SkinItem> Skins;

    public SkinItem FindSkin(string skinName)
    {
        if (string.IsNullOrEmpty(skinName)) return null;
        return Skins.Find(x => x.CodeName == skinName);
    }
    public abstract CharacterType Type { get; }
    public abstract float speed { get; }
}
