using NetEasy;
using Parterraria.Core.BoardSystem;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization;

[Serializable]
public class RemoveNodeModule(string name, short id) : Module
{
    private readonly string _name = name;
    private readonly short _id = id;

    protected override void Receive()
    {
        var board = WorldBoardSystem.GetBoard(_name);
        var node = board.nodes[_id];

        if (Main.netMode == NetmodeID.Server)
        {
            new RemoveNodeModule(_name, (short)board.nodes.IndexOf(node)).Send(runLocally: false);
        }

        board.RemoveNode(node);
    }
}

