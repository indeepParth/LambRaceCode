using DG.Tweening;
using PG;
using System;
using TurnTheGameOn.SimpleTrafficSystem;
using Unity.Mathematics;
using UnityEngine;

public enum PowerUpEnum
{
    TimeBonus,
    Booster
}

public class MyCarController : MonoBehaviour
{
    public Transform myChildCar;
    public Rigidbody myRigidbody;
    public BoxCollider boxCollider;
    public Transform[] wheelTransforms; // Assign in the inspector
    public Transform[] frontWheelTransforms;
    public float maxSpeed = 20f;
    [NonSerialized]
    public float _maxSpeed = 20f;
    public float acceleration = 30f;
    public float deceleration = 20f;
    public float maxTurnAngle = 30f;
    public float maxReverseTurnAngle = 60f;
    public float turnSpeed = 5f;
    public float minTurnSpeed = 150f;
    public float brakeFactor = 0.5f;
    public float currentSpeed = 0f;
    public bool isBrakeing = false;
    public bool isReverse = false;
    public float SpeedInHour { 
        get
        { 
            return (Mathf.Abs(currentSpeed) * CarParamsConstants.KPHMult);
        } 
    }

    public float penalty_Sidewalk = 10;
    public float penalty_sand = 5;

    public float turnSpeedReductionFactor = 0.5f;
    public float driftFactor = 0.95f; // How much the car drifts
    public float driftControl = 0.5f; // How much control the player has over drifting
    public float driftControlWhenNoDrift = 1.2f;
    public float driftAngle = 20;
    private bool isDrifting = false;
    public float driftAngleOnTurning = 1.3f;
    public float turningAngleOnChild = 10;
    private Vector3 driftVelocity;

    public bool blockControl;

    private float currentTurnAngle = 0f;

    public bool isBoosting = false;
    public float boostMultiplier = 2f; // Multiplier for boost speed
    public float boostDuration = 3f; // Duration of the boost in seconds
    public float boostCooldown = 5f; // Cooldown time between boosts in seconds
    
    private float boostEndTime = 0f;
    private float nextBoostTime = 0f;

    public float hitForceToOther = 100;
    private Quaternion rotationToDrift;

    public CarVFX carVFX;
    public PowerUps powerUps;

    private void Awake()
    {
        _maxSpeed = maxSpeed;
        myChildCar = transform.GetChild(0);
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleBoost();
        UpdateWheelPoses();
        DetectSurface();
    }

    void Update()
    {
        //HandleMovement();        
        //HandleBoost();
        //UpdateWheelPoses();
        //DetectSurface();
        float turnInput = Input.GetAxis("Horizontal");
        foreach (var wheel in frontWheelTransforms)
        {
            //Quaternion deltaRotation = Quaternion.Euler(0, turnInput * maxTurnAngle * turnSpeed, 0);
            Vector3 toV3 = new Vector3(0, turnInput * maxTurnAngle, 0);
            wheel.localEulerAngles = Vector3.Lerp(wheel.localEulerAngles,toV3, turnSpeed * Time.deltaTime);
        }
        float moveInput = Input.GetAxis("Vertical");
        if (currentSpeed > 2.01f || currentSpeed < -2.01f)
        {
            HandleSteering();
            MyGameController.instance.MySoundManager.EngineSound(true);
        }
        else
        {
            MyGameController.instance.MySoundManager.EngineSound(false);
        }
        //MyGameController.instance.MySoundManager.EngineSound(moveInput != 0 ? true : false);

        //if (Input.GetKey(KeyCode.Space) && moveInput == 0 && !isDrifting && !isBoosting)
        //{
        //    currentSpeed *= brakeFactor;
        //}
    }

    public void StopCarInPickupDropArea()
    {
        currentSpeed *= brakeFactor;
        isDrifting = false;
        boostEndTime = -1;
    }

    private void HandleMovement()
    {
        float moveInput = Input.GetAxis("Vertical");
        if(blockControl)
        {
            moveInput = 0;
        }

        if (moveInput != 0)
        {
            if (moveInput > 0)
            {
                currentSpeed += moveInput * acceleration * Time.deltaTime;
                isBrakeing = false;
                isReverse = false;
                //if (currentSpeed >= -1)
                    //OnReverseUpdateCarChildPosition();
            }
            else
            {
                if(currentSpeed >= 1)
                {
                    currentSpeed += moveInput * 2 * acceleration * Time.deltaTime;
                    isBrakeing = true;
                    isReverse = false;
                    //OnReverseUpdateCarChildPosition();
                }
                else
                {
                    currentSpeed += moveInput * acceleration * Time.deltaTime;
                    isBrakeing = false;
                    isReverse = true;
                    //if (!blockControl && currentSpeed <= -1)
                        //OnReverseUpdateCarChildPosition();
                }
                
            }
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
        }
        else if(!isBoosting)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, deceleration * Time.deltaTime);
            isBrakeing = false;
        }

        /*if (currentSpeed > 7.01f || currentSpeed < -7.01f)
        {
            HandleSteering();
            MyGameController.instance.MySoundManager.EngineSound(true);
        }
        else
        {
            MyGameController.instance.MySoundManager.EngineSound(false);
        }
        //MyGameController.instance.MySoundManager.EngineSound(moveInput != 0 ? true : false);

        if (Input.GetKey(KeyCode.Space) && moveInput == 0 && !isDrifting && !isBoosting)
        {
            currentSpeed *= brakeFactor;
        }*/

        Vector3 forward = transform.forward * currentSpeed * Time.deltaTime;        
        //transform.position += forward;
        myRigidbody.MovePosition(transform.position + forward);
    }
    
    private void HandleSteering()
    {
        float turnInput = Input.GetAxis("Horizontal");
        if (blockControl)
        {
            turnInput = 0;
        }
        // Handle drifting
        if (Input.GetKey(KeyCode.Space) && (isDrifting || Mathf.Abs(turnInput) > 0.5f) && (isDrifting || currentSpeed > maxSpeed * 0.5f)) // Drift when turning sharply and at higher speeds
        {
            isDrifting = true;
            rotationToDrift = Quaternion.Euler(0, turnInput * driftAngle, 0);
            myChildCar.localRotation = Quaternion.Lerp(myChildCar.localRotation, rotationToDrift, driftControl * Time.deltaTime);
            if (currentSpeed > maxSpeed * 0.5)
            {
                currentSpeed *= 1 - (Mathf.Abs(turnInput) * driftFactor * Time.deltaTime);
            }            
        }
        else
        {
            currentSpeed *= 1 - (Mathf.Abs(turnInput) * turnSpeedReductionFactor * Time.deltaTime);
            rotationToDrift = Quaternion.Euler(0, turnInput * turningAngleOnChild, 0);
            myChildCar.localRotation = Quaternion.Lerp(myChildCar.localRotation, rotationToDrift, driftControlWhenNoDrift * Time.deltaTime);
            isDrifting = false;            
        }

        carVFX.UpdateTrail(isDrifting);

        float turning_speed = turnSpeed * (1 - currentSpeed / maxSpeed);
        turning_speed = Mathf.Clamp(turning_speed, minTurnSpeed, turnSpeed);
        currentTurnAngle = (isDrifting ? driftAngleOnTurning : 1) * turnInput * (isReverse ? maxReverseTurnAngle : maxTurnAngle) * turning_speed * Time.deltaTime * Mathf.Sign(currentSpeed);

        if (turnInput != 0)
        {
            //transform.Rotate(0, currentTurnAngle, 0);
            Quaternion deltaRotation = Quaternion.Euler(0, currentTurnAngle * Time.fixedDeltaTime, 0);
            myRigidbody.MoveRotation(myRigidbody.rotation * deltaRotation);
        }        
    }

    private void HandleBoost()
    {
        // Check if the boost key is pressed and boost is available
        if (!blockControl && !isBoosting && currentSpeed > 0 && Input.GetKeyDown(KeyCode.LeftAlt) && Time.time > nextBoostTime)
        {
            isBoosting = true;
            maxSpeed *= boostMultiplier;
            currentSpeed = maxSpeed; // Apply boost
            boostEndTime = Time.time + boostDuration; // Set the end time for the boost
            if (!MyGameController.instance.freeMode)
            {
                nextBoostTime = Time.time + boostCooldown + boostDuration + 1000000;
            }
            else
            {
                nextBoostTime = Time.time + boostCooldown + boostDuration; // Set the next available boost time
            }
            MyGameController.instance.MyManager.OnBoostNitroEnableSpeedEffect();
            carVFX.UpdateBoost(isBoosting);
        }

        // Reset speed after boost duration
        if (isBoosting && Time.time > boostEndTime && boostEndTime != 0)
        {            
            maxSpeed /= boostMultiplier;
            currentSpeed = maxSpeed;
            boostEndTime = 0;
            isBoosting = false;
            MyGameController.instance.MyManager.DisableSpeedEffect();
            carVFX.UpdateBoost(isBoosting);
            powerUps.AddBoosterPowerUp();
        }

        if(Time.time > nextBoostTime && !isBoosting)
        {
            MyGameController.instance.UIManager.IsBoostAvailable(true);
        }
    }

    private void UpdateWheelPoses()
    {        
        foreach (Transform wheel in wheelTransforms)
        {            
            // Simple rotation of wheels based on current speed
            wheel.Rotate(Vector3.right, currentSpeed * (360f / (2f * Mathf.PI * 0.3f)) * Time.deltaTime); // Assuming 0.3 is the radius of the wheel
        }
    }

    private void DetectSurface()
    {
        if (isBoosting)
            return;

        int layerMaskRoad = 1 << 6; // road
        int layerMaskSideWalk = 1 << 11; // side walk
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10, layerMaskRoad))
        {
            maxSpeed = _maxSpeed;
        }
        else if (Physics.Raycast(transform.position, Vector3.down, out hit, 10, layerMaskSideWalk))
        {
            maxSpeed = penalty_Sidewalk;
        }
        else
        {
            maxSpeed = penalty_sand;
        }
    }

    public void UpdateBlockControl(bool control)
    {
        blockControl = control;        
    }

    public void ResetBoostAmount()
    {
        nextBoostTime = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        return;
        if (other.gameObject.CompareTag("carAI"))
        {
            if (!isBoosting)
            {
                currentSpeed *= brakeFactor;
            }
            Vector3 dir = other.transform.position - transform.position;
            Vector3 hitPoint = other.ClosestPoint(other.transform.position);
            hitPoint.y += 1;
            other.gameObject.GetComponent<AITrafficCar>().HideAfterAccident(dir, hitPoint);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("carAI"))
        {
            if (!isBoosting)
            {
                currentSpeed *= brakeFactor;
            }
                        
            //Vector3 hitPoint = collision.GetContact(0).point;
            //Vector3 dir = hitPoint - transform.position;
            //collision.gameObject.GetComponent<AITrafficCar>().HideAfterAccident(dir, hitPoint);
        }
    }

    bool carChildPivotBack = false;
    public void OnReverseUpdateCarChildPosition(bool _isReverse)
    {
        if(_isReverse && !carChildPivotBack)
        {
            carChildPivotBack = true;
            Vector3 pos = myChildCar.localPosition;
            pos.z = 3;
            myChildCar.localPosition = pos;

            Vector3 box = boxCollider.center;
            box.z = 2.5f;
            boxCollider.center = box;
        }
        else if(!_isReverse && carChildPivotBack)
        {
            carChildPivotBack = false;
            Vector3 pos = myChildCar.localPosition;
            pos.z = -1.5f;
            myChildCar.localPosition = pos;

            Vector3 box = boxCollider.center;
            box.z = -2f;
            boxCollider.center = box;
        }
    }

    public void SetCarChildPosition()
    {
        carChildPivotBack = false;
        Vector3 pos = myChildCar.localPosition;
        pos.z = -1.5f;
        myChildCar.localPosition = pos;

        Vector3 box = boxCollider.center;
        box.z = -2f;
        boxCollider.center = box;
    }
}
