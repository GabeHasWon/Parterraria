using Parterraria.Common;
using Parterraria.Content.Items.Board.PartyItems;

namespace Parterraria.Core.BoardSystem.Nodes;

public class ItemNode() : EmptyNode
{
    public override MinigameReferral Referral => MinigameReferral.Any;

    public override void LandOn(Board board, Player player)
    {
        int[] items = [ModContent.ItemType<HighDice>(), ModContent.ItemType<DoubleDice>(), ModContent.ItemType<PartyMirror>(), ModContent.ItemType<BrokenDice>(),
            ModContent.ItemType<LowDice>()];
        int type = Main.rand.Next(items);
        
        CommonUtils.SafelyAddItemToInv(player, type, 1);

        PopupText.NewText(new AdvancedPopupRequest()
        {
            Text = "Got item: " + Lang.GetItemNameValue(type),
            Color = Color.MediumPurple,
            DurationInFrames = 300,
            Velocity = new Vector2(0, -24)
        }, player.Center);
    }
}
