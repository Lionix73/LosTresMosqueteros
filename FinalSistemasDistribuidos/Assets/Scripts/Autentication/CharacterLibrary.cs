using System.Collections.Generic;
using UnityEngine;

public class CharacterLibrary : MonoBehaviour
{
    public List<CharacterEntry> characters;

    private Dictionary<string, GameObject> characterDict;

    void Awake()
    {
        characterDict = new Dictionary<string, GameObject>();
        foreach (var entry in characters)
        {
            characterDict.Add(entry.characterId, entry.prefab);
        }
    }

    public GameObject GetCharacterPrefab(string id)
    {
        return characterDict.ContainsKey(id) ? characterDict[id] : null;
    }
}

[System.Serializable]
public class CharacterEntry
{
    public string characterId;
    public GameObject prefab;
}
