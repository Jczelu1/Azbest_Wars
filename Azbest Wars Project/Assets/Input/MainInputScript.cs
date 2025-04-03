using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

//public class MainInputSystem : MonoBehaviour
//{
//    public static MainInputSystem Instance;
//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }
//        DontDestroyOnLoad(gameObject);
//    }


//    private bool Selecting = false;
//    int2 SelectStartPosition = new int2(-1,-1);
//    [HideInInspector]
//    public bool Selected = false;


//    public InputAction selectAction;
    
//    private void Start()
//    {
//        selectAction = InputSystem.actions.FindAction("LeftClick");
//        selectAction.Enable();
//    }

//    private void OnDisable()
//    {
//        selectAction.Disable();
//    }

//    private void Update()
//    {
//        if (selectAction.WasPressedThisFrame())
//        {
           
//        }
//        if (selectAction.WasReleasedThisFrame())
//        {
//            Selecting = false;
//            Selected = true;

//        }
//        if (selectAction.IsPressed())
//        {
//            Selected = false;
//            Selecting = true;

//        }
//    }
//}
