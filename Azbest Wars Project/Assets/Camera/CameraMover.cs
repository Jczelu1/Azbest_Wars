using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMover : MonoBehaviour
{
    public float moveInterval = 0.25f;
    private float timer = 0f;
    private Vector2 movementInput = Vector2.zero;
    private bool moving = false;
    public Vector2 minCameraPosition;
    public Vector2 maxCameraPosition;

    private InputAction moveAction;

    void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        moveAction.Enable();
    }

    void Update()
    {
        timer += Time.deltaTime;

        Vector2 newInput = moveAction.ReadValue<Vector2>();
        if (!moving && newInput != Vector2.zero)
        {
            movementInput = newInput;
        }

        if (timer >= moveInterval)
        {

            if (moving)
            {
                movementInput = newInput;
            }
            moving = false;

            if (movementInput != Vector2.zero)
            {
                moving = true;
                Vector3 move = new Vector3((Mathf.Abs(movementInput.x)>0.1)?Mathf.Sign(movementInput.x):0, (Mathf.Abs(movementInput.y) > 0.1) ? Mathf.Sign(movementInput.y) : 0, 0);

                float x = transform.position.x + move.x;
                float y = transform.position.y + move.y;
                if (x < minCameraPosition.x || x > maxCameraPosition.x) move.x = 0;
                if (y < minCameraPosition.y || y > maxCameraPosition.y) move.y = 0;
                transform.Translate(move, Space.World);
                movementInput = Vector2.zero;
            }
            timer = 0f;
        }
    }

    private void OnDisable()
    {
        moveAction.Disable();
    }
}
