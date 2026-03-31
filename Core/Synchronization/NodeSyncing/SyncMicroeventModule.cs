using NetEasy;
using Parterraria.Core.BoardSystem.Events;
using System;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.Synchronization.NodeSyncing;

/// <summary>
/// Runs a <see cref="Microevent"/> on all clients + server.
/// </summary>
/// <param name="index"></param>
/// <param name="fromWho"></param>
[Serializable]
public class SyncMicroeventModule(int index, int fromWho, byte[] info) : Module
{
    private readonly int _index = index;
    private readonly int _fromWho = fromWho;

    protected override void Receive()
    {
        if (Main.netMode == NetmodeID.Server)
            Send(-1, -1, false);

        using var stream = info.ToMemoryStream();
        using var reader = new BinaryReader(stream);

        Microevent.Microevents[_index].Invoke(Main.player[_fromWho], reader);
    }
}
