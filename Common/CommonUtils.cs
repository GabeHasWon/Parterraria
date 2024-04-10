using Terraria.DataStructures;

namespace Parterraria.Common;

internal static class CommonUtils
{
    public static void AddItemToInvOrSpawnIfOverfull(Player player, int type, int count)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            for (int i = 0; i < player.inventory.Length; ++i)
            {
                Item item = player.inventory[i];

                if (item.IsAir)
                {
                    item.SetDefaults(type);
                    item.stack = count;
                    return;
                }
            }

            player.QuickSpawnItem(new EntitySource_OverfullInventory(player), type, count);
        }
    }
}
