using UnityEngine;

public class TestMover : MonoBehaviour
{
    [SerializeField] private InputReader input;
    [SerializeField] private float speed = 5f;

    private void Awake()
    {
        // Temporary. GameStateManager (SYS-10.3) owns map switching once it exists.
        if (input != null) input.EnablePlayer();
    }

    private void Update()
    {
        if (input == null) return;
        transform.position += (Vector3)input.MoveValue * (speed * Time.deltaTime);
    }
}