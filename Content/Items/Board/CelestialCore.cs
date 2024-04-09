using Terraria.ID;

namespace Parterraria.Content.Items.Board;

class CelestialCore : ModItem
{
    public override void SetDefaults()
    {
        Item.Size = new(42);
        Item.rare = ItemRarityID.Purple;
        Item.maxStack = Item.CommonMaxStack;
    }
}
