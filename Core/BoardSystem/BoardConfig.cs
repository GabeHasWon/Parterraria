using System;

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
    public int MaxMoveTimer = 60 * 10;
    public string[] DisallowedMinigames = [];
    public int CoinDeltaFromNodes = 3;
}
