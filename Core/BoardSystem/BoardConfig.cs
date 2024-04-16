using System;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.BoardSystem;

[Serializable]
public class BoardConfig
{
    [Serializable]
    public enum WinType : byte
    {
        TurnCount,
        CoreCount,
    }

    public WinType Win = WinType.TurnCount;
    public ushort MaxMoveTimerInSeconds = 10;
    public ushort CoreCost = 20;
    public string[] DisallowedMinigames = [];
    public byte CoinDeltaFromNodes = 3;
    public byte CoinDeltaFromGames = 3;

    public void Save(TagCompound tag)
    {
        tag.Add(nameof(Win), (byte)Win);
        tag.Add(nameof(MaxMoveTimerInSeconds), MaxMoveTimerInSeconds);
        tag.Add(nameof(CoreCost), CoreCost);
        tag.Add(nameof(CoinDeltaFromNodes), CoinDeltaFromNodes);
        tag.Add(nameof(CoinDeltaFromGames), CoinDeltaFromGames);
    }

    public static BoardConfig Load(TagCompound tag)
    {
        BoardConfig config = new();
        config.Win = (WinType)tag.GetByte(nameof(Win));
        config.MaxMoveTimerInSeconds = (ushort)tag.GetShort(nameof(MaxMoveTimerInSeconds));
        config.CoreCost = (ushort)tag.GetShort(nameof(CoreCost));
        config.CoinDeltaFromNodes = tag.GetByte(nameof(CoinDeltaFromNodes));
        config.CoinDeltaFromGames = tag.GetByte(nameof(CoinDeltaFromGames));
        return config;
    }
}
