using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Parterraria.Core.WorldPresets;

internal class WorldPresetState(Action returnAction) : UIState
{
    private readonly Action _returnAction = returnAction;

    private UIText _description = null;

    public override void OnInitialize()
    {
        const int Buffer = 18;

        UIElement hiddenElement = new()
        {
            Width = StyleDimension.FromPixels(500f),
            Height = StyleDimension.FromPixels(484f + Buffer),
            Top = StyleDimension.FromPixels(170f - Buffer),
            HAlign = 0.5f,
            VAlign = 0f
        };
        hiddenElement.SetPadding(0f);
        Append(hiddenElement);

        UIPanel panel = new()
        {
            Width = StyleDimension.FromPercent(1f),
            Height = StyleDimension.FromPixels(460 + Buffer),
            Top = StyleDimension.FromPixels(50f),
            BackgroundColor = new Color(33, 43, 79) * 0.8f
        };
        panel.SetPadding(12);
        hiddenElement.Append(panel);

        UIButton<string> backButton = new("x")
        {
            Width = StyleDimension.FromPixels(32),
            Height = StyleDimension.FromPixels(32),
            HAlign = 1,
            VAlign = 0,
        };

        backButton.OnLeftClick += (_, _) => _returnAction();
        panel.Append(backButton);

        UIText title = new(Language.GetText("Mods.Parterraria.MiscUI.PresetWorlds.Title"), 0.8f, true);
        panel.Append(title);

        AddListUI(panel);

        _description = new UIText(Language.GetText("Mods.Parterraria.Presets.NoInfo"))
        {
            VAlign = 1,
            HAlign = 0.5f,
            Top = StyleDimension.FromPixels(-76)
        };

        panel.Append(_description);
    }

    private void AddListUI(UIPanel panel)
    {
        UIPanel listPanel = new()
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixelsAndPercent(-140, 1),
            Top = StyleDimension.FromPixels(36),
        };

        panel.Append(listPanel);

        UIList list = new()
        {
            Width = StyleDimension.FromPixelsAndPercent(-26, 1),
            Height = StyleDimension.Fill,
        };

        listPanel.Append(list);

        UIScrollbar bar = new()
        {
            Width = StyleDimension.FromPixels(22),
            Height = StyleDimension.Fill,
            HAlign = 1
        };

        list.SetScrollbar(bar);
        listPanel.Append(bar);

        foreach (WorldPreset preset in WorldPresetHooking.Presets)
        {
            UIWorldPreset presetElement = new(preset, SetDescription, OpenWorld)
            {
                Width = StyleDimension.Fill,
                Height = StyleDimension.FromPixels(50)
            };

            list.Add(presetElement);
        }
    }

    private void OpenWorld(WorldPreset preset)
    {
        byte[] bytes = preset.FetchFile();
        string path = Path.Combine(Main.SavePath, "Worlds", preset.Id);
        int id = 1;

        while (File.Exists(path))
        {
            id++;
            path = Path.Combine(Main.SavePath, "Worlds", preset.Id + " " + id);
        }

        using FileStream stream = File.Create(path, bytes.Length);

        stream.Write(bytes, 0, bytes.Length);
        stream.Flush();

        using (ZipArchive zip = new(stream))
        {
            zip.ExtractToDirectory(Path.Combine(Main.SavePath, "Worlds"));
        }

        stream.Close();

        File.Delete(path);
        Main.OpenWorldSelectUI();
    }

    private void SetDescription(WorldPreset preset)
    {
        string value = $"[c/FFAAAA:{preset.Name.Value}] by {preset.Authors}\n{preset.GetDisplayInfo().Value}\n{preset.Description.Value}";
        _description.SetText(value);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        _description.SetText(Language.GetText("Mods.Parterraria.Presets.NoInfo"));
    }
}
