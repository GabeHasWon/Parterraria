using Terraria.DataStructures;

namespace Parterraria.Core.BoardSystem;

internal class EntitySource_Board(Board board, string context = null) : IEntitySource
{
    private readonly string _context = context;
    private readonly Board _board = board;

    public string Context => _context;
    public Board Board => _board;
}
