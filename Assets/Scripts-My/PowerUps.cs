using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TextCore.Text;

public class PowerUps : MonoBehaviour
{
    public List<Transform> spwanPoints = new List<Transform>();
    private List<int> occupiedPos = new List<int>();
    public int timeBonusFrequncy = 3;
    public int boosterFrequncy = 1;
    public AssetReference timeBonusPref;
    public AssetReference boosterPref;

    public List<PowerupTrigger> poolBooster;
    public List<PowerupTrigger> poolTimeBonus;

    private Quaternion rotattion = new Quaternion(0, 0, 0, 0);

    private int GetSpwanPointIndex()
    {
        int _randomPos = Random.Range(0, spwanPoints.Count);
        while(occupiedPos.Contains(_randomPos))
        {
            _randomPos = Random.Range(0, spwanPoints.Count);
        }
        occupiedPos.Add(_randomPos);
        return _randomPos;
    }

    public void ResetBoosterOnTrigger()
    {
        foreach (var _boost in poolBooster)
        {
            _boost.gameObject.SetActive(false);
            occupiedPos.Remove(_boost.spwanIndex);
        }
        MyGameController.instance.MyManager.carLambController.ResetBoostAmount();
    }
    public void AddBoosterPowerUp()
    {
        for (int i = 0; i < boosterFrequncy; i++)
        {
            SpwanBoosterPowerUp();
        }
    }
    private void SpwanBoosterPowerUp()
    {
        GameObject _Booster = null;
        foreach (PowerupTrigger _boost in poolBooster)
        {
            if (!_boost.gameObject.activeInHierarchy)
            {
                _Booster = _boost.gameObject;
                break;
            }
        }

        int _randomPos = GetSpwanPointIndex();
        if (_Booster != null)
        {
            PowerupTrigger _BoostTrigger = _Booster.GetComponent<PowerupTrigger>();
            _BoostTrigger.spwanIndex = _randomPos;
            _Booster.transform.position = spwanPoints[_randomPos].position;
            _Booster.SetActive(true);
        }
        else
        {
            AsyncOperationHandle<GameObject> handle = boosterPref.InstantiateAsync(spwanPoints[_randomPos].position, rotattion,transform);
            handle.Completed += (obj) =>
            {
                if (obj.Status == AsyncOperationStatus.Succeeded)
                {
                    _Booster = obj.Result;
                    PowerupTrigger _BoostTrigger = _Booster.GetComponent<PowerupTrigger>();
                    _BoostTrigger.spwanIndex = _randomPos;
                    poolBooster.Add(_BoostTrigger);
                }
                else
                {
                    Debug.Log("error on load addressable Beach Character index = " + _randomPos);
                }
            };
        }
    }


    public void TimeBonusOnTrigger(int indx)
    {
        MyGameController.instance.IncreaseTimerAsReward(12);
        occupiedPos.Remove(indx);
        SpwanTimeBonusPowerUp();
    }
    public void AddTimeBonusPowerUpAtStart()
    {
        for (int i = 0; i < timeBonusFrequncy; i++)
        {
            SpwanTimeBonusPowerUp();
        }
    }
    public void SpwanTimeBonusPowerUp()
    {
        GameObject _timeBonus = null;
        foreach (PowerupTrigger _time in poolTimeBonus)
        {
            if (!_time.gameObject.activeInHierarchy)
            {
                _timeBonus = _time.gameObject;
                break;
            }
        }

        int _randomPos = GetSpwanPointIndex();
        if (_timeBonus != null)
        {
            PowerupTrigger _timeTrigger = _timeBonus.GetComponent<PowerupTrigger>();
            _timeTrigger.spwanIndex = _randomPos;
            _timeBonus.transform.position = spwanPoints[_randomPos].position;
            _timeBonus.SetActive(true);
        }
        else
        {
            AsyncOperationHandle<GameObject> handle = timeBonusPref.InstantiateAsync(spwanPoints[_randomPos].position, rotattion, transform);
            handle.Completed += (obj) =>
            {
                if (obj.Status == AsyncOperationStatus.Succeeded)
                {
                    _timeBonus = obj.Result;
                    PowerupTrigger _timeTrigger = _timeBonus.GetComponent<PowerupTrigger>();
                    _timeTrigger.spwanIndex = _randomPos;
                    poolTimeBonus.Add(_timeTrigger);
                }
                else
                {
                    Debug.Log("error on load addressable Beach Character index = " + _randomPos);
                }
            };
        }
    }

}
