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

    [Header("Horizontal Position Mapping")]
    [Tooltip("실제 Pelvis가 이만큼 좌우로 움직이면 게임 안에서 targetXAtRange만큼 이동한다.")]
    [SerializeField] private float pelvisHorizontalRange = 0.35f;

    [Tooltip("Pelvis가 pelvisHorizontalRange만큼 움직였을 때 게임 캐릭터의 목표 X 위치.")]
    [SerializeField] private float targetXAtRange = 3f;

    [Tooltip("이 거리 이하는 흔들림으로 보고 중앙 처리한다.")]
    [SerializeField] private float horizontalDeadZone = 0.05f;

    [SerializeField] private bool invertHorizontal;

    [Header("Old Button-like Horizontal Mode - Kept For Reference")]
    [SerializeField] private bool useOldButtonLikeHorizontalMode;

    [Tooltip("기존 방식에서 사용하던 Horizontal smoothing. useOldButtonLikeHorizontalMode가 켜졌을 때만 사용.")]
    [SerializeField] private float oldHorizontalSmooth = 12f;

    [Header("Jump")]
    [SerializeField] private float jumpHeightThreshold = 0.16f;
    [SerializeField] private float jumpVelocityThreshold = 0.55f;
    [SerializeField] private float jumpCooldown = 0.45f;
    [SerializeField] private float jumpResetThreshold = 0.08f;

    [Header("Slide")]
    [Tooltip("기준 골반 높이보다 이만큼 내려가면 슬라이딩 시작.")]
    [SerializeField] private float slideDownThreshold = 0.14f;

    [Tooltip("기준 골반 높이 근처로 이만큼 돌아오면 슬라이딩 해제.")]
    [SerializeField] private float slideReleaseThreshold = 0.06f;

    [SerializeField] private float slideSmooth = 16f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog;

    public bool IsCalibrated => isCalibrated;
    public float CurrentTargetX => currentTargetX;
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

    private float currentTargetX;
    private float currentHorizontal;
    private float currentHeightDelta;
    private float currentVerticalVelocity;

    private float lastJumpTime = -999f;
    private float slideValue;

    public override PlayerInputState ReadInput()
    {
        if (Input.GetKeyDown(recalibrateKey))
        {
            CalibrateNow();
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

        UpdateVerticalMetrics(pelvisPosition);

        bool jumpPressed = ReadJump();
        bool slideHeld = ReadSlideHeld();

        if (useOldButtonLikeHorizontalMode)
        {
            float horizontal = ReadOldButtonLikeHorizontal(pelvisPosition);

            return new PlayerInputState
            {
                Horizontal = horizontal,
                HasTargetX = false,
                TargetX = 0f,
                JumpPressed = jumpPressed,
                SlidePressed = slideHeld,
                SlideHeld = slideHeld
            };
        }

        float targetX = ReadTargetX(pelvisPosition);

        return new PlayerInputState
        {
            Horizontal = 0f,
            HasTargetX = true,
            TargetX = targetX,
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
            CalibrateNow();
        }
    }

public void CalibrateNow()    {
        if (pelvis == null)
        {
            Debug.LogWarning("[MocapPelvisInput] Pelvis Transform이 연결되지 않았습니다.");
            return;
        }

        Vector3 pelvisPosition = GetPelvisPosition();

        baseX = pelvisPosition.x;
        baseY = pelvisPosition.y;
        previousY = pelvisPosition.y;

        currentTargetX = 0f;
        currentHorizontal = 0f;
        currentHeightDelta = 0f;
        currentVerticalVelocity = 0f;
        slideValue = 0f;

        jumpArmed = true;
        isSlideHeld = false;
        isCalibrated = true;

        Debug.Log($"[MocapPelvisInput] 보정 완료. BaseX: {baseX:0.000}, BaseY: {baseY:0.000}");
    }

    private float ReadTargetX(Vector3 pelvisPosition)
    {
        float deltaX = pelvisPosition.x - baseX;

        if (invertHorizontal)
            deltaX *= -1f;

        if (Mathf.Abs(deltaX) < horizontalDeadZone)
            deltaX = 0f;

        float normalized = deltaX / Mathf.Max(0.001f, pelvisHorizontalRange);
        normalized = Mathf.Clamp(normalized, -1f, 1f);

        currentTargetX = normalized * targetXAtRange;

        return currentTargetX;
    }

    private float ReadOldButtonLikeHorizontal(Vector3 pelvisPosition)
    {
        float deltaX = pelvisPosition.x - baseX;

        if (invertHorizontal)
            deltaX *= -1f;

        if (Mathf.Abs(deltaX) < horizontalDeadZone)
            deltaX = 0f;

        float rawHorizontal = deltaX / Mathf.Max(0.001f, pelvisHorizontalRange);
        rawHorizontal = Mathf.Clamp(rawHorizontal, -1f, 1f);

        float lerpFactor = 1f - Mathf.Exp(-oldHorizontalSmooth * Time.deltaTime);

        currentHorizontal = Mathf.Lerp(
            currentHorizontal,
            rawHorizontal,
            lerpFactor
        );

        // 기존 방식:
        // currentHorizontal이 -1~1 값으로 유지되고,
        // PlayerController가 이 값을 속도처럼 사용했다.
        //
        // 예:
        // Pelvis가 왼쪽에 계속 있음
        // → currentHorizontal = -0.8 유지
        // → 왼쪽 버튼을 계속 누른 것처럼 캐릭터가 계속 이동

        return currentHorizontal;
    }

    private void UpdateVerticalMetrics(Vector3 pelvisPosition)
    {
        float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);

        float currentY = pelvisPosition.y;

        currentHeightDelta = currentY - baseY;
        currentVerticalVelocity = (currentY - previousY) / deltaTime;

        previousY = currentY;
    }

    private bool ReadJump()
    {
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

    private bool ReadSlideHeld()
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
                Debug.Log($"[MocapPelvisInput] Slide start. Down: {slideValue:0.000}");
        }
        else if (isSlideHeld && slideValue <= slideReleaseThreshold)
        {
            isSlideHeld = false;

            if (showDebugLog)
                Debug.Log($"[MocapPelvisInput] Slide end. Down: {slideValue:0.000}");
        }

        return isSlideHeld;
    }
}