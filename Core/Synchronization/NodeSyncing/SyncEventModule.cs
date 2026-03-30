using NetEasy;
using Parterraria.Core.BoardSystem.Events;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization.BoardItemSyncing;

/// <summary>
/// Runs a <see cref="Microevent"/> on all clients + server.
/// </summary>
/// <param name="index"></param>
/// <param name="fromWho"></param>
[Serializable]
public class SyncEventModule(int index, int fromWho) : Module
{
    private readonly int _index = index;
    private readonly int _fromWho = fromWho;

    protected override void Receive()
    {
        Microevent.Microevents[_index].Invoke(Main.player[_fromWho]);

        if (Main.netMode == NetmodeID.Server)
            Send(-1, -1, false);
    }
}
