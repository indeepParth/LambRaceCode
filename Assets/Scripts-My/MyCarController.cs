using DG.Tweening;
using PG;
using System;
using System.Data;
using UnityEngine;

public enum PowerUpEnum
{
    TimeBonus,
    Booster
}
public enum CarSpeedEnum
{
    Slow,
    Normal,
    Fast,
    SuperFast
}
[Serializable]
public class CarSpeedStatus
{
    public float acceleration = 30f;
    public float maxSpeed = 60f;
    public float engineSoundPitch = 1;
}

public class MyCarController : MonoBehaviour
{
    [Header("Car Speed Status")]
    public CarSpeedEnum carSpeedEnum = CarSpeedEnum.Slow;
    public CarSpeedStatus carSpeedStatusStandBy;
    public CarSpeedStatus carSpeedStatusNormal;
    public CarSpeedStatus carSpeedStatusFast;
    public CarSpeedStatus carSpeedStatusSuperFast;
    [Header("Other Car Settings")]
    public Transform myChildCar;
    public Rigidbody myRigidbody;
    public BoxCollider boxCollider;
    public Animator driverAnimator;
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
    public float SpeedInHour
    {
        get
        {
            return (Mathf.Abs(currentSpeed) * CarParamsConstants.KPHMult);
        }
    }

    public float penalty_Sidewalk = 15;
    public float penalty_sand = 10;
    public bool isGrounded = false;
    public bool isInAir = false;

    public float turnSpeedReductionFactor = 0.5f;
    public float driftFactor = 0.95f; // How much the car drifts
    public float driftControl = 0.5f; // How much control the player has over drifting
    public float driftControlWhenNoDrift = 1.2f;
    public float driftAngle = 20;
    public bool isDrifting = false;
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

    public float forceCar = 1;

    public bool enableOilSpillEffect = false;
    public float oilSlipFactor = 0;
    public float oilSlipInput = 0.6f;

    public float jumpYFactor = 0.2f;

    private void Awake()
    {
        _maxSpeed = maxSpeed;
        myChildCar = transform.GetChild(0);
        UpdateBlockControl(true);
    }

    private void FixedUpdate()
    {
        if (!MyGameController.instance.isGameStart)
            return;

        acceleration = GetCarSpeedStatus().acceleration;
        maxSpeed = GetCarSpeedStatus().maxSpeed;
        MyGameController.instance.MySoundManager.maxPitch = GetCarSpeedStatus().engineSoundPitch;

        if (jumpEffect) { JumpBoostEffectEnd(); }
        HandleMovement();
        HandleBoost();
        UpdateWheelPoses();
        DetectSurface();
    }

    void Update()
    {
        if (!MyGameController.instance.isGameStart)
            return;
        //HandleMovement();        
        //HandleBoost();
        //UpdateWheelPoses();
        //DetectSurface();
        OilSpillEffect();
        float turnInput = Input.GetAxis("Horizontal");
        foreach (var wheel in frontWheelTransforms)
        {
            //Quaternion deltaRotation = Quaternion.Euler(0, turnInput * maxTurnAngle * turnSpeed, 0);
            Vector3 toV3 = new Vector3(0, turnInput * maxTurnAngle, 0);
            wheel.localEulerAngles = Vector3.Lerp(wheel.localEulerAngles, toV3, turnSpeed * Time.deltaTime);
        }
        float moveInput = Input.GetAxis("Vertical");
        if (currentSpeed > 2.01f || currentSpeed < -2.01f)
        {
            HandleSteering();
            // MyGameController.instance.MySoundManager.EngineSound(true);
            UpdateDriverAnimator(true);
        }
        else
        {
            // MyGameController.instance.MySoundManager.EngineSound(false);
            UpdateDriverAnimator(false);
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
        if (blockControl)
        {
            moveInput = 0;
        }

        if (moveInput != 0 && !isBoosting && isGrounded)
        {
            if (moveInput > 0)
            {
                currentSpeed += moveInput * acceleration * Time.deltaTime;
                isBrakeing = false;
                isReverse = false;
                //if (currentSpeed >= -1)
                //OnReverseUpdateCarChildPosition();
                MyGameController.instance.MySoundManager.EngineSound(true);
                MyGameController.instance.MySoundManager.BreakSound(false);
            }
            else
            {
                if (currentSpeed >= 1)
                {
                    currentSpeed += moveInput * 2 * acceleration * Time.deltaTime;
                    isBrakeing = true;
                    isReverse = false;
                    //OnReverseUpdateCarChildPosition();
                    // MyGameController.instance.MySoundManager.EngineSound(false);
                    MyGameController.instance.MySoundManager.BreakSound(true);
                }
                else
                {
                    currentSpeed += moveInput * 0.5f * acceleration * Time.deltaTime;
                    isBrakeing = false;
                    isReverse = true;
                    //if (!blockControl && currentSpeed <= -1)
                    //OnReverseUpdateCarChildPosition();
                    MyGameController.instance.MySoundManager.EngineSound(true);
                    MyGameController.instance.MySoundManager.BreakSound(false);
                }

            }
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed * 0.5f, maxSpeed);
        }
        else if (!isBoosting)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, deceleration * Time.deltaTime);
            isBrakeing = false;
            // MyGameController.instance.MySoundManager.EngineSound(false);
            MyGameController.instance.MySoundManager.BreakSound(false);
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
        if (jumpEffect)
        {
            forward = (transform.forward + (transform.up * jumpYFactor)) * currentSpeed * Time.deltaTime;
            // forward = new Vector3( 0, jumpYFactor * currentSpeed * Time.deltaTime, currentSpeed * Time.deltaTime);
            Debug.Log("jump UP = " + isGrounded);
        }
        else
        {
            if (isInAir)
            {
                forward = (transform.forward + (-transform.up * jumpYFactor * 2)) * currentSpeed * Time.deltaTime;
                Debug.Log("jump DOWN = " + isGrounded);
            }
            else
            {
                forward = transform.forward * currentSpeed * Time.deltaTime;
                // Debug.Log("Forward");
            }
        }
        SetCarSpeedEnum(currentSpeed);
        //transform.position += forward;
        // myRigidbody.MovePosition(transform.position + forward);
        myRigidbody.AddForce(forward * forceCar, ForceMode.Impulse);
    }

    private void HandleSteering()
    {
        float turnInput = Input.GetAxis("Horizontal");
        if (blockControl)
        {
            turnInput = 0;
        }
        else if (enableOilSpillEffect)
        {
            turnInput = oilSlipFactor;
            // turnInput = Mathf.Clamp(turnInput, -1, 1);
        }

        // Handle drifting
        if (isGrounded && !isBoosting && (Input.GetKey(KeyCode.Space) || enableOilSpillEffect) && (isDrifting || Mathf.Abs(turnInput) > 0.5f) && (isDrifting || currentSpeed > maxSpeed * 0.5f)) // Drift when turning sharply and at higher speeds
        {
            isDrifting = true;
            rotationToDrift = Quaternion.Euler(0, turnInput * driftAngle, 0);
            myChildCar.localRotation = Quaternion.Lerp(myChildCar.localRotation, rotationToDrift, driftControl * Time.deltaTime);
            if (currentSpeed > maxSpeed * 0.5)
            {
                currentSpeed *= 1 - (Mathf.Abs(turnInput) * driftFactor * Time.deltaTime);
            }
            if (!enableOilSpillEffect && (currentSpeed > 10f || currentSpeed < -10f))
            {
                MyGameController.instance.MySoundManager.DriftSound(true);
            }
            else
            {
                MyGameController.instance.MySoundManager.DriftSound(false);
            }
        }
        else
        {
            currentSpeed *= 1 - (Mathf.Abs(turnInput) * turnSpeedReductionFactor * Time.deltaTime);
            rotationToDrift = Quaternion.Euler(0, turnInput * turningAngleOnChild, 0);
            myChildCar.localRotation = Quaternion.Lerp(myChildCar.localRotation, rotationToDrift, driftControlWhenNoDrift * Time.deltaTime);
            isDrifting = false;
            MyGameController.instance.MySoundManager.DriftSound(false);
        }

        carVFX.UpdateTrail(isDrifting);

        float turning_speed = turnSpeed * (1 - currentSpeed / maxSpeed);
        turning_speed = Mathf.Clamp(turning_speed, minTurnSpeed, turnSpeed);
        currentTurnAngle = (isDrifting ? driftAngleOnTurning : 1) * turnInput * (isReverse ? maxReverseTurnAngle : maxTurnAngle) * turning_speed * Time.deltaTime * Mathf.Sign(currentSpeed);

        if (turnInput != 0)
        {
            //transform.Rotate(0, currentTurnAngle, 0);
            Quaternion deltaRotation = Quaternion.Euler(0, currentTurnAngle * Time.fixedDeltaTime, 0);
            // myRigidbody.MoveRotation(myRigidbody.rotation * deltaRotation);
            myRigidbody.rotation = myRigidbody.rotation * deltaRotation;
        }
    }

    private void HandleBoost()
    {
        // Check if the boost key is pressed and boost is available
        if (!blockControl && !isBoosting && currentSpeed > 0 && Input.GetKeyDown(KeyCode.N) && Time.time > nextBoostTime)
        {
            isBoosting = true;
            maxSpeed *= boostMultiplier;
            currentSpeed = maxSpeed; // Apply boost
            boostEndTime = Time.time + boostDuration; // Set the end time for the boost
            if (MyGameController.instance.gameMode != GameMode.FreeRide)
            {
                nextBoostTime = Time.time + boostCooldown + boostDuration + 1000000;
            }
            else
            {
                nextBoostTime = Time.time + boostCooldown + boostDuration; // Set the next available boost time
            }
            MyGameController.instance.MyManager.OnBoostNitroEnableSpeedEffect();
            carVFX.UpdateBoost(isBoosting);
            // MyGameController.instance.MySoundManager.EngineSound(false);
            MyGameController.instance.MySoundManager.BreakSound(false);
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

        if (Time.time > nextBoostTime && !isBoosting)
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
        int layerMaskSand = 1 << 12; // sand
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10, layerMaskRoad))
        {
            maxSpeed = _maxSpeed;
            isGrounded = true;
            isInAir = false;
        }
        else if (Physics.Raycast(transform.position, Vector3.down, out hit, 10, layerMaskSideWalk))
        {
            maxSpeed = penalty_Sidewalk;
            isGrounded = true;
            isInAir = false;
        }
        else if (Physics.Raycast(transform.position, Vector3.down, out hit, 10, layerMaskSand))
        {
            maxSpeed = penalty_sand;
            isGrounded = true;
            isInAir = false;
        }
        else
        {
            maxSpeed = _maxSpeed;
            isGrounded = false;
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

    // private void OnTriggerEnter(Collider other)
    // {
    //     // return;
    //     // if (other.gameObject.CompareTag("carAI"))
    //     // {
    //     //     if (!isBoosting)
    //     //     {
    //     //         currentSpeed *= brakeFactor;
    //     //     }
    //     //     Vector3 dir = other.transform.position - transform.position;
    //     //     Vector3 hitPoint = other.ClosestPoint(other.transform.position);
    //     //     hitPoint.y += 1;
    //     //     other.gameObject.GetComponent<AITrafficCar>().HideAfterAccident(dir, hitPoint);
    //     // }
    // }

    private Collision lastColisionCar;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("carAI"))
        {
            if (!isBoosting)
            {
                currentSpeed *= brakeFactor;
                if (!ReferenceEquals(lastColisionCar, collision))
                {
                    MyGameController.instance.MySoundManager.PlayCrashCar();
                }
            }

            //Vector3 hitPoint = collision.GetContact(0).point;
            //Vector3 dir = hitPoint - transform.position;
            //collision.gameObject.GetComponent<AITrafficCar>().HideAfterAccident(dir, hitPoint);
            lastColisionCar = collision;
            CancelInvoke("CarCrashSoundAfterHitSameCar");
            Invoke("CarCrashSoundAfterHitSameCar", 3f);
        }
        else if (collision.gameObject.CompareTag("prop"))
        {
            if (!ReferenceEquals(lastColisionCar, collision))
            {
                MyGameController.instance.MySoundManager.PlayCrashCar();
            }
            lastColisionCar = collision;
            CancelInvoke("CarCrashSoundAfterHitSameCar");
            Invoke("CarCrashSoundAfterHitSameCar", 3f);
        }
        else if (collision.gameObject.CompareTag("jump_Platform"))
        {
            JumpEffectStart();
            // Quaternion deltaRotation = Quaternion.Euler(60, 0, 0); propOnRoad
            // myRigidbody.rotation = myRigidbody.rotation * deltaRotation;
        }
    }
    void CarCrashSoundAfterHitSameCar()
    {
        lastColisionCar = null;
    }

    // void OnCollisionExit(Collision collision)
    // {
    //     if (collision.gameObject.CompareTag("jump_Platform"))
    //     {

    //         // Quaternion deltaRotation = Quaternion.Euler(0, 0, 0);
    //         // myRigidbody.rotation = myRigidbody.rotation * deltaRotation;
    //     }
    // }

    bool carChildPivotBack = false;
    public void OnReverseUpdateCarChildPosition(bool _isReverse)
    {
        if (_isReverse && !carChildPivotBack)
        {
            carChildPivotBack = true;
            Vector3 pos = myChildCar.localPosition;
            pos.z = 3;
            myChildCar.localPosition = pos;

            Vector3 box = boxCollider.center;
            box.z = 2.5f;
            boxCollider.center = box;
        }
        else if (!_isReverse && carChildPivotBack)
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

    bool oilTweeenStarts = false;
    void OilSpillEffect()
    {
        if (!oilTweeenStarts && enableOilSpillEffect)
        {
            if (currentSpeed > maxSpeed * 0.5f)
            {
                oilTweeenStarts = true;
                float valFloat = 0f;
                float timeRandom = UnityEngine.Random.Range(0.4f, 0.6f);
                bool isright = timeRandom > 0.5f ? true : false;
                Timer.Schedule(this, isright ? 0 : timeRandom, () =>
                {
                    MyGameController.instance.MySoundManager.PlayOilSlipCar();
                });
                DOTween.To(() => valFloat, x => valFloat = x, oilSlipInput, timeRandom).SetDelay(isright ? 0 : timeRandom).OnUpdate(() => { oilSlipFactor = valFloat; })
                .OnComplete(() =>
                {
                    timeRandom = UnityEngine.Random.Range(0.3f, 0.6f);
                    Timer.Schedule(this, isright ? 0 : timeRandom, () =>
                    {
                        MyGameController.instance.MySoundManager.PlayOilSlipCar();
                    });
                    DOTween.To(() => valFloat, x => valFloat = x, -oilSlipInput, timeRandom).SetDelay(!isright ? 0 : timeRandom).OnUpdate(() => { oilSlipFactor = valFloat; })
                    .OnComplete(() =>
                    {
                        timeRandom = UnityEngine.Random.Range(0.2f, 0.6f);
                        Timer.Schedule(this, timeRandom, () =>
                        {
                            MyGameController.instance.MySoundManager.PlayOilSlipCar();
                        });
                        DOTween.To(() => valFloat, x => valFloat = x, oilSlipInput, timeRandom).SetDelay(timeRandom).OnUpdate(() => { oilSlipFactor = valFloat; })
                        .OnComplete(() =>
                        {
                            timeRandom = UnityEngine.Random.Range(0.2f, 0.6f);
                            DOTween.To(() => valFloat, x => valFloat = x, -oilSlipInput, timeRandom).OnUpdate(() => { oilSlipFactor = valFloat; })
                            .OnComplete(() =>
                            {
                                timeRandom = UnityEngine.Random.Range(0.1f, 0.4f);
                                DOTween.To(() => valFloat, x => valFloat = x, oilSlipInput, timeRandom).OnUpdate(() => { oilSlipFactor = valFloat; })
                                .OnComplete(() =>
                                {
                                    timeRandom = UnityEngine.Random.Range(0.1f, 0.3f);
                                    DOTween.To(() => valFloat, x => valFloat = x, -oilSlipInput, timeRandom).OnUpdate(() => { oilSlipFactor = valFloat; })
                                    .OnComplete(() =>
                                    {
                                        oilSlipFactor = 0;
                                        enableOilSpillEffect = false;
                                        oilTweeenStarts = false;
                                    });
                                });
                            });
                        });
                    });
                });
            }
            else
            {
                enableOilSpillEffect = false;
            }
        }
    }

    private bool jumpEffect = false;
    private void JumpEffectStart()
    {
        if (!blockControl && !isBoosting && currentSpeed > 0)
        {
            jumpEffect = true;
            isBoosting = true;
            maxSpeed *= boostMultiplier;
            currentSpeed = maxSpeed; // Apply boost
            boostEndTime = Time.time + boostDuration; // Set the end time for the boost            
            MyGameController.instance.MyManager.OnBoostNitroEnableSpeedEffect();
            carVFX.UpdateBoost(isBoosting);
            myRigidbody.useGravity = false;
            // myRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            isInAir = true;
        }
    }
    private void JumpBoostEffectEnd()
    {
        // Reset speed after boost duration
        if (isBoosting && Time.time > boostEndTime && boostEndTime != 0)
        {
            jumpEffect = false;
            maxSpeed /= boostMultiplier;
            currentSpeed = maxSpeed;
            boostEndTime = 0;
            isBoosting = false;
            MyGameController.instance.MyManager.DisableSpeedEffect();
            carVFX.UpdateBoost(isBoosting);
            myRigidbody.useGravity = true;
            // myRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
        else if (isBoosting && Time.time > boostEndTime - 1 && boostEndTime != 0)
        {
            myRigidbody.useGravity = true;
        }
    }

    private bool cp0, cp1, cp2, cp3, cp4;
    public void OnCheckPointHit(string cp) // only for grand prix mode
    {
        switch (cp)
        {
            case "0":
                cp0 = true;
                // MyGameController.instance.MyManager.textStartFinish.gameObject.SetActive(false);
                break;
            case "1":
                if (cp0)
                {
                    cp1 = true;
                }
                break;
            case "2":
                if (cp1)
                {
                    cp2 = true;
                }
                break;
            case "3":
                if (cp2)
                {
                    cp3 = true;
                    MyGameController.instance.MyManager.textStartFinish.text = "Finish";
                    // MyGameController.instance.MyManager.textStartFinish.gameObject.SetActive(true);
                }
                break;
            // case "4":
            //     if (cp3)
            //     {
            //         cp4 = true;
            //         MyGameController.instance.MyManager.textStartFinish.text = "Finish";
            //         MyGameController.instance.MyManager.textStartFinish.gameObject.SetActive(true);
            //     }
            //     break;
            default: // finish line
                if (cp0 && cp1 && cp2 && cp3)
                {
                    MyGameController.instance.isGameOver = true;
                    MyGameController.instance.MySoundManager.RaceTrackSound(false);
                    MyGameController.instance.MyManager.carLambController.UpdateBlockControl(true);
                    MyGameController.instance.UIManager.ShowGameOver();
                    MyGameController.instance.PlayFabLogin.SubmitLapsTimeOnGameOver(MyGameController.instance.counterUpTime);
                }
                break;
        }
    }

    private void UpdateDriverAnimator(bool isDriving)
    {
        driverAnimator.SetBool("driving", isDriving);
    }

    public void SetCarSpeedEnum(float carSpeed)
    {
        int speed = SpeedInHour.ToInt();

        if (speed <= 40)
        {
            MyGameController.instance.MyManager.HighSpeedEffect(false);
            if (carSpeedEnum == CarSpeedEnum.Slow)
                return; // No change needed
            Debug.Log("Car Speed Enum: StandBy = " + speed);
            carSpeedEnum = CarSpeedEnum.Slow;
        }
        else if (speed > 40 && speed <= 100)
        {
            MyGameController.instance.MyManager.HighSpeedEffect(false);
            if (carSpeedEnum == CarSpeedEnum.Normal)
                return; // No change needed
            Debug.Log("Car Speed Enum: Normal = " + speed);
            carSpeedEnum = CarSpeedEnum.Normal;
        }
        else if (speed > 100 && speed <= 272)
        {
            MyGameController.instance.MyManager.HighSpeedEffect(false);
            if (carSpeedEnum == CarSpeedEnum.Fast)
                return; // No change needed
            Debug.Log("Car Speed Enum: Fast = " + speed);
            carSpeedEnum = CarSpeedEnum.Fast;
            
        }
        else if (speed > 272)
        {
            MyGameController.instance.MyManager.HighSpeedEffect(true);
            if (carSpeedEnum == CarSpeedEnum.SuperFast)
                return; // No change needed
            Debug.Log("Car Speed Enum: SuperFast = " + speed);
            carSpeedEnum = CarSpeedEnum.SuperFast;
            
        }
        
    }

    public CarSpeedStatus GetCarSpeedStatus()
    {
        CarSpeedStatus carSpeedStatus = null;
        switch (carSpeedEnum)
        {
            case CarSpeedEnum.Normal:
                carSpeedStatus = carSpeedStatusNormal;
                break;
            case CarSpeedEnum.Fast:
                carSpeedStatus = carSpeedStatusFast;
                break;
            case CarSpeedEnum.SuperFast:
                carSpeedStatus = carSpeedStatusSuperFast;
                break;
            default:
                carSpeedStatus = carSpeedStatusStandBy;
                break;
        }

        return carSpeedStatus;
    }
}
