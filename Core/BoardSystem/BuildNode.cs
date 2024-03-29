using Parterraria.Core.BoardSystem.Nodes;

namespace Parterraria.Core.BoardSystem;

internal class BuildNode(string nodeType) : BoardNode
{
    public readonly string nodeType = nodeType;

    public bool settingRadius = false;

    public BuildNode() : this("")
    {
    }

    public override void LandOn(Board board, Player player) => throw null;
}
