using Parterraria.Core.MinigameSystem;
using Terraria.DataStructures;

namespace Parterraria.Core.BoardSystem;

internal class EntitySource_Board(Board board, string context = null) : IEntitySource
{
    public string Context { get; } = context;
    public Board Board { get; } = board;
}

internal class EntitySource_Minigame(Board board, Minigame minigame, string context = null) : EntitySource_Board(board, context)
{
    public Minigame Minigame { get; } = minigame;
}
