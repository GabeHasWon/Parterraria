using System.Collections.Generic;

namespace Parterraria.Core.BoardSystem;

public class Board
{
    public readonly List<BoardNode> nodes = [];

    public void Draw() 
    {
        foreach (var node in nodes)
            node.Draw();
    }
}
