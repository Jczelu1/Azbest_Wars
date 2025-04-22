using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoBoardUI : MonoBehaviour
{
    public static InfoBoardUI Instance;
    private const int MAX_LINES = 10;
    private int displayingLines = 0;

    [SerializeField]
    private TextMeshProUGUI infoBoardText;

    [SerializeField]
    private float textSpeed = 0.05f;

    private readonly Queue<string> messageQueue = new Queue<string>();
    private bool isTyping = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowText(string text)
    {
        messageQueue.Enqueue(text);
        if (!isTyping)
        {
            StartCoroutine(ProcessQueue());
        }
    }
    private IEnumerator ProcessQueue()
    {
        isTyping = true;
        while (messageQueue.Count > 0)
        {
            string nextText = messageQueue.Dequeue();
            yield return StartCoroutine(ShowTextCoroutine(nextText));
        }
        isTyping = false;
    }

    private IEnumerator ShowTextCoroutine(string text)
    {
        if (displayingLines >= MAX_LINES)
        {
            int newLineIndex = infoBoardText.text.IndexOf('\n');
            if (newLineIndex >= 0)
            {
                infoBoardText.text = infoBoardText.text.Substring(newLineIndex);
                displayingLines--;
            }
        }

        infoBoardText.text += "\n";

        foreach (char c in text)
        {
            infoBoardText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
        yield return new WaitForSeconds(textSpeed);
        displayingLines++;
    }
}
