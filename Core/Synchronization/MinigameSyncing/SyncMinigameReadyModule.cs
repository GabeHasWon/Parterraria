using NetEasy;
using Parterraria.Core.BoardSystem;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization.MinigameSyncing;

[Serializable]
public class SyncMinigameReadyModule(int readyPlayer) : Module
{
    private readonly int _who = readyPlayer;

    protected override void Receive()
    {
        if (Main.netMode == NetmodeID.Server)
            Send(-1, _who);
        else
            Main.player[_who].GetModPlayer<PlayingBoardPlayer>().minigameReady = true;
    }
}
