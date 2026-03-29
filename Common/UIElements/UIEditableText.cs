using ReLogic.Localization.IME;
using ReLogic.OS;
using System;
using System.Text.RegularExpressions;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace NewBeginnings.Common.UI;

#nullable enable

public enum InputType
{
    Text,
    Integer,
    Number
}

/// <summary>
/// Ported from DragonLens: https://github.com/ScalarVector1/DragonLens/blob/master/Content/GUI/FieldEditors/TextField.cs <br/>
/// Cleaned up and modified to not use DragonLens UI.
/// </summary>
internal partial class UIEditableText(InputType inputType = InputType.Text, string backingText = "", UIEditableText.EnterDelegate? onEnterAction = null, int maxChars = -1) : UIElement
{
    public delegate void EnterDelegate(UIEditableText text, ref string currentText);

    public readonly InputType InputType = inputType;

    public bool CurrentlyTyping { get; private set; }

    private readonly string _backingString = backingText;
    private readonly EnterDelegate? _onEnterAction = onEnterAction;
    private readonly int _maxChars = maxChars;

    public string currentValue = "";

    private bool _updated;
    private bool _reset;

    // Composition string is handled at the very beginning of the update
    // In order to check if there is a composition string before backspace is typed, we need to check the previous state
    private bool _oldHasCompositionString;

    public void SetTyping()
    {
        CurrentlyTyping = true;
        Main.blockInput = true;
    }

    public void SetNotTyping()
    {
        CurrentlyTyping = false;
        Main.blockInput = false;
    }

    public override void LeftClick(UIMouseEvent evt) => SetTyping();

    public override void RightClick(UIMouseEvent evt)
    {
        SetTyping();
        currentValue = "";
        _updated = true;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (_reset)
        {
            _updated = false;
            _reset = false;
        }

        if (_updated)
            _reset = true;

        if (Main.mouseLeft && !IsMouseHovering)
            SetNotTyping();
    }

    public void HandleText()
    {
        if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            SetNotTyping();

        if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter))
        {
            _onEnterAction?.Invoke(this, ref currentValue);

            Main.drawingPlayerChat = false; // Counteract vanilla functionality
            SetNotTyping();
        }

        PlayerInput.WritingText = true;
        Main.instance.HandleIME();

        string newText = Main.GetInputText(currentValue);

        // GetInputText() handles typing operation, but there is a issue that it doesn't handle backspace correctly when the composition string is not empty.
        // It will delete a character both in the text and the composition string instead of only the one in composition string.
        // We'll fix the issue here to provide a better user experience
        if (_oldHasCompositionString && Main.inputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Back))
            newText = currentValue; // force text not to be changed

        if (InputType == InputType.Integer && NumberRegex().IsMatch(newText))
        {
            if (newText != currentValue)
            {
                if (_maxChars != -1 && newText.Length > _maxChars)
                    newText = newText[.._maxChars];

                currentValue = newText;
                _updated = true;
            }
        }
        else if (InputType == InputType.Number && UnreadableRegex().IsMatch(newText)) //I found this regex on SO so no idea if it works right lol
        {
            if (newText != currentValue)
            {
                if (_maxChars != -1 && newText.Length > _maxChars)
                    newText = newText[.._maxChars];

                currentValue = newText;
                _updated = true;
            }
        }
        else
        {
            if (newText != currentValue)
            {
                if (_maxChars != -1 && newText.Length > _maxChars)
                    newText = newText[.._maxChars];

                currentValue = newText;
                _updated = true;
            }
        }

        _oldHasCompositionString = Platform.Get<IImeService>().CompositionString is { Length: > 0 };
    }

    private void DrawPanel(SpriteBatch spriteBatch, Texture2D texture, Color color)
    {
        const int _cornerSize = 12;
        const int _barSize = 4;

        CalculatedStyle dimensions = GetDimensions();
        Point point = new Point((int)dimensions.X, (int)dimensions.Y);
        Point point2 = new Point(point.X + (int)dimensions.Width - _cornerSize, point.Y + (int)dimensions.Height - _cornerSize);
        int width = point2.X - point.X - _cornerSize;
        int height = point2.Y - point.Y - _cornerSize;
        spriteBatch.Draw(texture, new Rectangle(point.X, point.Y, _cornerSize, _cornerSize), new Rectangle(0, 0, _cornerSize, _cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y, _cornerSize, _cornerSize), new Rectangle(_cornerSize + _barSize, 0, _cornerSize, _cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X, point2.Y, _cornerSize, _cornerSize), new Rectangle(0, _cornerSize + _barSize, _cornerSize, _cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point2.X, point2.Y, _cornerSize, _cornerSize), new Rectangle(_cornerSize + _barSize, _cornerSize + _barSize, _cornerSize, _cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X + _cornerSize, point.Y, width, _cornerSize), new Rectangle(_cornerSize, 0, _barSize, _cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X + _cornerSize, point2.Y, width, _cornerSize), new Rectangle(_cornerSize, _cornerSize + _barSize, _barSize, _cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X, point.Y + _cornerSize, _cornerSize, height), new Rectangle(0, _cornerSize, _cornerSize, _barSize), color);
        spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y + _cornerSize, _cornerSize, height), new Rectangle(_cornerSize + _barSize, _cornerSize, _cornerSize, _barSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X + _cornerSize, point.Y + _cornerSize, width, height), new Rectangle(_cornerSize, _cornerSize, _barSize, _barSize), color);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        DrawPanel(spriteBatch, Main.Assets.Request<Texture2D>("Images/UI/PanelBorder").Value, Color.Black);
        DrawPanel(spriteBatch, Main.Assets.Request<Texture2D>("Images/UI/PanelBackground").Value, new Color(63, 82, 151) * 0.7f);

        if (CurrentlyTyping)
        {
            HandleText();

            // Draw ime panel, note that if there's no composition string then it won't draw anything
            Main.instance.DrawWindowsIMEPanel(GetDimensions().Position());
        }

        Rectangle rect = GetDimensions().ToRectangle();
        Vector2 pos = GetDimensions().Position() + Vector2.One * 6;
        Color color = Color.White;

        if (rect.Contains(Main.MouseScreen.ToPoint()))
            color = new Color(180, 180, 180);

        const float Scale = 1f;
        string displayed = currentValue ?? "";

        if (displayed != string.Empty)
            Utils.DrawBorderString(spriteBatch, displayed, pos, color, Scale);
        else
            Utils.DrawBorderString(spriteBatch, _backingString, pos, Color.Gray.MultiplyRGB(color), Scale);
            
        // Composition string + cursor drawing below
        if (!CurrentlyTyping)
            return;

        pos.X += FontAssets.MouseText.Value.MeasureString(displayed).X * Scale;
        string compositionString = Platform.Get<IImeService>().CompositionString;

        if (compositionString is { Length: > 0 })
        {
            Utils.DrawBorderString(spriteBatch, compositionString, pos, new Color(255, 240, 20), Scale);
            pos.X += FontAssets.MouseText.Value.MeasureString(compositionString).X * Scale;
        }

        if (Main.GameUpdateCount % 20 < 10)
            Utils.DrawBorderString(spriteBatch, "|", pos, Color.White, Scale);
    }

    [GeneratedRegex("(?<=^| )[0-9]+(.[0-9]+)?(?=$| )|(?<=^| ).[0-9]+(?=$| )")]
    private static partial Regex UnreadableRegex();

    [GeneratedRegex("[0-9]*$")]
    private static partial Regex NumberRegex();
}