using Terraria.DataStructures;

namespace Parterraria.Common;

internal static class CommonUtils
{
    public static void SafelyAddItemToInv<T>(Player player, int count) where T : ModItem => SafelyAddItemToInv(player, ModContent.ItemType<T>(), count);

    public static void SafelyAddItemToInv(Player player, int type, int count)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            if (!player.HasItem(type))
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
            }
            else
            {
                for (int i = 0; i < player.inventory.Length; ++i)
                {
                    Item item = player.inventory[i];

                    if (!item.IsAir && item.type == type)
                    {
                        item.stack += count;
                        return;
                    }
                }
            }

            player.QuickSpawnItem(new EntitySource_OverfullInventory(player), type, count);
        }
    }

    public static bool ConsumeItemFromInventory<T>(Player player, int count, bool consumeAlways = false) where T : ModItem 
        => ConsumeItemFromInventory(player, ModContent.ItemType<T>(), count, consumeAlways);

    public static bool ConsumeItemFromInventory(Player player, int type, int count, bool consumeAlways = false)
    {
        int itemCount = player.CountItem(type, count);

        if (itemCount > count)
            itemCount = count;

        if (!consumeAlways && itemCount < count)
            return false;

        for (int i = 0; i < itemCount; i++)
            player.ConsumeItem(type);

        return itemCount <= count;
    }
}
