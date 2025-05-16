using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TextWriter : MonoBehaviour
{
    public TextMeshProUGUI textComponent;

    public float timePerCharacter = 0.05f;
    public float timePerPause = 1f;
    public float endTime = 3f;
    public int maxLines = 10;

    private InputAction skipAction;
    private InputAction skipAllAction;
    private InputAction leftClickAction;

    public UnityEvent OnFinishWriting;

    [SerializeField]
    [TextArea]
    public string fullText;

    private bool skip = false;
    public static bool skipAll = false;
    private bool newLine = false;
    private bool skipped = false;


    void Awake()
    {
        skipAction = InputSystem.actions.FindAction("Skip");
        skipAllAction = InputSystem.actions.FindAction("SkipAll");
        leftClickAction = InputSystem.actions.FindAction("LeftClick");
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
        if (skipAction.WasPressedThisFrame() || leftClickAction.WasPressedThisFrame()) skip = true;
        if (skipAllAction.WasPressedThisFrame())  skipAll = true;
    }
    public void StartTutorial()
    {
        TutorialSystem.startTutorial = true; ;
    }
    public void ResetSkipAll()
    {
        skipAll = false;
    }

    IEnumerator ShowText()
    {
        int index = 0;
        int length = fullText.Length;

        while (index < length)
        {
            if (skipAll)
            {
                textComponent.text = fullText.Replace("|", "");
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
                skipped = true;
                int nextNewline = fullText.IndexOf('|', index);
                if (nextNewline >= 0)
                {
                    textComponent.text += fullText.Substring(index, nextNewline - index + 1).Replace("|", "");
                    index = nextNewline + 1;
                    newLine = true;
                    yield return new WaitForSeconds(timePerPause/2);
                    continue;
                }
                textComponent.text = fullText.Replace("|", ""); ;
                yield return new WaitForSeconds(timePerPause);
                OnFinishWriting?.Invoke();
                yield break;
            }


            char c = fullText[index++];
            newLine = false;
            if (c == '|')
            {
                if (skipped)
                {
                    yield return new WaitForSeconds(timePerCharacter);
                }
                else
                {
                    newLine = true;
                    yield return new WaitForSeconds(timePerPause);
                }
                
            }
            else
            {
                skipped = false;
                textComponent.text += c;
                yield return new WaitForSeconds(timePerCharacter);
            }
        }

        // Completed writing
        yield return new WaitForSeconds(endTime);
        OnFinishWriting?.Invoke();
    }

    public void Restart()
    {
        StopAllCoroutines();
        textComponent.text = string.Empty;
        StartCoroutine(ShowText());
    }
}
