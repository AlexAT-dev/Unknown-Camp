using System;
using UnityEngine;

[CreateAssetMenu(menuName = "GameData/Character/Catcher")]
public class CharacterCatcher : Character
{
    [Serializable]
    public struct CatcherTraits
    {
        [Range(1, 15)] public float speed;
        public int preparePhase;
        public int attackPhase;
        public int catchCD;
        [TextArea(1, 5)] public string abilityText;
    }

    public CatcherTraits Traits;
    public override CharacterType Type => CharacterType.Catcher;
    public override float speed => Traits.speed;
}
