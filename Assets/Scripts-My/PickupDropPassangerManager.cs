using DG.Tweening;
using PG;
using SplineMesh;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TextCore.Text;

public class PickupDropPassangerManager : MonoBehaviour
{
    public GameObject PickupDropPoint;
    public GameObject pickupParticle;
    public GameObject dropParticle;
    public ParticleSystem heartParicleOfPassanger;

    public List<AssetReference> prefPassangerCharacter;
    public List<GameObject> poolPassangerCharacter;
    [NonSerialized]
    public GameObject passanger = null;
    Animator passangerAnimator;
    NavMeshAgent passangerNavmeshAgent;
    public NavMeshSurface meshSurface;
    private Vector3 modelScale = new Vector3(1.5f, 1.5f, 1.5f);
    private Quaternion modelRotation = new Quaternion(0, 0, 0, 0);

    public Animator door1Right;
    public int walkSpeedSquence = 5;
    public Transform lookAtDoor;
    public Transform doorPoint;
    public Transform[] carCorner1; // front left corner
    public Transform[] carCorner2; // rear left corner
    public Transform[] carCorner3; // right side corner all
    public Transform[] carCornersAll;
    public Transform nearestCarCorner;
    private float carCornerDistance;
    private float carCornerNearestDistance = 10000;
    private bool isWalkingPauseAnim = false;

    public int minDistance = 300;
    public float currCarAtDistance;
    public int prevPointAtDistance = -1; // car at this point
    public bool isPickupGirl = true;
    public bool leftSideOfRoad = false;
    public bool lookAtCar = true;

    private bool forwardDir;
    public Transform directionalArrow;
    public Transform pathZonePoint;
    public Transform pathZonePointTest;
    private bool lookAtPoint = false;
    public Transform extrusionSegmentParent;
    public int curCarSegmentIndex;
    public int curDropSegmentIndex;
    private List<ExtrusionSegment> extrusionSegment = new List<ExtrusionSegment>();

    private int bestHeart = 0; // in single game collection of heart

    bool isActiveNavmesh = false;
    private void Update()
    {
        if (isActiveNavmesh && passangerNavmeshAgent != null)
        {
            if(passangerNavmeshAgent.remainingDistance > passangerNavmeshAgent.stoppingDistance)
            {
                
            }
            else
            {
                isActiveNavmesh = false;
                passangerNavmeshAgent.enabled = false;
                Vector3 lookAt = lookAtDoor.position;
                lookAt.y = passanger.transform.position.y;
                passanger.transform.LookAt(lookAt);
                OnCompleteAnimationWalkToDoor();
            }
        }
    }

    private void Start()
    {
        directionalArrow.gameObject.SetActive(false);
        bestHeart = 0;

        if (MyGameController.instance.gameMode == GameMode.DateRush)
        {
            MyGameController.instance.MyManager.carLambController.powerUps.AddTimeBonusPowerUpAtStart();
            Invoke("DelayOnStart", 2);
        }
    }
    void DelayOnStart()
    {
        for (int i = 0; i < extrusionSegmentParent.childCount; i++)
        {
            extrusionSegment.Add(extrusionSegmentParent.GetChild(i).GetComponent<ExtrusionSegment>());
        }

        GetPickDropPointLocation();
    }

    public void GetPickDropPointLocation()
    {
        if (MyGameController.instance.isGameOver)
            return;

        CurveSample curveSample = MyGameController.instance.MyManager.spline.GetSampleAtDistance(GetPointAtDistance());// (GetPointAtDistance());
        Debug.Log("PickDropPointLocation = " + curveSample.location);
        leftSideOfRoad = UnityEngine.Random.Range(0, 2) == 0 ? true : false;
        PickupDropPoint.transform.localPosition = curveSample.location; //new Vector3(curveSample.location.x + (leftSideOfRoad ? -5 : 5), curveSample.location.y, curveSample.location.z);
        PickupDropPoint.transform.localRotation = curveSample.Rotation;
        PickupDropPoint.gameObject.SetActive(true);
        if (isPickupGirl)
        {
            //dropParticle.SetActive(false);
            //pickupParticle.SetActive(true);
            PassangerCharacterEnable();
            lookAtCar = true;
        }
        else
        {
            //pickupParticle.SetActive(false);
            //dropParticle.SetActive(true);
        }

        for (int i = 0; i < extrusionSegment.Count; i++)
        {
            if (extrusionSegment[i].curve.n1 == curveSample.curve.n1 && extrusionSegment[i].curve.n2 == curveSample.curve.n2)
            {
                curDropSegmentIndex = i;
                break;
            }
        }
        GetCarLocationOnSpline();
    }

    public int GetPointAtDistance()
    {
        if (prevPointAtDistance < 0)
        {
            prevPointAtDistance = 75;
            return prevPointAtDistance;
        }

        int point = 0;
        // from car current point avoid distance on both side...
        int minAvoidRange = prevPointAtDistance < minDistance ? 0 : prevPointAtDistance - minDistance;
        int maxAvoidRange = prevPointAtDistance > ((int)MyGameController.instance.MyManager.spline.Length-minDistance) ? 
            (int)MyGameController.instance.MyManager.spline.Length : prevPointAtDistance + minDistance;

        //Debug.Log("minAvoidRange = " + minAvoidRange);
        //Debug.Log("maxAvoidRange = " + maxAvoidRange);
        point = UnityEngine.Random.Range(0, (int)MyGameController.instance.MyManager.spline.Length);
        while (point > minAvoidRange && point < maxAvoidRange)
        {
            point = UnityEngine.Random.Range(0, (int)MyGameController.instance.MyManager.spline.Length);
        }
        prevPointAtDistance = point;
        Debug.Log("Pickup Drop Point = " + point);
        return point;
    }

    public void OnCarTriggerPickupDropLocationDelay()
    {
        Invoke("OnCarTriggerPickupDropLocation", 1);
    }
    private void OnCarTriggerPickupDropLocation()
    {
        if (MyGameController.instance.isGameOver)
            return;

        MyGameController.instance.MyManager.carLambController.SetCarChildPosition();
        lookAtCar = false;
        lookAtPoint = false;
        directionalArrow.gameObject.SetActive(false);        
        if (isPickupGirl)
        {
            MyGameController.instance.MyCameraFollow.OnCameraFocusChange(false);
            MyGameController.instance.isGamePause = true;
            // character walk in car animation
            GetNearestCarCorner();
        }
        else
        {
            MyGameController.instance.isGamePause = true;
            // character walk out of car animation
            passanger.transform.parent = transform;
            isPickupGirl = true;
            //isWalkingPauseAnim = false;
            passanger.GetComponent<Animator>().SetTrigger("exitcar");       
        }
        //dropParticle.SetActive(false);
        //pickupParticle.SetActive(false);
        //PickupDropPoint.gameObject.SetActive(false);
    }

    void PassangerCharacterEnable()
    {
        Vector3 pos = new Vector3(PickupDropPoint.transform.position.x + (leftSideOfRoad ? -7 : 7), PickupDropPoint.transform.position.y, PickupDropPoint.transform.position.z);
        int randomChar = UnityEngine.Random.Range(0, prefPassangerCharacter.Count);
        passanger = GetPassangerCharacter(randomChar);
        if (passanger == null)
        {            
            
            AsyncOperationHandle<GameObject> handle = prefPassangerCharacter[randomChar].InstantiateAsync(pos, PickupDropPoint.transform.rotation, transform);
            handle.Completed += (obj) =>
            {
                if (obj.Status == AsyncOperationStatus.Succeeded)
                {
                    passanger = obj.Result;
                    passanger.transform.localScale = modelScale;
                    poolPassangerCharacter.Add(passanger);
                    passangerAnimator = passanger.GetComponent<Animator>();
                    passangerNavmeshAgent = passanger.GetComponent<NavMeshAgent>();
                    passangerNavmeshAgent.enabled = true;
                }
                else
                {
                    Debug.Log("error on load addressable Beach Character index = " + randomChar);
                }
            };
        }
        else
        {
            passanger.transform.parent = transform;
            passanger.transform.position = pos;
            passanger.transform.localRotation = PickupDropPoint.transform.rotation;
            passanger.gameObject.SetActive(true);
            passangerAnimator = passanger.GetComponent<Animator>();
            passangerNavmeshAgent = passanger.GetComponent<NavMeshAgent>();
            passangerNavmeshAgent.enabled = true;
        }
    }

    GameObject GetPassangerCharacter(int ind)
    {
        GameObject _passanger = null;
        foreach (GameObject _pas in poolPassangerCharacter)
        {
            CharacterInfo chinfo = _pas.GetComponent<CharacterInfo>();
            if (chinfo.indexPref == ind && !_pas.activeInHierarchy)
            {
                _passanger = _pas.gameObject;
                break;
            }
        }
        return _passanger;
    }

    void PassangerDisable()
    {
        passanger.SetActive(false);
        GetPickDropPointLocation();
    }

    void GetNearestCarCorner()
    {
        /*for (int i = 0; i < carCornersAll.Length; i++)
        {
            carCornerDistance = Vector3.Distance(passanger.transform.position, carCornersAll[i].transform.position);
            if(carCornerDistance < carCornerNearestDistance)
            {
                nearestCarCorner = carCornersAll[i];
                carCornerNearestDistance = carCornerDistance;
            }
        }
        carCornerNearestDistance = 10000;               
        if(nearestCarCorner.name != "11" && nearestCarCorner.name != "22" && nearestCarCorner.name != "0")
        {
            nearestCarCorner = doorPoint;
        }*/
        meshSurface.BuildNavMesh();
        //passanger.transform.DOLookAt(nearestCarCorner.transform.position,0f);
        passangerNavmeshAgent.SetDestination(doorPoint.position);
        passangerAnimator.SetBool("walk", true);
        isActiveNavmesh = true;
        /*if (nearestCarCorner.name == "11")
        {
            DG.Tweening.Sequence mySequence = DOTween.Sequence();
            mySequence.Append(passanger.transform.DOMove(carCorner1[0].position, walkSpeedSquence))
                .Append(passanger.transform.DOLookAt(carCorner1[1].position, 0f))
                .Append(passanger.transform.DOMove(carCorner1[1].position, walkSpeedSquence))
                .Append(passanger.transform.DOLookAt(carCorner1[2].position, 0f))
                .Append(passanger.transform.DOMove(carCorner1[2].position, walkSpeedSquence))
                .PrependInterval(1)
                .SetSpeedBased(true).OnComplete(() =>
                {
                    passanger.transform.LookAt(lookAtDoor.position);
                    OnCompleteAnimationWalkToDoor();
                });
        }
        else if(nearestCarCorner.name == "22")
        {
            DG.Tweening.Sequence mySequence = DOTween.Sequence();
            mySequence.Append(passanger.transform.DOMove(carCorner2[0].position, walkSpeedSquence))
                .Append(passanger.transform.DOLookAt(carCorner2[1].position, 0f))
                .Append(passanger.transform.DOMove(carCorner2[1].position, walkSpeedSquence))
                .Append(passanger.transform.DOLookAt(carCorner2[2].position, 0f))
                .Append(passanger.transform.DOMove(carCorner2[2].position, walkSpeedSquence))
                .PrependInterval(1)
                .SetSpeedBased(true).OnComplete(() =>
                {
                    passanger.transform.LookAt(lookAtDoor.position);
                    OnCompleteAnimationWalkToDoor();
                });
        }
        else
        {
            DG.Tweening.Sequence mySequence = DOTween.Sequence();
            mySequence.Append(passanger.transform.DOMove(carCorner3[0].position, walkSpeedSquence))
                .PrependInterval(1)
                .SetSpeedBased(true).OnComplete(() =>
                {
                    passanger.transform.LookAt(lookAtDoor.position);
                    OnCompleteAnimationWalkToDoor();
                });
        }*/

        /*
        //passanger.transform.parent = nearestCarCorner.transform;
        passangerAnimator.SetBool("walk", true);        
        passanger.transform.DOLocalMove(new Vector3(0, 0, 0), 1).SetDelay(0.5f).SetSpeedBased(true).OnComplete(() =>
        {
            //passanger.transform.localPosition = new Vector3(0, 0, 0);
            passanger.transform.rotation = modelRotation;
            nearestCarCorner.DORestart();
            isWalkingPauseAnim = false;
        });       
        */
    }

    public void OnCompleteAnimationWalkToDoor()
    {
        MyGameController.instance.MyCameraFollow.OnCameraFocusChange(true);
        passangerAnimator.SetBool("walk", false);
        passanger.transform.parent = doorPoint.transform;
        isPickupGirl = false;
        nearestCarCorner = null;
        passangerAnimator.SetTrigger("entercar");
        PickupDropPoint.gameObject.SetActive(false);
        GetPickDropPointLocation();
        MyGameController.instance.MyManager.carLambController.UpdateBlockControl(false);
        MyGameController.instance.isGamePause = false;
    }
    public void HandAnimationInCar(bool _hand)
    {
        passangerAnimator.SetBool("hand", _hand);
    }
    public void OpenCarDoor()
    {
        door1Right.SetBool("open", true);
    }
    public void CloseCarDoor()
    {
        door1Right.SetBool("open", false);
    }
    public void SetPassangerSittingHeight(float pos)
    {        
        if (pos != 0) // enter in car and sit adjust
        {
            passanger.transform.DOLocalMove(new Vector3(0, pos, 0), 1);
            passanger.transform.DOLocalRotate(new Vector3(0, -90, 0), 1);
        }
        else // exit from car
        {
            passanger.transform.DOLocalMoveY(pos, 1);
        }
    }

    public void EnableHeartParticleOfPassanger()
    {
        heartParicleOfPassanger.transform.position = new Vector3(passanger.transform.position.x,
            passanger.transform.position.y + 3,
            passanger.transform.position.z);
        heartParicleOfPassanger.Play();
        MyGameController.instance.IncreaseTimerAsReward(MyGameController.instance.increaseTimeReward);
        MyGameController.instance.isGamePause = false;
        Invoke("PassangerDisable", 4f);
        MyGameController.instance.MyManager.carLambController.ResetBoostAmount();
        MyGameController.instance.MyManager.carLambController.UpdateBlockControl(false);
        PickupDropPoint.gameObject.SetActive(false);
        bestHeart = bestHeart + 1;
        MyGameController.instance.UpdateHeartPointOnPlayfab(bestHeart);
    }

    private void GetCarLocationOnSpline()
    {
        pathZonePoint.localPosition = MyGameController.instance.MyManager.spline.transform.InverseTransformPoint(MyGameController.instance.MyManager.carLamb.position);
        CurveSample pointOnSpline = MyGameController.instance.MyManager.spline.GetProjectionSample(pathZonePoint.localPosition);
        pathZonePointTest.localPosition = pointOnSpline.location;
        CubicBezierCurve carCubicBezier = pointOnSpline.curve;
        currCarAtDistance = 0;
        for (int i = 0; i < extrusionSegment.Count; i++)
        {
            if (extrusionSegment[i].curve.n1 == carCubicBezier.n1 && extrusionSegment[i].curve.n2 == carCubicBezier.n2)
            {
                curCarSegmentIndex = i;
                break;
            }
            currCarAtDistance = currCarAtDistance + extrusionSegment[i].curve.Length;
        }
        currCarAtDistance = currCarAtDistance + pointOnSpline.distanceInCurve;
        Debug.Log("carDistTotal = " + currCarAtDistance);

        if(currCarAtDistance  < prevPointAtDistance) // first car than point
        {
            float forwardDistance = prevPointAtDistance - currCarAtDistance;
            float backwardDistance = (MyGameController.instance.MyManager.spline.Length - prevPointAtDistance) + currCarAtDistance;
            if(forwardDistance < backwardDistance)
            {
                forwardDir = true;
                Debug.Log("direction = Forward");
            }
            else
            {
                forwardDir = false;
                Debug.Log("direction = Back");
            }
        }
        else // first point than car
        {
            float forwardDistance = (MyGameController.instance.MyManager.spline.Length - currCarAtDistance) + prevPointAtDistance;
            float backwardDistance = currCarAtDistance - prevPointAtDistance;
            if (forwardDistance < backwardDistance)
            {
                forwardDir = true;
                Debug.Log("direction = Forward");
            }
            else
            {
                forwardDir = false;
                Debug.Log("direction = Back");
            }
        }

        ArrowTarget();              
    }

    private void LateUpdate()
    {
        if (lookAtPoint && PickupDropPoint != null)
        {
            t_CheckDirectionTime += Time.deltaTime;
            directionalArrow.LookAt(arrowTargetVector,Vector3.up);

            if(Vector3.Distance(directionalArrow.position, arrowTargetVector) < 40)
            {
                t_CheckDirectionTime = 0;
                ArrowTarget();
            }
            if(t_CheckDirectionTime > 5)
            {
                t_CheckDirectionTime = 0;
                GetCarLocationOnSpline();
            }
        }
    }
    float t_CheckDirectionTime;
    Vector3 arrowTargetVector;
    void ArrowTarget()
    {
        lookAtPoint = false;
        if (curCarSegmentIndex == curDropSegmentIndex)
        {
            lookAtPoint = true;
            return;
        }

        if (forwardDir)
        {
            curCarSegmentIndex++;
            if (curCarSegmentIndex >= extrusionSegment.Count)
            {
                curCarSegmentIndex = 0;
            }
            arrowTargetVector = MyGameController.instance.MyManager.spline.transform.TransformPoint(extrusionSegment[curCarSegmentIndex].curve.n2.Position);
        }
        else
        {
            curCarSegmentIndex--;
            if (curCarSegmentIndex < 0)
            {
                curCarSegmentIndex = extrusionSegment.Count - 1;
            }
            arrowTargetVector = MyGameController.instance.MyManager.spline.transform.TransformPoint(extrusionSegment[curCarSegmentIndex].curve.n1.Position);
        }

        if (curCarSegmentIndex == curDropSegmentIndex)
        {
            arrowTargetVector = PickupDropPoint.transform.position;
            directionalArrow.DOScaleZ(1.5f, 0.5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetId("directionalArrow");
        }
        else
        {
            DOTween.Kill("directionalArrow");
            directionalArrow.DOScaleZ(1f, 0.5f);
        }
        arrowTargetVector.y = 0;
        directionalArrow.DOLookAt(arrowTargetVector, 0.5f).OnComplete(() =>
        {
            lookAtPoint = true;
        });
        directionalArrow.gameObject.SetActive(true);

    }
}
