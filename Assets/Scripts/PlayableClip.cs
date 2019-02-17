using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class PlayableClip : MonoBehaviour
{
    public AnimationClip clip;
    public double speed = 1.0;

    public PlayableGraph playableGraph;
    AnimationClipPlayable clipPlayable;

    void Awake()
    {
        playableGraph = PlayableGraph.Create();
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

        clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);

        playableOutput.SetSourcePlayable(clipPlayable);
        
        playableGraph.Play();
        speed = 0.0f;
    }

    void Update()
    {
        clipPlayable.SetSpeed(speed);
    }

    void OnDisable()
    {
        // Destroys all Playables and PlayableOutputs created by the graph.
        playableGraph.Destroy();
    }

}
