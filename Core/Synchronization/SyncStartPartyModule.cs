using NetEasy;
using Parterraria.Core.BoardSystem;
using Parterraria.Core.BoardSystem.BoardUI;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization;

[Serializable]
public class SyncStartPartyModule(int fromWho, string boardKey) : Module
{
    private readonly int _fromWho = fromWho;
    private readonly string _boardKey = boardKey;

    protected override void Receive()
    {
        Main.NewText($"Starting party \"{_boardKey}\" from {Main.player[_fromWho].name}!");
        WorldBoardSystem.PlayParty(_boardKey);

        if (Main.netMode == NetmodeID.Server)
            Send(-1, -1, false);
        else
        {
            WorldBoardSystem.CloseToolUI();
            WorldBoardSystem.CheckCloseMiscUI<EditObjectUIState>();
        }
    }
}
