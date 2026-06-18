using Microsoft.Xna.Framework;

namespace MHEventsList.UI;

public sealed class Theme
{
	public Color Background { get; init; }

	public Color PanelBackground { get; init; }

	public Color TextPrimary { get; init; }

	public Color TextSecondary { get; init; }

	public Color Accent { get; init; }

	public Color Success { get; init; }

	public Color Warning { get; init; }

	public Color Error { get; init; }

	public Color Border { get; init; }

	public Color Hover { get; init; }

	public Color Selected { get; init; }

	public static Theme Dark { get; } = new Theme
	{
		Background = new Color(30, 30, 35),
		PanelBackground = new Color(45, 45, 50),
		TextPrimary = new Color(240, 240, 240),
		TextSecondary = new Color(160, 160, 170),
		Accent = new Color(100, 149, 237),
		Success = new Color(100, 200, 100),
		Warning = new Color(230, 180, 80),
		Error = new Color(220, 80, 80),
		Border = new Color(70, 70, 80),
		Hover = new Color(255, 255, 255, 30),
		Selected = new Color(100, 149, 237, 60)
	};

	public static Theme Light { get; } = new Theme
	{
		Background = new Color(245, 245, 248),
		PanelBackground = new Color(255, 255, 255),
		TextPrimary = new Color(30, 30, 35),
		TextSecondary = new Color(100, 100, 110),
		Accent = new Color(60, 120, 200),
		Success = new Color(40, 160, 40),
		Warning = new Color(200, 150, 50),
		Error = new Color(200, 60, 60),
		Border = new Color(200, 200, 210),
		Hover = new Color(0, 0, 0, 20),
		Selected = new Color(60, 120, 200, 40)
	};

	public static Theme Current
	{
		get
		{
			if (!MHEventsListMod.Config.UseDarkTheme)
			{
				return Light;
			}
			return Dark;
		}
	}
}

