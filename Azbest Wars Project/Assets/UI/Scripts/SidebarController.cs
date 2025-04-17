using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SidebarController : MonoBehaviour
{
    [SerializeField]
    GameObject Sidebar;

    [SerializeField]
    GameObject[] TickrateButtons;

    private InputAction pauseAction;
    private InputAction x1Action;
    private InputAction x2Action;
    private InputAction x4Action;
    private InputAction x8Action;
    private void Start()
    {
        pauseAction = InputSystem.actions.FindAction("Pause");
        x1Action = InputSystem.actions.FindAction("x1");
        x2Action = InputSystem.actions.FindAction("x2");
        x4Action = InputSystem.actions.FindAction("x4");
        x8Action = InputSystem.actions.FindAction("x8");
        TickrateButtons[2].GetComponent<CustomButtonScript>().Select();
    }
    public void Update()
    {
        if (pauseAction.WasPressedThisFrame())
        {
            TickrateButtons[0].GetComponent<Button>().onClick.Invoke();
        }
        if (x1Action.WasPressedThisFrame())
        {
            TickrateButtons[1].GetComponent<Button>().onClick.Invoke();
        }
        if (x2Action.WasPressedThisFrame())
        {
            TickrateButtons[2].GetComponent<Button>().onClick.Invoke();
        }
        if (x4Action.WasPressedThisFrame())
        {
            TickrateButtons[4].GetComponent<Button>().onClick.Invoke();
        }
        if (x8Action.WasPressedThisFrame())
        {
            TickrateButtons[8].GetComponent<Button>().onClick.Invoke();
        }
    }
    public void ShowSidebar()
    {
        Sidebar.SetActive(true);
    }
    public void HideSidebar()
    {
        Sidebar.SetActive(false);
    }
    public void UnselectTickrateButtons()
    {
        foreach(var b in TickrateButtons)
        {
            if(b != null)
                b.GetComponent<CustomButtonScript>().UnSelect();
        }
    }
    public void SetTickrate(string stringLevel)
    {
        byte level;
        if(byte.TryParse(stringLevel, out level))
        {
            TickSystemGroup.SetTickrate(level);
            UnselectTickrateButtons();
            if(TickrateButtons.Length > level)
            {
                TickrateButtons[level].GetComponent<CustomButtonScript>().Select();
            }
        }
    }
}
