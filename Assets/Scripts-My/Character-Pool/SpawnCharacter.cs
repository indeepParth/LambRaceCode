using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SpawnCharacter : MonoBehaviour
{
    public bool isBeachChar = false;
    public int randomChar = 0;
    GameObject _character = null;
    private Vector3 modelScale = new Vector3(1.5f, 1.5f, 1.5f);
    private Quaternion modelRotation = new Quaternion(0, 0, 0, 0);

    public void CharacterEnable()
    {
        _character = null;
        if (isBeachChar)
        {            
            _character = CharacterPoolManager.instance.GetCharacter(this);
            if (_character == null)
            {
                randomChar = UnityEngine.Random.Range(0, CharacterPoolManager.instance.prefBeachCharacter.Count);
                AsyncOperationHandle<GameObject> handle = CharacterPoolManager.instance.prefBeachCharacter[randomChar].InstantiateAsync(transform.position, modelRotation, transform);
                handle.Completed += (obj) =>
                {
                    if (obj.Status == AsyncOperationStatus.Succeeded)
                    {
                        _character = obj.Result;
                        _character.transform.localScale = modelScale;
                        CharacterInfo _chara = obj.Result.GetComponent<CharacterInfo>();
                        CharacterPoolManager.instance.poolBeachCharacter.Add(_chara);
                    }
                    else
                    {
                        Debug.Log("error on load addressable Beach Character index = " + randomChar);
                    }
                };
            }
            else
            {
                _character.transform.parent = transform;
                _character.transform.position = transform.position;
                _character.transform.localRotation = modelRotation;
                _character.gameObject.SetActive(true);                
            }
        }
        else
        {            
            _character = CharacterPoolManager.instance.GetCharacter(this);
            if (_character == null)
            {
                randomChar = UnityEngine.Random.Range(0, CharacterPoolManager.instance.prefCityCharacters.Count);
                AsyncOperationHandle<GameObject> handle = CharacterPoolManager.instance.prefCityCharacters[randomChar].InstantiateAsync(transform.position, modelRotation, transform);
                handle.Completed += (obj) =>
                {
                    if (obj.Status == AsyncOperationStatus.Succeeded)
                    {
                        _character = obj.Result;
                        _character.transform.localScale = modelScale;
                        CharacterInfo _chara = obj.Result.GetComponent<CharacterInfo>();
                        CharacterPoolManager.instance.poolCityCharacters.Add(_chara);
                    }
                    else
                    {
                        Debug.Log("error on load addressable City Character index = " + randomChar);
                    }
                };
            }
            else
            {
                _character.transform.parent = transform;
                _character.transform.position = transform.position;
                _character.transform.localRotation = modelRotation;
                _character.gameObject.SetActive(true);
            }
        }
    }
    public void CharacterDisable()
    {
        _character.gameObject.SetActive(false);
        _character.transform.parent = CharacterPoolManager.instance.poolParent;
    }
}
