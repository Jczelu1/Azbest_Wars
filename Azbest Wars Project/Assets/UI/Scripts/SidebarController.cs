using UnityEngine;

public class SidebarController : MonoBehaviour
{
    [SerializeField]
    GameObject Sidebar;
    
    public void ShowSidebar()
    {
        Sidebar.SetActive(true);
    }
    public void HideSidebar()
    {
        Sidebar.SetActive(false);
    }
}
