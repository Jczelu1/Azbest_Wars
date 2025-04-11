using UnityEngine;

public class SidebarController : MonoBehaviour
{
    [SerializeField]
    GameObject Sidebar;

    [SerializeField]
    CustomButtonScript[] TickrateButtons;
    private void Start()
    {
        TickrateButtons[1].Select();
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
                b.UnSelect();
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
                TickrateButtons[level].Select();
            }
        }
    }
}
