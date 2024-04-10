using NetEasy;
using Parterraria.Core.BoardSystem;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization;

[Serializable]
public class SyncEndPartyModule() : Module
{
    protected override void Receive()
    {
        WorldBoardSystem.StopParty();

        if (Main.netMode == NetmodeID.Server)
            Send(-1, -1, false);
    }
}
