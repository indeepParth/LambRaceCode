using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCameraFollow : MonoBehaviour
{
    public Transform target; // The car to follow
    public Vector3 offset; // Offset from the car
    public float followSpeed = 10f; // Speed at which the camera follows the car
    public float rotateSpeed = 5f; // Speed at which the camera rotates to match the car's rotation
    private bool isFocusOnCar = false;
    private Vector3 lastCamPosOnCar = Vector3.zero;
    public Vector3 offsetPassanger;

    public Camera myCamera;

    [Space(10)]
    [Header("Camera Look Around Settings")]
    public bool isLookAround = false; // Enable/disable look around feature
    private bool isInitSettings = false; // Flag to check if settings are initialized
    public float rotationSpeedLookAround = 5.0f;  // Mouse rotation sensitivity
    public float zoomSpeed = 2.0f;  // Scroll wheel zoom speed
    public float minZoom = 5.0f;  // Minimum zoom distance
    public float maxZoom = 20.0f; // Maximum zoom distance

    private float distance = 10.0f; // Default camera distance
    private float yaw = 0.0f;  // Horizontal rotation angle
    private float pitch = 10.0f; // Vertical rotation angle (initial tilt)
    private bool isTouching = false;

    private void Start()
    {
        lastCamPosOnCar = transform.position;
        OnCameraFocusChange(true);
    }

    void LateUpdate()
    {
        if (isLookAround) return;
        isInitSettings = false;

        if (isFocusOnCar)
        {
            Vector3 playerForward = (MyGameController.instance.MyManager.carLambRigidbody.velocity + target.transform.forward).normalized;
            transform.position = Vector3.Lerp(transform.position,
                target.position + target.transform.TransformVector(offset)
                + playerForward * (-rotateSpeed),
                followSpeed * Time.deltaTime);

            // Debug.Log("playerForward" + playerForward);
            // if (playerForward.z > 0 && !MyGameController.instance.MyManager.carLambController.isReverse)
            // { MyGameController.instance.MyManager.carLambController.OnReverseUpdateCarChildPosition(false); }
            // else if (playerForward.z < 0 && MyGameController.instance.MyManager.carLambController.isReverse)
            // { MyGameController.instance.MyManager.carLambController.OnReverseUpdateCarChildPosition(true); }

            lastCamPosOnCar = transform.position;
        }
        else
        {
            Vector3 playerForward = target.transform.forward.normalized;
            transform.position = Vector3.Lerp(transform.position,
                target.position + target.transform.TransformVector(offsetPassanger)
                + playerForward * (-rotateSpeed),
                followSpeed * Time.deltaTime);
        }

        transform.LookAt(target);
    }

    public void OnCameraFocusChange(bool _isFocusOnCar)
    {
        if (!_isFocusOnCar && MyGameController.instance.MyManager.pickupDropPassangerManager.passanger != null)
        {
            isFocusOnCar = false;
            target = MyGameController.instance.MyManager.pickupDropPassangerManager.passanger.transform;
            //Vector3 _pos = MyGameController.instance.MyManager.carLamb.position;
            //_pos.y = transform.position.y + 3;
            //transform.DOMove(_pos, 0.5f);
            myCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("carsAI"));
        }
        else
        {
            transform.DOMove(lastCamPosOnCar, 0);
            isFocusOnCar = true;
            target = MyGameController.instance.MyManager.carLamb;
            myCamera.cullingMask |= 1 << LayerMask.NameToLayer("carsAI");

            InitCameraSettings();
        }
    }

    void Update()
    {
        if (target == null || !isFocusOnCar || MyGameController.instance.gameMode != GameMode.FreeRide
        || !MyGameController.instance.isGameStart || MyGameController.instance.isGameOver
        || MyGameController.instance.isGamePause)
            return;

#if UINITY_MOBILE || UNITY_ANDROID || UNITY_IOS
        MobileTouchController();
#else
        MouseController();
#endif

        pitch = Mathf.Clamp(pitch, 0f, 80f); // Clamp vertical rotation

        if (!isLookAround) return;

        // Convert spherical coordinates to position
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 position = target.position - (rotation * Vector3.forward * distance);

        // Apply position and rotation to camera
        transform.position = position;
        transform.LookAt(target.position);
    }
    private void InitCameraSettings()
    {
        if (target != null && MyGameController.instance.gameMode == GameMode.FreeRide) // Look Around
        {
            Vector3 direction = transform.position - target.position;
            distance = direction.magnitude;

            // Initialize angles based on current camera position
            yaw = transform.eulerAngles.y;
            pitch = transform.eulerAngles.x;
            isInitSettings = true;
        }
    }

    private void MouseController()
    {
        // Rotate when the right mouse button is held down
        if (Input.GetMouseButton(0)) // Right Mouse Button (0 = Left, 1 = Right, 2 = Middle)
        {
            if (!isInitSettings) InitCameraSettings();
            isLookAround = true;
            yaw += Input.GetAxis("Mouse X") * rotationSpeedLookAround;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeedLookAround;
            // pitch = Mathf.Clamp(pitch, 0f, 80f); // Limit vertical rotation

            // Zoom using scroll wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minZoom, maxZoom); // Clamp zoom range
        }
        else
        {
            isLookAround = false;
        }
    }

    private void MobileTouchController()
    {
        // Mobile Controls (Touch)
        if (Input.touchCount == 1) // Single touch to rotate
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                isTouching = true;
            }
            else if (touch.phase == TouchPhase.Moved && isTouching)
            {
                if (!isInitSettings) InitCameraSettings();
                isLookAround = true;
                Vector2 delta = touch.deltaPosition;
                yaw += delta.x * rotationSpeedLookAround * 0.1f;
                pitch -= delta.y * rotationSpeedLookAround * 0.1f;
                // pitch = Mathf.Clamp(pitch, 0f, 80f); // Clamp vertical rotation
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isLookAround = false;
                isTouching = false;
            }
        }

        // Mobile Zoom (Pinch)
        if (Input.touchCount == 2) // Two-finger pinch to zoom
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            float prevDistance = (touch1.position - touch1.deltaPosition - (touch2.position - touch2.deltaPosition)).magnitude;
            float currentDistance = (touch1.position - touch2.position).magnitude;

            float pinchAmount = (prevDistance - currentDistance) * 0.01f;
            distance += pinchAmount * zoomSpeed;
            distance = Mathf.Clamp(distance, minZoom, maxZoom);
        }
    }
}
