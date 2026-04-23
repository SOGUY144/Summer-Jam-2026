using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueSequence : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;

    [System.Serializable]
    public class DialogueLine
    {
        public string text;
        public float duration = 3f;
    }

    public DialogueLine[] lines;

    public void StartDialogue()
    {
        StartCoroutine(PlayDialogue());
    }

    IEnumerator PlayDialogue()
    {
        foreach (DialogueLine line in lines)
        {
            dialogueText.text = line.text;
            yield return new WaitForSeconds(line.duration);
        }
    }
}