using NetEasy;
using Parterraria.Core.BoardSystem;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization.BoardItemSyncing;

[Serializable]
public class SyncDieCount(int fromWho, int dieCount) : Module
{
    private readonly int _fromWho = fromWho;
    private readonly int _dieCount = dieCount;

    protected override void Receive()
    {
         Main.player[_fromWho].GetModPlayer<PlayingBoardPlayer>().diceCount = _dieCount;

        if (Main.netMode == NetmodeID.Server)
            Send(-1, -1, false);
    }
}
