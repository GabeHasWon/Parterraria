using Terraria.ID;

namespace Parterraria.Core.BoardSystem.Nodes;

internal class EmptyNode(BoardNode next, BoardNode prior) : BoardNode
{
    readonly BoardNode _next = next;
    readonly BoardNode _prior = prior;

    public override void LandOn(Board board, Player player) { }
    public override BoardNode NextNode(Board board) => _next;
    public override BoardNode PriorNode(Board board) => _prior;
}
