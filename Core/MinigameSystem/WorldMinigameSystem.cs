using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;

namespace Parterraria.Core.MinigameSystem;

internal class WorldMinigameSystem : ModSystem
{
    internal static readonly List<Minigame> worldMinigames = [];

    public static bool TryAddMinigame(string name, Rectangle rectangle)
    {
        Minigame.MinigamesByModAndName[name].ValidateRectangle(ref rectangle);

        if (worldMinigames.Any(x => x.area.Intersects(rectangle)))
        {
            Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolInfo.Minigame.Intersecting"));
            return false;
        }

        var game = Minigame.MinigamesByModAndName[name].Clone();
        game.area = rectangle;
        worldMinigames.Add(game);
        return true;
    }
}
