using System;

namespace Parterraria.Core.BoardSystem.BoardUI.EditUI;

internal class EditToolPlayer : ModPlayer
{
    public int placeDelay = 0;
    public object placingType = null;
    public Action<object> placeResult = null;

    public override void PreUpdateBuffs()
    {
        if (placeDelay-- > 0)
            return;

        bool leftClick = Main.mouseLeft && Main.mouseLeftRelease;

        if (leftClick && placingType is not null)
        {
            switch (placingType)
            {
                case Point:
                    placeResult.Invoke(Main.MouseWorld.ToTileCoordinates());
                    break;
            }

            placingType = null;
            placeResult = null;
        }
    }
}
