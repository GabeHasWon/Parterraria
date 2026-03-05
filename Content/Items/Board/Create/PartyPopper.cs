using Parterraria.Common;
using Parterraria.Core.BoardSystem;
using Parterraria.Core.BoardSystem.BoardUI;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.Synchronization;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;

namespace Parterraria.Content.Items.Board.Create;

class PartyPopper : ModItem
{
    internal string selectedBoard = "";

    private bool promptedInvalidBoard = false;

    public override void SetDefaults()
    {
        Item.Size = new(30);
        Item.noMelee = false;
        Item.useTurn = true;
        Item.useTime = 8;
        Item.useAnimation = 8;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.rare = ItemRarityID.Blue;
    }

    internal bool TryStartBoard(Player player)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient && !Main.countsAsHostForGameplay[player.whoAmI])
        {
            Main.NewText(ToolUIState.Text("FailedPerms").Value, CommonColors.Error);
            return false;
        }

        if (selectedBoard == string.Empty)
        {
            Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolInfo.Board.NoBoard"), CommonColors.Error);
            return false;
        }

        if (!promptedInvalidBoard && ToolUIState.CheckInvalidBoard(selectedBoard, out _, out _))
        {
            Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolUI.InvalidPlay"), CommonColors.Error);
            promptedInvalidBoard = true;
            return false;
        }

        if (WorldBoardSystem.CanPlayParty(selectedBoard, out string denialKey))
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                new SyncStartPartyModule(Main.myPlayer, selectedBoard).Send(-1, -1, false);
            else
            {
                WorldBoardSystem.Self.boardHost = Main.myPlayer;
                WorldBoardSystem.PlayParty(selectedBoard);
                BoardUISystem.CloseToolUI();
                BoardUISystem.CheckCloseMiscUI<EditObjectUIState>();
            }
        }
        else
            Main.NewText(Language.GetTextValue(denialKey), CommonColors.Error);
        return true;
    }

    public override bool ConsumeItem(Player player) => false;
    public override bool AltFunctionUse(Player player) => true;

    public override bool? UseItem(Player player)
    {
        if (player.altFunctionUse != 2)
            return false;

        if (!BoardUISystem.ToolUIOpen())
            BoardUISystem.OpenToolUI(null, this);
        else
            BoardUISystem.CloseToolUI();

        return true;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (tooltips.FirstOrDefault(x => x.Name == "ItemName") is { } nameTip && selectedBoard != string.Empty)
        {
            nameTip.Text += $" ({selectedBoard})";
        }
    }
}
