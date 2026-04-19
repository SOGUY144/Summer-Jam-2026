using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.8623f, 0.870f)]
[TrackClipType(typeof(DialogueClip))]
[TrackBindingType(typeof(CutsceneSpeechBubble))]
public class DialogueTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<DialogueTrackMixer>.Create(graph, inputCount);
    }
}

// Ensure the mixer class is defined in the project
public class DialogueTrackMixer : PlayableBehaviour
{
    private CutsceneSpeechBubble trackBinding;
    private int currentInput = -1; // To track which clip is currently active

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        trackBinding = playerData as CutsceneSpeechBubble;

        if (trackBinding == null)
            return;

        int activeInput = -1;
        int inputCount = playable.GetInputCount();

        // Loop through all clips to see which one is currently playing
        for (int i = 0; i < inputCount; i++)
        {
            if (playable.GetInputWeight(i) > 0f)
            {
                activeInput = i;
                break;
            }
        }

        // If the active clip changed (either entering a new clip, or exiting all)
        if (activeInput != currentInput)
        {
            // If we are currently inside a clip
            if (activeInput != -1)
            {
                ScriptPlayable<DialogueBehaviour> scriptPlayable = (ScriptPlayable<DialogueBehaviour>)playable.GetInput(activeInput);
                DialogueBehaviour input = scriptPlayable.GetBehaviour();
                
                trackBinding.StartDialogue(input.dialogueData, input.index);
            }
            // If we exited all clips
            else
            {
                trackBinding.StopDialogue();
            }

            currentInput = activeInput;
        }
    }
}
