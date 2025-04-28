using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialUI : MonoBehaviour
{
    public static TutorialUI Instance;

    public InputAction moveAction;
    public InputAction scrollAction;
    public InputAction middleClickAction;

    public bool moved = false;
    public bool maped = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        moveAction = InputSystem.actions.FindAction("Move");
        scrollAction = InputSystem.actions.FindAction("ScrollWheel");
        middleClickAction = InputSystem.actions.FindAction("MiddleClick");
    }
    private void Update()
    {
        if (moveAction.WasPerformedThisFrame())
        {
            moved = true;
        }
        if(scrollAction.WasPerformedThisFrame() || middleClickAction.WasPerformedThisFrame())
        {
            maped = true;
        }
    }

    public GameObject[] tutorialControls;
}
