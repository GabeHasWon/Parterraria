using NetEasy;
using Parterraria.Core.MinigameSystem;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization.MinigameSyncing;

[Serializable]
public class RemoveMinigameModule(short netId) : Module
{
    private readonly short _netId = netId;

    protected override void Receive()
    {
        if (Main.netMode == NetmodeID.Server)
            Send(-1, -1, false);
     
        WorldMinigameSystem.worldMinigamesByNetId.Remove(_netId);
    }
}
