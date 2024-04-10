using Terraria.ID;

namespace Parterraria.Core.MinigameSystem.Games;

internal class CraftLargeDiamondGame : Minigame
{
    public override bool ValidateRectangle(ref Rectangle rectangle)
    {
        bool modified = false;

        if (rectangle.Width < 60 * 16)
        {
            rectangle.Width = 60 * 16;
            modified = true;
        }

        if (rectangle.Height < 30 * 16)
        {
            rectangle.Height = 30 * 16;
            modified = true;
        }

        return modified;
    }

    public override void InternalUpdate()
    {
        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (plr.active && !plr.dead && (plr.HasItem(ItemID.LargeDiamond) || plr.HeldItem.type == ItemID.LargeDiamond))
            {
                Beaten = true;
                break;
            }    
        }
    }
}
