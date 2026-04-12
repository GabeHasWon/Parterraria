using Terraria.DataStructures;
using Terraria.ID;

namespace Parterraria.Content.Items.MinigameItems;

internal class DuelingPistol : ModItem
{
    internal class DuelingPistolShot : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool FromDueling = false;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is EntitySource_ItemUse_WithAmmo { Item: Item item } && item.type == ModContent.ItemType<DuelingPistol>())
                FromDueling = true;
        }
    }

    public override void SetDefaults()
    {
        Item.DamageType = DamageClass.Ranged;
        Item.damage = 10;
        Item.shoot = ProjectileID.WoodenArrowFriendly;
        Item.useAmmo = AmmoID.Bullet;
        Item.shootSpeed = 16;
        Item.useTime = 180;
        Item.useAnimation = 180;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.autoReuse = false;
    }
}
