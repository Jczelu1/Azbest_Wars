using UnityEngine;

public class LoadingScreenUI : MonoBehaviour
{
    void Update()
    {
        if(SetupSystem.startDelay == -1)
        {
            this.gameObject.SetActive(false);
        }
    }
}
