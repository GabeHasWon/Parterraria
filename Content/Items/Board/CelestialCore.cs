using System;
using Terraria.DataStructures;
using Terraria.ID;

namespace Parterraria.Content.Items.Board;

class CelestialCore : ModItem, IBoardClearItem
{
    private int timer = 0;

    public override void SetStaticDefaults()
    {
        ItemID.Sets.AnimatesAsSoul[Type] = true;

        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(7, 6));
    }

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
