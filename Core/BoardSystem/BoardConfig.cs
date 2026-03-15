using Parterraria.Core.BoardSystem.BoardUI.EditUI;
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

    [HideFromEdit]
    public WinType Win = WinType.TurnCount;

    public ushort MaxMoveTimerInSeconds = 10;
    public ushort CoreCost = 20;
    public string[] DisallowedMinigames = [];
    public byte CoinDeltaFromNodes = 3;
    public byte CoinDeltaFromGames = 3;
    public byte TurnMax = 15;
    public Point WinIdlePosition = Point.Zero;
    public Point ThirdPlacePosition = Point.Zero;
    public Point SecondPlacePosition = Point.Zero;
    public Point FirstPlacePosition = Point.Zero;

    public void Save(TagCompound tag)
    {
        tag.Add(nameof(Win), (byte)Win);
        tag.Add(nameof(MaxMoveTimerInSeconds), MaxMoveTimerInSeconds);
        tag.Add(nameof(CoreCost), CoreCost);
        tag.Add(nameof(CoinDeltaFromNodes), CoinDeltaFromNodes);
        tag.Add(nameof(CoinDeltaFromGames), CoinDeltaFromGames);
        tag.Add(nameof(WinIdlePosition), WinIdlePosition);
        tag.Add(nameof(ThirdPlacePosition), ThirdPlacePosition);
        tag.Add(nameof(SecondPlacePosition), SecondPlacePosition);
        tag.Add(nameof(FirstPlacePosition), FirstPlacePosition);
        tag.Add(nameof(TurnMax), TurnMax);
    }

    public static BoardConfig Load(TagCompound tag)
    {
        BoardConfig config = new();
        config.Win = (WinType)tag.GetByte(nameof(Win));
        config.MaxMoveTimerInSeconds = (ushort)tag.GetShort(nameof(MaxMoveTimerInSeconds));
        config.CoreCost = (ushort)tag.GetShort(nameof(CoreCost));
        config.CoinDeltaFromNodes = tag.GetByte(nameof(CoinDeltaFromNodes));
        config.CoinDeltaFromGames = tag.GetByte(nameof(CoinDeltaFromGames));
        config.WinIdlePosition = tag.Get<Point>(nameof(WinIdlePosition));
        config.ThirdPlacePosition = tag.Get<Point>(nameof(ThirdPlacePosition));
        config.SecondPlacePosition = tag.Get<Point>(nameof(SecondPlacePosition));
        config.FirstPlacePosition = tag.Get<Point>(nameof(FirstPlacePosition));
        config.TurnMax = tag.GetByte(nameof(TurnMax));
        return config;
    }
}
