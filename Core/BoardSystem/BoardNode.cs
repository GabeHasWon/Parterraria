namespace Parterraria.Core.BoardSystem;

internal abstract class BoardNode
{
    public Vector2 position;

    public abstract void LandOn(Board board, Player player);
    public abstract BoardNode NextNode(Board board);
    public abstract BoardNode PriorNode(Board board);
}
