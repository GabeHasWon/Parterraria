using NetEasy;
using Parterraria.Core.BoardSystem;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization;

[Serializable]
public class RequestBoardsFromServerModule(int fromWho) : Module
{
    private readonly int _fromWho = fromWho;
    
    protected override void Receive()
    {
        if (Main.netMode == NetmodeID.Server)
        {
            foreach (var board in WorldBoardSystem.Self.worldBoards)
                new SyncBoardModule(board.Value.GetData(board.Key)).Send(_fromWho, -1, false);
        }
    }
}
