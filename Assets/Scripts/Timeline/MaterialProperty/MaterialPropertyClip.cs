
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;


[System.Serializable]
public class MaterialPropertyClip : PlayableAsset//, ITimelineClipAsset
{
    public Material material;

    // Base Map
    public Color targetBaseMapColor;

    // Emission
    // public Color targetEmissionColor;

    public float startTime;
    public float endTime;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<MaterialPropertyBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();
        behaviour.material = material;
        behaviour.targetBaseMapColor = targetBaseMapColor;
        // behaviour.targetEmissionColor = targetEmissionColor;
        behaviour.startTime = startTime;
        behaviour.endTime = endTime;

        return playable;
    }

    // このクリップがサポートする機能
    // public ClipCaps clipCaps
    // {
    //     get { return ClipCaps.All; }
    // }
}
