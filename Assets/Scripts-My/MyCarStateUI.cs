using PG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MyCarStateUI : MonoBehaviour
{
    public MyCarController carLambController;
    public bool IsInitialized { get; private set; }

    [Header("Main info settings")]
    [SerializeField] float UpdateSpeedTime;
    [SerializeField] TextMeshProUGUI CurrentSpeedText;
    [SerializeField] TextMeshProUGUI CurrentGearText;
    [SerializeField] TextMeshProUGUI MeasurementUnits;
    [SerializeField] Image GearCircleImage;
    [SerializeField] Image ArrowImage;
    [SerializeField] Transform ArrowRotateTransform;
    [SerializeField] float ZeroRpmArrowAngle;
    [SerializeField] float MaxRpmArrowAnle;

    [SerializeField] Image FillTachometrImage;
    [SerializeField] float ZeroRpmFill;
    [SerializeField] float MaxRpmFill;
    [SerializeField] float speedometerArrowFactor = 0.9f;

    [Header("DashboardIcons")]
    [SerializeField] Image HandbrakeIcon;

    [Header("Boost")]
    [SerializeField] GameObject BoostStateGO;
    [SerializeField] Image BoostFillImage;
    [SerializeField] float ZeroBoostFillAmount = 0;
    [SerializeField] float MaxBoostFillAmount = 1;

    [SerializeField] Color LowRpmColor;
    [SerializeField] Color MediumRpmColor;
    [SerializeField] Color HighRpmColor;
    [SerializeField] float ChangeColorSpeed = 10f;

    //
    int CurrentGear;
    float UpdateSpeedTimer;
    float RPMPercent;
    RpmState CurrentRpmState;
    Coroutine SetColorCoroutine;

    private void OnDisable()
    {
        IsInitialized = false;
    }

    bool IsCarInitialize()
    {
        return carLambController != null && IsInitialized;
    }

    public void InitializeCar()
    {
        MeasurementUnits.text = "kmh";

        BoostFillImage.fillAmount = 1;
        boostAmount = carLambController.boostDuration;

        Color color;
        if (HandbrakeIcon)
        {
            color = HandbrakeIcon.color;
            color.a = 0;
            HandbrakeIcon.color = color;
        }

        IsInitialized = true;
    }

    private void Update()
    {
        if (!IsCarInitialize())
        {
            return;
        }

        UpdateSpeed();

        UpdateGear();
        UpdateTachometr();
        UpdateColors();
        //UpdateTurbo();
        UpdateBoost();
        UpdateDashboard();
    }

    /// <summary>
    /// Update speed text, with a small interval between updates (To prevent flickering).
    /// </summary>
    void UpdateSpeed()
    {
        if (UpdateSpeedTimer <= 0)
        {
            CurrentSpeedText.text = carLambController.SpeedInHour.ToInt().ToString();
            UpdateSpeedTimer = UpdateSpeedTime;
        }
        else
        {
            UpdateSpeedTimer -= Time.deltaTime;
        }
    }

    void UpdateGear()
    {
        var currentGear = carLambController.currentSpeed < 0 ?
            -1 :
            carLambController.currentSpeed > -0.05f && carLambController.currentSpeed < 0.05f ?
            0 :
            carLambController.currentSpeed < CarMaxSpeedForGearCalculation() * 0.15f ?
            1 :
            carLambController.currentSpeed < CarMaxSpeedForGearCalculation() * 0.3f ?
            2 :
            carLambController.currentSpeed < CarMaxSpeedForGearCalculation() * 0.45f ?
            3 :
            carLambController.currentSpeed < CarMaxSpeedForGearCalculation() * 0.6f ?
            4 :
            carLambController.currentSpeed < CarMaxSpeedForGearCalculation() * 0.75f ?
            5 :
            carLambController.currentSpeed < CarMaxSpeedForGearCalculation() * 0.9f ?
            6 :
            7;

        if (CurrentGear != currentGear)
        {
            CurrentGear = currentGear;
            if (CurrentGear < 0)
            {
                CurrentGearText.text = "R";
            }
            else if (CurrentGear == 0)
            {
                CurrentGearText.text = "N";
            }
            else
            {
                CurrentGearText.text = CurrentGear.ToString();
            }
        }
    }

    /// <summary>
    /// Update tachometer arrow position.
    /// </summary>
    void UpdateTachometr()
    {
        RPMPercent = Mathf.Lerp(RPMPercent, Mathf.Abs(carLambController.currentSpeed) / CarMaxSpeedForSpeedometer(), Time.deltaTime * 15);

        var arrowAngle = Mathf.Lerp(ZeroRpmArrowAngle, MaxRpmArrowAnle, RPMPercent);
        ArrowRotateTransform.localRotation = Quaternion.AngleAxis(arrowAngle, Vector3.forward);

        var fill = Mathf.Lerp(ZeroRpmFill, MaxRpmFill, RPMPercent);
        FillTachometrImage.fillAmount = fill;
    }

    /// <summary>
    /// Coloring tachometer arrow and ring around gear in RPM-dependent colors.
    /// </summary>
    void UpdateColors()
    {
        RpmState newState =
            carLambController.currentSpeed > CarMaxSpeedForSpeedometer() * 0.8f ?
                RpmState.High :
                carLambController.currentSpeed > CarMaxSpeedForSpeedometer() * 0.3f ?
                    RpmState.Medium :
                RpmState.Low;

        if (newState != CurrentRpmState)
        {
            CurrentRpmState = newState;

            Color targetColor;
            switch (CurrentRpmState)
            {
                case RpmState.High:
                    targetColor = HighRpmColor;
                    break;
                case RpmState.Medium:
                    targetColor = MediumRpmColor;
                    break;
                default:
                    targetColor = LowRpmColor;
                    break;
            }

            if (SetColorCoroutine != null)
            {
                StopCoroutine(SetColorCoroutine);
            }

            SetColorCoroutine = StartCoroutine(DoSetColor(targetColor));
        }
    }

    public void ResetBoostWhenAvailable()
    {
        boostAmount = carLambController.boostDuration;
        BoostFillImage.DOFillAmount(1, 0.5f);
    }
    float boostAmount;
    void UpdateBoost()
    {
        if (!carLambController.isBoosting)
        {
            return;
        }

        var fill = boostAmount / carLambController.boostDuration;
        BoostFillImage.fillAmount = Mathf.Lerp(ZeroBoostFillAmount, MaxBoostFillAmount, fill);
        boostAmount -= Time.deltaTime;
        boostAmount = Mathf.Clamp(boostAmount, 0, carLambController.boostDuration);
    }

    void UpdateDashboard()
    {
        Color color;
        float offSpeed = 10 * Time.deltaTime;
        if (HandbrakeIcon)
        {
            color = HandbrakeIcon.color;
            if (carLambController.isDrifting) //isBrakeing
            {
                color.a = 1;
            }
            else
            {
                color.a = Mathf.MoveTowards(color.a, 0, offSpeed);
            }
            HandbrakeIcon.color = color;
        }
    }

    /// <summary>
    /// Smooth color change.
    /// </summary>
    IEnumerator DoSetColor(Color targetColor)
    {
        float t = 0;
        Color startColor = CurrentGearText.color;
        Color currentColor;
        while (t < 1)
        {
            t += Time.deltaTime * ChangeColorSpeed;
            currentColor = Color.Lerp(startColor, targetColor, t);
            CurrentGearText.color = currentColor;
            GearCircleImage.color = currentColor;
            ArrowImage.color = currentColor;

            yield return null;
        }

        CurrentGearText.color = targetColor;
        GearCircleImage.color = targetColor;
        ArrowImage.color = targetColor;

        SetColorCoroutine = null;
    }

    float CarMaxSpeedForGearCalculation()
    {
        return carLambController.isBoosting? carLambController.maxSpeed : carLambController._maxSpeed;
    }
    
    float CarMaxSpeedForSpeedometer()
    {
        return carLambController._maxSpeed * carLambController.boostMultiplier * speedometerArrowFactor;
    }

    enum RpmState
    {
        Low,
        Medium,
        High
    }
}
