using NetEasy;
using Parterraria.Core.BoardSystem;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization.BoardItemSyncing;

[Serializable]
public class SyncRolledDice(int fromWho, int roll) : Module
{
    private readonly int _fromWho = fromWho;
    private readonly int _roll = roll;

    protected override void Receive()
    {
        Main.player[_fromWho].GetModPlayer<PlayingBoardPlayer>().RolledDice(_roll);

        if (Main.netMode == NetmodeID.Server)
            Send(-1, _fromWho, false);
    }
}
 