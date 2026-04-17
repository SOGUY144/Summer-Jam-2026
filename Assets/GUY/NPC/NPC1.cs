using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC1 : MonoBehaviour, IInteractable
{
    
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;
    public AudioSource audioSource;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    private bool isPlayerInRange;

    private void Update()
    {
        // When the player presses 'F', trigger interaction
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isPlayerInRange || isDialogueActive)
            {
                Interact();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            
            // Dialogue will start IMMEDIATELY when walking close
            if (!isDialogueActive)
            {
                Interact();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (isDialogueActive)
            {
                EndDialogue();
            }
        }
    }

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        // If no dialogue data or the game is paused and no dialogue is active
        if (dialogueData == null || (PauseController.IsGamePaused && !isDialogueActive))
            return;

        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }
    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.SetText(dialogueData.npcName);
        portraitImage.sprite = dialogueData.npcPortrait;

        dialoguePanel.SetActive(true);
        PauseController.SetPause(true);

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;

        }
        else if (dialogueIndex + 1 < dialogueData.dialogueLines.Length)
        {
            dialogueIndex++;
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }
    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");
        
        foreach(char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueText.text += letter;
            
            // Play Animal Crossing style voice bleeps (ignore spaces)
            if (letter != ' ' && dialogueData.voiceSound != null && audioSource != null)
            {
                audioSource.Stop(); // Stop previous letter sound from overlapping and getting too loud
                audioSource.pitch = dialogueData.voicePitch + Random.Range(-0.05f, 0.05f);
                audioSource.PlayOneShot(dialogueData.voiceSound);
            }

            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);

        }
        
        isTyping = false;

        if(dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
            NextLine();
        }
    }
    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);
        PauseController.SetPause(false);    
    }
}
