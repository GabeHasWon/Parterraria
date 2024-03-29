using ReLogic.Content;

namespace Parterraria.Core.BoardSystem;

internal abstract class BoardNode
{
    public Vector2 position;
    public float radius;

    public static Asset<Texture2D> Tex(BoardNode node) => Tex(node.GetType().Name.Replace("Node", ""));
    public static Asset<Texture2D> Tex(string node) => ModContent.Request<Texture2D>("Parterraria/Assets/Textures/Nodes/" + node.Replace("Node", ""));

    public abstract void LandOn(Board board, Player player);
    public abstract BoardNode NextNode(Board board);
    public abstract BoardNode PriorNode(Board board);
}
