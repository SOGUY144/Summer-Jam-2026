using UnityEngine;

public class NPCStatemachineController : MonoBehaviour
{
    public Animator animator;
    public RuntimeAnimatorController unfreshController;
    public RuntimeAnimatorController refreshController;
    // The Enum defining the states
    public enum NPCState
    {
        Unfresh,
        Refresh
    }

    public NPCState currentState;

    void Start()
    {
        currentState = NPCState.Unfresh;
    }

    void Update()
    {
        // The State Machine Switch
        switch (currentState)
        {
            case NPCState.Unfresh:
                HandleUnfreshState();
                break;

            case NPCState.Refresh:
                HandleRefreshState();
                break;
        }
    }

    private void HandleUnfreshState()
    {
        animator.runtimeAnimatorController = unfreshController;
        //when unfresh
    }

    private void HandleRefreshState()
    {

        animator.runtimeAnimatorController = refreshController;
        //when fresh
    }
}