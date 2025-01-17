using System;
using Terraria.ID;

namespace Parterraria.Content.Items.Board;

class CelestialCore : ModItem, IBoardClearItem
{
    private int timer = 0;

    public override void SetDefaults()
    {
        Item.Size = new(42);
        Item.rare = ItemRarityID.Purple;
        Item.maxStack = Item.CommonMaxStack;
    }

    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        gravity = MathF.Sin(timer++ * 0.2f) * 0.1f + 0.2f;
        maxFallSpeed = 3;
    }
}
