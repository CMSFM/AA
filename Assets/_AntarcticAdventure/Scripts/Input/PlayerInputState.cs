public struct PlayerInputState
{
    // 기존 버튼/스틱식 입력.
    // Horizontal 값이 -1이면 왼쪽으로 계속 이동,
    // 1이면 오른쪽으로 계속 이동하는 방식이다.
    public float Horizontal;

    // 새 위치 매핑식 입력.
    // HasTargetX가 true이면 PlayerController는 Horizontal 대신 TargetX를 사용한다.
    public bool HasTargetX;
    public float TargetX;

    public bool JumpPressed;

    public bool SlidePressed;
    public bool SlideHeld;
}