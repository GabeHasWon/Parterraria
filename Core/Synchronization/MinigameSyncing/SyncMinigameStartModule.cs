using NetEasy;
using Parterraria.Core.BoardSystem;
using Parterraria.Core.MinigameSystem;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization.MinigameSyncing;

[Serializable]
public class SyncMinigameStartModule(string minigameName, int slot) : Module
{
    private readonly string _minigameName = minigameName;
    private readonly int _slot = slot;

    protected override void Receive()
    {
        WorldMinigameSystem.Self.StartMinigame(_minigameName, _slot);

        if (Main.netMode != NetmodeID.Server)
            BoardUISystem.CloseMiscUI();
    }
}
