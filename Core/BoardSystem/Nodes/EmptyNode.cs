namespace Parterraria.Core.BoardSystem.Nodes;

public class EmptyNode() : BoardNode
{
    public override MinigameReferral Referral => MinigameReferral.Any;
}
