using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class PrizePlayableAsset : PlayableAsset
{
    public string prizeName;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<PrizePlayableBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        // Pass the prize name
        behaviour.prizeName = prizeName;

        // Get the OpenPrizes component from the PlayableDirector's GameObject
        behaviour.prizeHandler = owner.GetComponent<OpenPrizes>();

        return playable;
    }
}
