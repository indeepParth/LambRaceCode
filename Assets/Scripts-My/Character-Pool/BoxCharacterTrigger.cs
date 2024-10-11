using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxCharacterTrigger : MonoBehaviour
{
    public int boxIndex = 0;
    public List<SpawnCharacter> spawnPoints;

    private void Awake()
    {
        foreach (SpawnCharacter sp in spawnPoints)
        {
            sp.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("car"))
        {
            foreach (SpawnCharacter sp in spawnPoints)
            {
                sp.gameObject.SetActive(true);
                sp.CharacterEnable();
            }
            // int boxIndx = boxIndex != 0 ? boxIndex - 1 : CharacterPoolManager.instance.boxCharacterTriggers.Count - 1;
        }

        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("car"))
        {
            foreach (SpawnCharacter sp in spawnPoints)
            {
                sp.gameObject.SetActive(false);
                sp.CharacterDisable();
            }
        }
    }
}
