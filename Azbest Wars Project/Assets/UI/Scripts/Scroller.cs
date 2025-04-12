using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Scroller : MonoBehaviour
{
    public TextMeshProUGUI textObj;
    public string[] texts;
    public string[] keys;
    public int current = 0;
    public bool loop;

    private void Start()
    {
        textObj.text = texts[current];
    }
    public void Left()
    {
        if (current == 0)
        {
            if (loop)
            {
                current = texts.Length - 1;
            }
        }
        else
        {
            current--;
        }
        textObj.text = texts[current];
    }
    public void Right()
    {
        if (current == texts.Length - 1)
        {
            if (loop)
            {
                current = 0;
            }
        }
        else
        {
            current++;
        }
        textObj.text = texts[current];
    }
    public void SetScroller(int setTo)
    {
        if (setTo >= texts.Length) return;
        current = setTo;
        textObj.text = texts[current];
    }
    public void ResetScroller()
    {
        textObj.text = texts[current];
    }
}
