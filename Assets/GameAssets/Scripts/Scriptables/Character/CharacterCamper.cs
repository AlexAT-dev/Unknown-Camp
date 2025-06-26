using System;
using UnityEngine;

[CreateAssetMenu(menuName = "GameData/Character/Camper")]
public class CharacterCamper : Character
{
    [Serializable]
    public struct CamperTraits
    {
        [Range(1, 15)] public float speed;
        [Range(1, 10)] public int hp;
        [Range(0, 10)] public int mind;
        [Range(0, 10)] public int strength;
        [Range(0, 10)] public int stealth;
		[TextArea(1, 5)] public string abilityText;

        public void Buff(int value)
        {
            mind = Math.Clamp(mind + value, 0, 10);
            strength = Math.Clamp(strength + value, 0, 10);
            stealth = Math.Clamp(stealth + value, 0, 10);
        }
    }

    public CamperTraits Traits;
    public override CharacterType Type => CharacterType.Camper;
    public override float speed => Traits.speed;
}
