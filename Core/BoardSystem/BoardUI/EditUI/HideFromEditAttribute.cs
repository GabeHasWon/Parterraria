using System;

namespace Parterraria.Core.BoardSystem.BoardUI.EditUI;

/// <summary>
/// Hides a member from editing tool.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
internal class HideFromEditAttribute : Attribute
{
}
