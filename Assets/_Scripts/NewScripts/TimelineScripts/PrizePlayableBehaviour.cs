using UnityEngine;
using UnityEngine.Playables;

public class PrizePlayableBehaviour : PlayableBehaviour
{
    public string prizeName;
    public OpenPrizes prizeHandler;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (prizeHandler != null)
        {
            prizeHandler.OpenPrize(prizeName);
        }
        else
        {
            Debug.LogWarning("Prize handler is missing!");
        }
    }
}
