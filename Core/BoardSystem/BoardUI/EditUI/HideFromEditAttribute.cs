using System;

namespace Parterraria.Core.BoardSystem.BoardUI.EditUI;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
internal class HideFromEditAttribute : Attribute
{
}
