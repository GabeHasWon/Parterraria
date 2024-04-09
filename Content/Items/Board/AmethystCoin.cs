using Terraria.ID;

namespace Parterraria.Content.Items.Board;

class AmethystCoin : ModItem, IBoardClearItem
{
    public override void SetDefaults()
    {
        Item.Size = new(30);
        Item.rare = ItemRarityID.Purple;
        Item.maxStack = Item.CommonMaxStack;
    }
}
