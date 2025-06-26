using NetEasy;
using Parterraria.Core.BoardSystem;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization;

[Serializable]
public class AddNodeModule(string name, string typeName, Vector2 position, float halfWidth) : Module
{
    private readonly string _name = name;
    private readonly string _typeName = typeName;
    private readonly Vector2 _position = position;
    private readonly float _halfWidth = halfWidth;

    protected override void Receive()
    {
        if (Main.netMode == NetmodeID.Server)
        {
            Send(runLocally: false);
        }

        var board = WorldBoardSystem.GetBoard(_name);
        board.AddNode(BoardNode.GenerateNode(board, Type.GetType(_typeName), _position, _halfWidth));
    }
}

