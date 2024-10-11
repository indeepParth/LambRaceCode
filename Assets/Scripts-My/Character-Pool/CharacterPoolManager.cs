using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CharacterPoolManager : MonoBehaviour
{
    public static CharacterPoolManager instance = null;

    public List<BoxCharacterTrigger> boxCharacterTriggers;

    public Transform poolParent;

    public List<AssetReference> prefBeachCharacter;
    public List<CharacterInfo> poolBeachCharacter;

    public List<AssetReference> prefCityCharacters;
    public List<CharacterInfo> poolCityCharacters;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject GetCharacter(SpawnCharacter spawn)
    {
        GameObject _character = null;
        if (spawn.isBeachChar)
        {
            foreach(CharacterInfo chara in poolBeachCharacter)
            {
                if(chara.indexPref == spawn.randomChar && !chara.gameObject.activeInHierarchy)
                {
                    _character = chara.gameObject;
                    break;
                }
            }
        }
        else
        {
            foreach (CharacterInfo chara in poolCityCharacters)
            {
                if (chara.indexPref == spawn.randomChar && !chara.gameObject.activeInHierarchy)
                {
                    _character = chara.gameObject;
                    break;
                }
            }
        }
        return _character;
    }
}
