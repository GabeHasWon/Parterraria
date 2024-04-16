using NetEasy;
using Parterraria.Core.MinigameSystem;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization;

[Serializable]
public class SyncMinigameModule(string type, Rectangle area) : Module
{
    private readonly string _type = type;
    private readonly Rectangle _area = area;

    protected override void Receive()
    {
        if (Main.netMode == NetmodeID.Server)
            return;
        else
            WorldMinigameSystem.TryAddMinigame(_type, _area);
    }
}
