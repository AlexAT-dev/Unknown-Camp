using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameData/CharactersList", fileName = "CharactersList")]
public class CharactersList : ScriptableObject
{
    public List<Character> Characters
    {
        get
        {
            List<Character> list = new List<Character>();
            foreach(Character camper in Campers)
            {
                list.Add(camper);
            }
            foreach (Character catcher in Catchers)
            {
                list.Add(catcher);
            }
            return list;
        }
    }

    public List<CharacterCamper> Campers;
    public List<CharacterCatcher> Catchers;
    public CharacterCamper DefaultCamper;
    public CharacterCatcher DefaultCatcher;

    public Character FindCharacter(string name)
    {
        if (string.IsNullOrEmpty(name)) return DefaultCamper;
        return Characters.Find(x => x.Name == name);
    }

    public Character FindCharacter(string name, Character.CharacterType type)
    {
        switch(type)
        {
            case Character.CharacterType.Camper: return FindCharacterCamper(name);
            case Character.CharacterType.Catcher: return FindCharacterCatcher(name);
            default: return FindCharacter(name);
        }
    }

    public CharacterCamper FindCharacterCamper(string name)
    {
        if (string.IsNullOrEmpty(name)) return DefaultCamper;
        return Campers.Find(x => x.Name == name);
    }

    public CharacterCatcher FindCharacterCatcher(string name)
    {
        if (string.IsNullOrEmpty(name)) return DefaultCatcher;
        return Catchers.Find(x => x.Name == name);
    }
}
