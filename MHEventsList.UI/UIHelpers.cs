using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace MHEventsList.UI;

public static class UIHelpers
{
	private static Texture2D pixelTexture;

	public static Texture2D Pixel
	{
		get
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			if (pixelTexture == null)
			{
				pixelTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
				pixelTexture.SetData<Color>((Color[])(object)new Color[1] { Color.White });
			}
			return pixelTexture;
		}
	}

	public static void DrawRect(SpriteBatch b, Rectangle rect, Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		b.Draw(Pixel, rect, color);
	}

	public static void DrawRectOutline(SpriteBatch b, Rectangle rect, Color color, int thickness = 1)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		b.Draw(Pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
		b.Draw(Pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
		b.Draw(Pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
		b.Draw(Pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
	}

	public static void DrawPanel(SpriteBatch b, Rectangle rect, Theme theme)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		DrawRect(b, rect, theme.PanelBackground);
		DrawRectOutline(b, rect, theme.Border, 2);
	}

	public static void DrawPanel(SpriteBatch b, Rectangle rect, bool darkMode = false)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Color val = (Color)(darkMode ? new Color(50, 50, 60) : Color.White);
		IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), rect.X, rect.Y, rect.Width, rect.Height, val, 1f, true, -1f);
	}

	public static void DrawTextWithShadow(SpriteBatch b, string text, SpriteFont font, Vector2 position, Color color, float scale = 1f)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		b.DrawString(font, text, position + new Vector2(2f, 2f), Color.Black * 0.3f, 0f, Vector2.Zero, scale, (SpriteEffects)0, 0f);
		b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, (SpriteEffects)0, 0f);
	}

	public static void DrawTruncatedText(SpriteBatch b, string text, SpriteFont font, Vector2 position, Color color, int maxWidth, float scale = 1f)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		string text2 = TruncateText(text, font, maxWidth, scale);
		DrawTextWithShadow(b, text2, font, position, color, scale);
	}

	public static string TruncateText(string text, SpriteFont font, int maxWidth, float scale = 1f)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(text))
		{
			return "";
		}
		Vector2 val = font.MeasureString(text) * scale;
		if (val.X <= (float)maxWidth)
		{
			return text;
		}
		string text2 = text;
		while (text2.Length > 0 && font.MeasureString(text2 + "...").X * scale > (float)maxWidth)
		{
			text2 = text2.Substring(0, text2.Length - 1);
		}
		return text2 + "...";
	}

	public static bool DrawButton(SpriteBatch b, Rectangle rect, string text, Theme theme, bool isHovered, bool isSelected = false, float fontScale = 1f)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		Color color = (isSelected ? theme.Selected : (isHovered ? theme.Hover : theme.PanelBackground));
		DrawRect(b, rect, color);
		DrawRectOutline(b, rect, isHovered ? theme.Accent : theme.Border);
		SpriteFont smallFont = Game1.smallFont;
		Vector2 val = smallFont.MeasureString(text) * fontScale;
		Vector2 position = default(Vector2);
		position._002Ector((float)rect.X + ((float)rect.Width - val.X) / 2f, (float)rect.Y + ((float)rect.Height - val.Y) / 2f);
		DrawTextWithShadow(b, text, smallFont, position, isHovered ? theme.Accent : theme.TextPrimary, fontScale);
		return isHovered;
	}

	public static void DrawScrollbar(SpriteBatch b, Rectangle track, float scrollPosition, float viewportSize, float contentSize, Theme theme)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (!(contentSize <= viewportSize))
		{
			DrawRect(b, track, theme.Border);
			float num = viewportSize / contentSize;
			int num2 = Math.Max(20, (int)((float)track.Height * num));
			float num3 = scrollPosition / (contentSize - viewportSize);
			int num4 = track.Y + (int)((float)(track.Height - num2) * num3);
			Rectangle rect = default(Rectangle);
			rect._002Ector(track.X, num4, track.Width, num2);
			DrawRect(b, rect, theme.Accent);
		}
	}

	public static void DrawSearchBox(SpriteBatch b, Rectangle rect, string text, string placeholder, Theme theme, bool isFocused)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		DrawRect(b, rect, theme.PanelBackground);
		DrawRectOutline(b, rect, isFocused ? theme.Accent : theme.Border, (!isFocused) ? 1 : 2);
		string text2 = (string.IsNullOrEmpty(text) ? placeholder : text);
		Color color = (string.IsNullOrEmpty(text) ? theme.TextSecondary : theme.TextPrimary);
		Vector2 position = default(Vector2);
		position._002Ector((float)(rect.X + 10), (float)(rect.Y + (rect.Height - Game1.smallFont.LineSpacing) / 2));
		DrawTruncatedText(b, text2, Game1.smallFont, position, color, rect.Width - 20);
	}
}
