using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameData/Skin/Treasure")]
public class SkinsTreasure : ScriptableObject
{
    public string Id;
    public List<SkinItem> Items;
    public List<RarityChance> RarityChances;
    public SkinItem.SkinRarity ElseChance = SkinItem.SkinRarity.B;

    [Serializable]
    public struct RarityChance
    {
        public SkinItem.SkinRarity Rarity;
        public float Value;
    }

    public SkinItem GetRandomItem()
    {
        float random = UnityEngine.Random.Range(1f, 100f);
        SkinItem.SkinRarity gotRarity = ElseChance;

        float a = 0f;
        foreach (RarityChance rarity in RarityChances)
        {
            a+= rarity.Value;

            if (random <= a)
            {
                gotRarity = rarity.Rarity;
                break;
            }
        }

        return GetRarityItem(gotRarity);
    }

    public SkinItem GetRarityItem(SkinItem.SkinRarity rarity)
    {
        List<SkinItem> items = GetRarityItems(rarity);
        return items[UnityEngine.Random.Range(0, items.Count)];
    }

    public List<SkinItem> GetRarityItems(SkinItem.SkinRarity rarity)
    {
        return Items.FindAll(x => x.Rarity == rarity);
    }
}
