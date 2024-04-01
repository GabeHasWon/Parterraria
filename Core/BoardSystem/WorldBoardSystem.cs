using Parterraria.Core.BoardSystem.BoardUI;
using System;
using System.Collections.Generic;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.UI;

namespace Parterraria.Core.BoardSystem;

internal class WorldBoardSystem : ModSystem
{
    public static WorldBoardSystem Self => ModContent.GetInstance<WorldBoardSystem>();

    public Dictionary<string, Board> worldBoards = [];

    private UserInterface _toolUI = null;
    private UserInterface _keyboardUI = null;

    public override void Load()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            _toolUI = new UserInterface();
            _toolUI.SetState(null);

            _keyboardUI = new UserInterface();
            _keyboardUI.SetState(null);
        }
    }

    public override void ClearWorld()
    {
        worldBoards.Clear();
        worldBoards = [];

        _toolUI.SetState(null);
        _keyboardUI.SetState(null);
    }

    internal string GetUnrepeatedKey(string value)
    {
        if (!worldBoards.ContainsKey(value))
            return value;

        int copy = 1;

        while (worldBoards.ContainsKey(value + copy))
            copy++;

        return value + copy;
    }

    // UI stuff

    public static bool ToolUIOpen() => Self._toolUI.CurrentState is ToolUIState;
    public static void OpenToolUI() => Self._toolUI.SetState(new ToolUIState(Main.LocalPlayer));
    internal static void CloseToolUI() => Self._toolUI.SetState(null);

    public static void OpenKeyboard(UIVirtualKeyboard.KeyboardSubmitEvent submitEvent, Action cancelAction) 
        => Self._keyboardUI.SetState(new UIVirtualKeyboard("Enter Board name", "Party", submitEvent, cancelAction, 0));
    public static void CloseKeyboard() => Self._keyboardUI.SetState(null);

    public override void UpdateUI(GameTime gameTime)
    {
        if (_keyboardUI.CurrentState is not null && Main.playerInventory)
            _keyboardUI.SetState(null);

        _toolUI.Update(gameTime);
        _keyboardUI.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));

        if (resourceBarIndex != -1)
        {
            layers.Insert(resourceBarIndex - 1, new LegacyGameInterfaceLayer(
                "Parterraria: Board",
                DrawBoard,
                InterfaceScaleType.UI)
            );

            layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                "Parterraria: Tool UI",
                delegate
                {
                    _toolUI.Draw(Main.spriteBatch, Main.gameTimeCache);
                    return true;
                },
                InterfaceScaleType.UI)
            );

            layers.Insert(resourceBarIndex + 2, new LegacyGameInterfaceLayer(
                "Parterraria: Keyboard UI",
                delegate
                {
                    _keyboardUI.Draw(Main.spriteBatch, Main.gameTimeCache);
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }

    public static bool DrawBoard()
    {
        foreach (var item in Self.worldBoards.Values)
            item.Draw();

        ToolUsage.DrawBuilding();
        return true;
    }

    // Board stuff

    public static Board GetBoard(string key) => Self.worldBoards[key];
}
