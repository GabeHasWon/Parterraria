using NetEasy;
using Parterraria.Core.BoardSystem;
using Parterraria.Core.MinigameSystem;
using Parterraria.Core.MinigameSystem.MinigameUI;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization.MinigameSyncing;

[Serializable]
public class SyncMinigameRollUIModule(float timerSpeed, string[] minigames) : Module
{
    private readonly float _timerSpeed = timerSpeed;
    private readonly string[] _minigames = minigames;

    protected override void Receive()
    {
        if (Main.netMode == NetmodeID.Server)
            Send(-1, -1, false);
        else
            BoardUISystem.SetMiscUI(new MinigameSelectionUIState(ModContent.GetInstance<WorldMinigameSystem>().StartMinigame, _timerSpeed, _minigames));
    }
}
