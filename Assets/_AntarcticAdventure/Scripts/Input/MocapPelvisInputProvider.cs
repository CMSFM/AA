using UnityEngine;

public class MocapPelvisInputProvider : PlayerInputProvider
{
    [Header("Pelvis Tracking")]
    [SerializeField] private Transform pelvis;

    [Tooltip("Receiver가 Pelvis 오브젝트 자체를 월드에서 움직이면 false. 본 로컬 움직임을 읽어야 하면 true.")]
    [SerializeField] private bool useLocalPosition;

    [Header("Calibration")]
    [SerializeField] private bool calibrateOnStart = true;
    [SerializeField] private float autoCalibrateDelay = 1f;
    [SerializeField] private KeyCode recalibrateKey = KeyCode.C;

    [Header("Horizontal")]
    [SerializeField] private float horizontalRange = 0.35f;
    [SerializeField] private float horizontalDeadZone = 0.08f;
    [SerializeField] private float horizontalSmooth = 12f;
    [SerializeField] private bool invertHorizontal;

    [Header("Jump")]
    [SerializeField] private float jumpHeightThreshold = 0.16f;
    [SerializeField] private float jumpVelocityThreshold = 0.55f;
    [SerializeField] private float jumpCooldown = 0.45f;
    [SerializeField] private float jumpResetThreshold = 0.08f;

    [Header("Slide")]
    [Tooltip("기준 골반 높이보다 이만큼 내려가면 슬라이딩 시작.")]
    [SerializeField] private float slideDownThreshold = 0.14f;

    [Tooltip("기준 골반 높이 근처로 이만큼 돌아오면 슬라이딩 해제. slideDownThreshold보다 작게 둬서 떨림 방지.")]
    [SerializeField] private float slideReleaseThreshold = 0.06f;

    [SerializeField] private float slideSmooth = 16f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog;

    public bool IsCalibrated => isCalibrated;
    public float CurrentHorizontal => currentHorizontal;
    public float CurrentHeightDelta => currentHeightDelta;
    public float CurrentVerticalVelocity => currentVerticalVelocity;
    public bool IsSlideHeld => isSlideHeld;

    private bool isCalibrated;
    private bool jumpArmed = true;
    private bool isSlideHeld;

    private float calibrationTimer;
    private float baseX;
    private float baseY;
    private float previousY;
    private float currentHorizontal;
    private float currentHeightDelta;
    private float currentVerticalVelocity;
    private float lastJumpTime = -999f;

    private float slideValue;

    public override PlayerInputState ReadInput()
    {
        if (Input.GetKeyDown(recalibrateKey))
        {
            Calibrate();
        }

        if (pelvis == null)
        {
            return default;
        }

        if (!isCalibrated)
        {
            TryAutoCalibrate();
            return default;
        }

        Vector3 pelvisPosition = GetPelvisPosition();

        float horizontal = ReadHorizontal(pelvisPosition);
        bool jumpPressed = ReadJump(pelvisPosition);
        bool slideHeld = ReadSlideHeld(pelvisPosition);

        return new PlayerInputState
        {
            Horizontal = horizontal,
            JumpPressed = jumpPressed,
            SlidePressed = slideHeld,
            SlideHeld = slideHeld
        };
    }

    private Vector3 GetPelvisPosition()
    {
        return useLocalPosition
            ? pelvis.localPosition
            : pelvis.position;
    }

    private void TryAutoCalibrate()
    {
        if (!calibrateOnStart)
            return;

        calibrationTimer += Time.deltaTime;

        if (calibrationTimer >= autoCalibrateDelay)
        {
            Calibrate();
        }
    }

    public void Calibrate()
    {
        if (pelvis == null)
        {
            Debug.LogWarning("[MocapPelvisInput] Pelvis Transform이 연결되지 않았습니다.");
            return;
        }

        Vector3 pelvisPosition = GetPelvisPosition();

        baseX = pelvisPosition.x;
        baseY = pelvisPosition.y;
        previousY = pelvisPosition.y;

        currentHorizontal = 0f;
        currentHeightDelta = 0f;
        currentVerticalVelocity = 0f;
        slideValue = 0f;

        jumpArmed = true;
        isSlideHeld = false;
        isCalibrated = true;

        Debug.Log($"[MocapPelvisInput] 보정 완료. BaseX: {baseX:0.000}, BaseY: {baseY:0.000}");
    }

    private float ReadHorizontal(Vector3 pelvisPosition)
    {
        float deltaX = pelvisPosition.x - baseX;

        if (Mathf.Abs(deltaX) < horizontalDeadZone)
        {
            deltaX = 0f;
        }

        float rawHorizontal = deltaX / Mathf.Max(0.001f, horizontalRange);

        if (invertHorizontal)
        {
            rawHorizontal *= -1f;
        }

        rawHorizontal = Mathf.Clamp(rawHorizontal, -1f, 1f);

        float lerpFactor = 1f - Mathf.Exp(-horizontalSmooth * Time.deltaTime);

        currentHorizontal = Mathf.Lerp(
            currentHorizontal,
            rawHorizontal,
            lerpFactor
        );

        return currentHorizontal;
    }

    private bool ReadJump(Vector3 pelvisPosition)
    {
        float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);

        float currentY = pelvisPosition.y;

        currentHeightDelta = currentY - baseY;
        currentVerticalVelocity = (currentY - previousY) / deltaTime;

        previousY = currentY;

        if (!jumpArmed && currentHeightDelta <= jumpResetThreshold)
        {
            jumpArmed = true;

            if (showDebugLog)
                Debug.Log("[MocapPelvisInput] Jump re-armed.");
        }

        if (isSlideHeld)
            return false;

        bool heightEnough = currentHeightDelta >= jumpHeightThreshold;
        bool velocityEnough = currentVerticalVelocity >= jumpVelocityThreshold;
        bool cooldownReady = Time.time >= lastJumpTime + jumpCooldown;

        if (jumpArmed && heightEnough && velocityEnough && cooldownReady)
        {
            jumpArmed = false;
            lastJumpTime = Time.time;

            if (showDebugLog)
            {
                Debug.Log(
                    $"[MocapPelvisInput] Jump. HeightDelta: {currentHeightDelta:0.000}, Velocity: {currentVerticalVelocity:0.000}"
                );
            }

            return true;
        }

        return false;
    }

    private bool ReadSlideHeld(Vector3 pelvisPosition)
    {
        float targetSlideValue = currentHeightDelta < 0f
            ? Mathf.Abs(currentHeightDelta)
            : 0f;

        float lerpFactor = 1f - Mathf.Exp(-slideSmooth * Time.deltaTime);

        slideValue = Mathf.Lerp(
            slideValue,
            targetSlideValue,
            lerpFactor
        );

        if (!isSlideHeld && slideValue >= slideDownThreshold)
        {
            isSlideHeld = true;

            if (showDebugLog)
            {
                Debug.Log($"[MocapPelvisInput] Slide start. Down: {slideValue:0.000}");
            }
        }
        else if (isSlideHeld && slideValue <= slideReleaseThreshold)
        {
            isSlideHeld = false;

            if (showDebugLog)
            {
                Debug.Log($"[MocapPelvisInput] Slide end. Down: {slideValue:0.000}");
            }
        }

        return isSlideHeld;
    }
}