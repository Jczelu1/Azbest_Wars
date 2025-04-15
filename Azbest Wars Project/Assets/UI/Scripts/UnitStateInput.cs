using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitStateInput : MonoBehaviour
{
    InputAction DefendAction;
    InputAction AttackAction;
    InputAction StopAction;

    [SerializeField]
    CustomButtonScript[] movementStateButtons;

    [SerializeField]
    GameObject movementStateUI;
    private bool showingUI = true;

    public static byte currentMovementState = 255;
    void Start()
    {
        DefendAction = InputSystem.actions.FindAction("Defend");
        AttackAction = InputSystem.actions.FindAction("Attack");
        StopAction = InputSystem.actions.FindAction("Stop");
    }

    void Update()
    {
        if (MainGridScript.Instance.Selected)
        {
            if (DefendAction.WasPressedThisFrame())
            {
                SetMovementState("0");
            }
            if (AttackAction.WasPressedThisFrame())
            {
                SetMovementState("1");
            }
            if (StopAction.WasPressedThisFrame())
            {
                SetMovementState("2");
            }
        }

        if (!showingUI && SelectSystem.unitsSelected > 0)
        {
            showingUI = true;
            movementStateUI.SetActive(true);
        }
        else if(showingUI && SelectSystem.unitsSelected < 1)
        {
            showingUI = false;
            movementStateUI.SetActive(false);
        }
        if (!showingUI) return;
        for (int i = 0; i < 2; i++)
        {
            movementStateButtons[i].UnSelect();
        }
        if (currentMovementState < 3)
        {
            if (currentMovementState == 2) currentMovementState = 0;
            movementStateButtons[currentMovementState].Select();
        }
    }
    public void SetMovementState(string state)
    {
        byte s;
        if(byte.TryParse(state, out s))
        {
            if (s > 2) return;
            PathfindSystem.setMoveState[TeamManager.Instance.PlayerTeam] = s;
            currentMovementState = s;
        }
    }
}
