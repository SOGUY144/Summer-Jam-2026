using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NPC : MonoBehaviour //Dialogue , Press E to make NPC Refresh state in NPCStatemachineController ,Data for Dialogue
{
    public NPCStatemachineController npcStateMachineController;
    public NPCData npcData; // Reference to the NPCData ScriptableObject            '
    public TextMeshProUGUI text;
    public bool isPlayerInRange;
    public GameObject dialoguePanel;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            text.text = ""; // Clear dialogue when player leaves
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isPlayerInRange)
        {
            npcStateMachineController.currentState = NPCStatemachineController.NPCState.Refresh;
            
            StartCoroutine(ShowDialogue());

        }
    }

    
    private IEnumerator ShowDialogue()
    {
        text.text = "";
        dialoguePanel.SetActive(true);
        foreach (char letter in npcData.dialogue.ToCharArray())
        {
            text.text += letter;
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(1f); // Wait for 2 seconds after the dialogue is fully displayed
        dialoguePanel.SetActive(false);

    }
}

[CreateAssetMenu(fileName = "NPCData", menuName = "ScriptableObjects/NPCData", order = 1)]
public class NPCData : ScriptableObject
{
    public string npcName;
    public string dialogue;
    // Add more fields as needed for your NPC data
}
