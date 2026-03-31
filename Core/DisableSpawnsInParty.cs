using Parterraria.Core.BoardSystem;

namespace Parterraria.Core;

internal class DisableSpawnsInParty : GlobalNPC
{
    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        if (!WorldBoardSystem.PlayingParty)
            return;

        spawnRate = int.MaxValue / 2;
        maxSpawns = 0;
    }
}
