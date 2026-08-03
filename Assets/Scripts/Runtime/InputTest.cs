using UnityEngine;

public class InputTest : MonoBehaviour
{
    [SerializeField] private InputReader input;

    private void OnEnable()
    {
        if (input == null)
        {
            Debug.LogError("InputTest: no InputReader assigned.", this);
            enabled = false;
            return;
        }

        input.MoveEvent += OnMove;
        input.InteractEvent += OnInteract;
        input.OpenInventoryEvent += OnOpenInventory;
    }

    private void OnDisable()
    {
        if (input == null) return;

        input.MoveEvent -= OnMove;
        input.InteractEvent -= OnInteract;
        input.OpenInventoryEvent -= OnOpenInventory;
    }

    private void OnMove(Vector2 v) => Debug.Log($"Move {v}");
    private void OnInteract() => Debug.Log("Interact");
    private void OnOpenInventory() => Debug.Log("OpenInventory");
}