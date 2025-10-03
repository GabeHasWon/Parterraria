using NetEasy;
using Parterraria.Core.BoardSystem;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization.BoardItemSyncing;

/// <summary>
/// Forces a reset of a player's stored roll and dice count. This accounts for potential issues with returning players.
/// </summary>
[Serializable]
public class ForceResetInformation(byte who) : Module
{
    private readonly byte _fromWho = who;

    protected override void Receive()
    {
        Main.player[_fromWho].GetModPlayer<PlayingBoardPlayer>().storedRoll = 0;
        Main.player[_fromWho].GetModPlayer<PlayingBoardPlayer>().diceCount = 0;

        if (Main.netMode == NetmodeID.Server)
            Send(-1, _fromWho, false);
    }
}
 