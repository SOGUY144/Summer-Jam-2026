using System.Collections;
using TMPro;
using UnityEngine;

public class CutsceneSpeechBubble : MonoBehaviour
{
    public GameObject bubblePanel;
    public TMP_Text dialogueText;
    public AudioSource audioSource;

    private Coroutine typingCoroutine;

    // Call this from the Timeline when the clip starts
    public void StartDialogue(NPCDialogue data, int index)
    {
        if (data == null) return;

        bubblePanel.SetActive(true);
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeLine(data, index));
    }

    // Call this from the Timeline when the clip ends
    public void StopDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        dialogueText.SetText("");
        bubblePanel.SetActive(false);
    }

    IEnumerator TypeLine(NPCDialogue data, int index)
    {
        dialogueText.SetText("");
        
        if (data.dialogueLines.Length <= index || index < 0)
        {
            yield break;
        }

        foreach (char letter in data.dialogueLines[index])
        {
            dialogueText.text += letter;
            
            // Typewriter Animal Crossing style sound 
            // We only play sound if the character is not empty space
            if (letter != ' ' && data.voiceSound != null && audioSource != null)
            {
                audioSource.Stop();
                audioSource.pitch = data.voicePitch + Random.Range(-0.05f, 0.05f);
                audioSource.PlayOneShot(data.voiceSound);
            }

            yield return new WaitForSeconds(data.typingSpeed);
        }
    }
}
