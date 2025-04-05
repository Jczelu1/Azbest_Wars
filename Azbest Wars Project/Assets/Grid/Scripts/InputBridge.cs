using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputBridge : MonoBehaviour
{
    InputAction DefendAction;
    InputAction AttackAction;
    InputAction StopAction;
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
                MovementStateSystem.SetMoveState = 0;
            }
            if (AttackAction.WasPressedThisFrame())
            {
                MovementStateSystem.SetMoveState = 1;
            }
            if (StopAction.WasPressedThisFrame())
            {
                MovementStateSystem.SetMoveState = 2;
            }
        }
        
    }
}
