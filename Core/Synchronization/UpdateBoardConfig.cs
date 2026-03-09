using NetEasy;
using Parterraria.Core.BoardSystem;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization;

[Serializable]
public class UpdateBoardConfig(BoardConfig config, string boardKey, int fromPlayer) : Module
{
    private readonly BoardConfig config = config;
    private readonly string boardKey = boardKey;
    private readonly int fromPlayer = fromPlayer;

    protected override void Receive()
    {
        if (Main.netMode == NetmodeID.Server)
            Send(-1, fromPlayer);

        if (!WorldBoardSystem.Self.worldBoards.TryGetValue(boardKey, out Board board))
            throw new ArgumentException("Board " + boardKey + " not found?");

        board.config = config;
    }
}
