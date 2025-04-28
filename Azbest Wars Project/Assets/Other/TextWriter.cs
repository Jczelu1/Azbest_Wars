using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TextWriter : MonoBehaviour
{
    public TextMeshProUGUI textComponent;

    public float timePerCharacter = 0.05f;
    public float timePerNewline = 1f;
    public int maxLines = 10;

    public InputAction skipAction;
    public InputAction skipAllAction;

    public UnityEvent OnFinishWriting;

    [SerializeField]
    [TextArea]
    public string fullText;

    private bool skip = false;
    private bool skipAll = false;
    private bool newLine = false;

    void Awake()
    {
        skipAction = InputSystem.actions.FindAction("Skip");
        skipAllAction = InputSystem.actions.FindAction("SkipAll");
        if (textComponent == null)
        {
            //textComponent = GetComponent<TMP_Text>();
        }
        fullText = textComponent.text;
        textComponent.text = string.Empty;
    }

    void OnEnable()
    {
        StartCoroutine(ShowText());
    }
    public void Update()
    {
        if (skipAction.WasPressedThisFrame()) skip = true;
        if (skipAllAction.WasPressedThisFrame())  skipAll = true;
    }
    public void NotPauseOnSetup()
    {
        SetupSystem.pauseOnSetup = false;
    }
    public void StartTutorial()
    {
        TutorialSystem.startTutorial = true; ;
    }

    IEnumerator ShowText()
    {
        int index = 0;
        int length = fullText.Length;
        int currentLines = 1;

        while (index < length)
        {
            if (skipAll)
            {
                skipAll = false;
                textComponent.text = fullText;
                OnFinishWriting?.Invoke();
                yield break;
            }
            if (newLine)
            {
                skip = false;
            }
            if (skip)
            {
                skip = false;
                int nextNewline = fullText.IndexOf('\n', index);
                if (nextNewline >= 0)
                {
                    textComponent.text += fullText.Substring(index, nextNewline - index + 1);
                    index = nextNewline + 1;
                    currentLines++;

                    if (currentLines > maxLines)
                    {
                        textComponent.text = string.Empty;
                        currentLines = 1;
                    }

                    yield return new WaitForSeconds(timePerCharacter);
                    continue;
                }
                textComponent.text = fullText;
                yield return new WaitForSeconds(timePerNewline);
                OnFinishWriting?.Invoke();
                yield break;
            }

            char c = fullText[index++];
            textComponent.text += c;

            newLine = false;
            if (c == '\n')
            {
                newLine = true;
                currentLines++;
                if (currentLines > maxLines)
                {
                    textComponent.text = string.Empty;
                    currentLines = 1;
                }
                yield return new WaitForSeconds(timePerNewline);
            }
            else
            {
                yield return new WaitForSeconds(timePerCharacter);
            }
        }

        // Completed writing
        yield return new WaitForSeconds(timePerNewline * 2);
        OnFinishWriting?.Invoke();
    }

    public void Restart()
    {
        StopAllCoroutines();
        textComponent.text = string.Empty;
        StartCoroutine(ShowText());
    }
}
