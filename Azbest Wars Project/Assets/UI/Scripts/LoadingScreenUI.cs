using UnityEngine;

public class LoadingScreenUI : MonoBehaviour
{
    void Update()
    {
        if(!SetupSystem.started)
        {
            this.gameObject.SetActive(false);
        }
    }
}
