using UnityEngine;

public class MocapTrackerInputProvider : PlayerInputProvider
{
    [Header("Tracking References")]
    [Tooltip("Receiver Plugin이 움직이는 루트가 있으면 넣는다. 없으면 비워도 된다.")]
    [SerializeField] private Transform trackingRoot;

    [Tooltip("가능하면 Hips/Pelvis/BodyCenter 계열 Transform을 넣는다.")]
    [SerializeField] private Transform bodyCenter;

    [Tooltip("bodyCenter가 없을 때 임시로 Head Transform을 넣는다.")]
    [SerializeField] private Transform head;

    [Header("Calibration")]
    [SerializeField] private bool calibrateOnStart = true;
    [SerializeField] private float autoCalibrateDelay = 1.0f;
    [SerializeField] private KeyCode recalibrateKey = KeyCode.C;

    [Header("Horizontal Movement")]
    [Tooltip("기준 위치에서 이 거리만큼 좌우 이동하면 입력값 1로 본다.")]
    [SerializeField] private float horizontalRange = 0.35f;

    [Tooltip("이 거리 이하는 흔들림으로 보고 무시한다.")]
    [SerializeField] private float horizontalDeadZone = 0.08f;

    [SerializeField] private float horizontalSmooth = 12f;
    [SerializeField] private bool invertHorizontal;

    [Header("Jump Detection")]
    [Tooltip("기준 높이보다 이만큼 올라가면 점프 후보로 본다.")]
    [SerializeField] private float jumpHeightThreshold = 0.16f;

    [Tooltip("상승 속도가 이 값 이상일 때만 점프로 본다.")]
    [SerializeField] private float jumpVelocityThreshold = 0.55f;

    [Tooltip("연속 점프 오입력 방지 시간.")]
    [SerializeField] private float jumpCooldown = 0.45f;

    [Tooltip("기준 높이 근처로 돌아오면 다음 점프를 허용한다.")]
    [SerializeField] private float groundedResetThreshold = 0.08f;

    [Header("Debug / Fallback")]
    [SerializeField] private bool enableKeyboardFallback = true;
    [SerializeField] private bool showDebugLog;
    [SerializeField] private bool drawDebugGizmos = true;

    public bool IsCalibrated => calibrated;
    public float CurrentHorizontal => smoothedHorizontal;
    public float CurrentHeightDelta => currentHeightDelta;
    public float CurrentVerticalVelocity => currentVerticalVelocity;

    private bool calibrated;
    private bool jumpArmed = true;

    private float calibrationTimer;
    private float baseX;
    private float baseY;
    private float previousY;
    private float smoothedHorizontal;
    private float lastJumpTime = -999f;

    private float currentHeightDelta;
    private float currentVerticalVelocity;

    public override PlayerInputState ReadInput()
    {
        if (Input.GetKeyDown(recalibrateKey))
        {
            Calibrate();
        }

        if (!HasTrackingReference())
        {
            return enableKeyboardFallback
                ? ReadKeyboardFallback()
                : default;
        }

        if (!calibrated)
        {
            TryAutoCalibrate();

            return enableKeyboardFallback
                ? ReadKeyboardFallback()
                : default;
        }

        Vector3 bodyPosition = GetTrackingSpacePosition();

        PlayerInputState state = new PlayerInputState
        {
            Horizontal = ReadHorizontal(bodyPosition),
            JumpPressed = ReadJump(bodyPosition),
            SlidePressed = false
        };

        if (enableKeyboardFallback)
        {
            ApplyKeyboardFallback(ref state);
        }

        return state;
    }

    private bool HasTrackingReference()
    {
        return bodyCenter != null || head != null;
    }

    private Transform GetMainTrackingTransform()
    {
        if (bodyCenter != null)
            return bodyCenter;

        return head;
    }

    private Vector3 GetTrackingSpacePosition()
    {
        Transform target = GetMainTrackingTransform();

        if (target == null)
            return Vector3.zero;

        if (trackingRoot != null)
            return trackingRoot.InverseTransformPoint(target.position);

        return target.position;
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
        if (!HasTrackingReference())
        {
            Debug.LogWarning("[MocapInput] 보정 실패: BodyCenter 또는 Head Transform이 연결되지 않았습니다.");
            return;
        }

        Vector3 bodyPosition = GetTrackingSpacePosition();

        baseX = bodyPosition.x;
        baseY = bodyPosition.y;
        previousY = bodyPosition.y;

        smoothedHorizontal = 0f;
        currentHeightDelta = 0f;
        currentVerticalVelocity = 0f;

        jumpArmed = true;
        calibrated = true;

        Debug.Log($"[MocapInput] 보정 완료. BaseX: {baseX:0.000}, BaseY: {baseY:0.000}");
    }

    private float ReadHorizontal(Vector3 bodyPosition)
    {
        float deltaX = bodyPosition.x - baseX;

        float rawHorizontal = deltaX / Mathf.Max(0.001f, horizontalRange);

        if (invertHorizontal)
            rawHorizontal *= -1f;

        if (Mathf.Abs(deltaX) < horizontalDeadZone)
            rawHorizontal = 0f;

        rawHorizontal = Mathf.Clamp(rawHorizontal, -1f, 1f);

        float lerpFactor = 1f - Mathf.Exp(-horizontalSmooth * Time.deltaTime);

        smoothedHorizontal = Mathf.Lerp(
            smoothedHorizontal,
            rawHorizontal,
            lerpFactor
        );

        return smoothedHorizontal;
    }

    private bool ReadJump(Vector3 bodyPosition)
    {
        float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);

        float currentY = bodyPosition.y;

        currentHeightDelta = currentY - baseY;
        currentVerticalVelocity = (currentY - previousY) / deltaTime;

        previousY = currentY;

        if (!jumpArmed && currentHeightDelta <= groundedResetThreshold)
        {
            jumpArmed = true;

            if (showDebugLog)
                Debug.Log("[MocapInput] Jump re-armed.");
        }

        bool isAboveThreshold = currentHeightDelta >= jumpHeightThreshold;
        bool isMovingUpFast = currentVerticalVelocity >= jumpVelocityThreshold;
        bool cooldownReady = Time.time >= lastJumpTime + jumpCooldown;

        if (jumpArmed && isAboveThreshold && isMovingUpFast && cooldownReady)
        {
            jumpArmed = false;
            lastJumpTime = Time.time;

            if (showDebugLog)
            {
                Debug.Log(
                    $"[MocapInput] Jump! HeightDelta: {currentHeightDelta:0.000}, Velocity: {currentVerticalVelocity:0.000}"
                );
            }

            return true;
        }

        return false;
    }

    private PlayerInputState ReadKeyboardFallback()
    {
        PlayerInputState state = default;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            state.Horizontal -= 1f;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            state.Horizontal += 1f;

        state.JumpPressed =
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.UpArrow);

        state.SlidePressed =
            Input.GetKeyDown(KeyCode.LeftControl) ||
            Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.DownArrow);

        return state;
    }

    private void ApplyKeyboardFallback(ref PlayerInputState state)
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            state.Horizontal = -1f;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            state.Horizontal = 1f;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
            state.JumpPressed = true;

        if (Input.GetKeyDown(KeyCode.LeftControl) ||
            Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.DownArrow))
        {
            state.SlidePressed = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawDebugGizmos || !HasTrackingReference())
            return;

        Transform target = GetMainTrackingTransform();

        if (target == null)
            return;

        Gizmos.color = calibrated ? Color.cyan : Color.yellow;
        Gizmos.DrawWireSphere(target.position, 0.08f);

        if (!calibrated)
            return;

        Vector3 baseWorldPosition;

        if (trackingRoot != null)
        {
            baseWorldPosition = trackingRoot.TransformPoint(new Vector3(baseX, baseY, 0f));
        }
        else
        {
            Vector3 targetPosition = target.position;
            baseWorldPosition = new Vector3(baseX, baseY, targetPosition.z);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(baseWorldPosition, 0.1f);
        Gizmos.DrawLine(baseWorldPosition, target.position);
    }
}