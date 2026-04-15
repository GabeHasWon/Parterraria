using NetEasy;
using Parterraria.Core.MinigameSystem;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization;

[Serializable]
public class SyncMinigameDisabledModule(byte who, bool enabled) : Module
{
    private readonly byte _who = who;
    private readonly bool _enabled = enabled;

    protected override void Receive()
    {
        if (Main.netMode == NetmodeID.Server)
            new SyncMinigameDisabledModule(_who, _enabled).Send(runLocally: false);

        MinigameDisablePlayer plr = Main.player[_who].GetModPlayer<MinigameDisablePlayer>();

        if (_enabled)
            plr.Enable(true);
        else
            plr.Disable(true);
    }
}

