using System;
using System.IO;
using Terraria.ModLoader.IO;

namespace Parterraria.Common;

internal class NetUtils
{
    /// <summary>
    /// Creates a <see cref="BinaryWriter"/> and returns the resultant bytes.
    /// </summary>
    public static byte[] WriteAsBytes(Action<BinaryWriter> writerAction)
    {
        using MemoryStream mem = new();
        using BinaryWriter writer = new(mem);
        writerAction(writer);
        writer.Flush();
        mem.Position = 0;
        return mem.ReadBytes(mem.Length);
    }
}
