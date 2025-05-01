using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraMover : MonoBehaviour
{
    private Camera m_Camera;
    public float moveInterval = 0.25f;
    private float timer = 0f;
    private Vector2 movementInput = Vector2.zero;
    private bool moving = false;

    [HideInInspector]
    public Vector2 minCameraPosition;
    [HideInInspector]
    public Vector2 maxCameraPosition;
    private float maxCamSize = 18f;
    private float minCamSize = 4.5f;

    public float edgeScrollThreshold = 0.05f;

    private InputAction moveAction;
    private InputAction scrollWheelAction;
    private InputAction zoomPlusAction;
    private InputAction zoomMinusAction;



    void Awake()
    {
        scrollWheelAction = InputSystem.actions.FindAction("ScrollWheel");
        moveAction = InputSystem.actions.FindAction("Move");
        zoomPlusAction = InputSystem.actions.FindAction("ZoomPlus");
        zoomMinusAction = InputSystem.actions.FindAction("ZoomMinus");
        moveAction.Enable();
    }
    private void Start()
    {
        m_Camera = GetComponent<Camera>();
    }

    void Update()
    {
        Vector2 scrollValue = scrollWheelAction.ReadValue<Vector2>();

        if ((scrollValue.y > 0 || zoomPlusAction.WasPressedThisFrame()) && m_Camera.orthographicSize > (minCamSize+.5f))
        {
            m_Camera.orthographicSize -= 4.5f;
            moveInterval *= 1.5f;
        }
        else if ((scrollValue.y < 0 || zoomMinusAction.WasPressedThisFrame()) && m_Camera.orthographicSize < maxCamSize)
        {
            m_Camera.orthographicSize += 4.5f;
            moveInterval /= 1.5f;
        }
        timer += Time.deltaTime;

        Vector2 keyboardInput = moveAction.ReadValue<Vector2>();

        //mouse edging
        Vector2 edgeInput = Vector2.zero;
        //Vector2 mousePos = Input.mousePosition;
        //if (mousePos.x >= 0 && mousePos.x <= Screen.width && mousePos.y >= 0 && mousePos.y <= Screen.height)
        //{
        //    float normalizedX = mousePos.x / Screen.width;
        //    float normalizedY = mousePos.y / Screen.height;

        //    if (normalizedX <= edgeScrollThreshold)
        //        edgeInput.x = -1f;
        //    else if (normalizedX >= 1f - edgeScrollThreshold)
        //        edgeInput.x = 1f;

        //    if (normalizedY <= edgeScrollThreshold)
        //        edgeInput.y = -1f;
        //    else if (normalizedY >= 1f - edgeScrollThreshold)
        //        edgeInput.y = 1f;
        //}

        //combine inputs
        Vector2 newInput = keyboardInput != Vector2.zero ? keyboardInput : edgeInput;

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
                if (x + .1f < minCameraPosition.x || x - .1f > maxCameraPosition.x) move.x = 0;
                if (y + .1f < minCameraPosition.y || y - .1f > maxCameraPosition.y) move.y = 0;
                transform.Translate(move, Space.World);
                movementInput = Vector2.zero;
            }
            timer = 0f;
        }
    }
    public void MoveCamera(Vector3 cameraPosition)
    {
        transform.position = cameraPosition;
    }
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    private void OnDisable()
    {
        //moveAction.Disable();
    }
}
