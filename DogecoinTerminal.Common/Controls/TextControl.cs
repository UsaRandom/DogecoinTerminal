﻿
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DogecoinTerminal.Common
{
    public class TextControl : PageControl
	{
		public TextControl(XElement element)
			: base(element)
		{
			Position = GetPoint(element.Attribute(nameof(Position)));
			Color = GetTerminalColor(element.Attribute(nameof(Color)));
			TextSize = int.Parse(element.Attribute(nameof(TextSize)).Value);
			StringDef = element.Attribute(nameof(StringDef))?.Value;
			Text = StringDef ?? element.Attribute(nameof(Text))?.Value;
		}

		public string Text { get; set; }
		public Point Position { get; set; }
		public TerminalColor Color { get; set; }
		public TerminalColor ForegroundColor { get; set; }
		public int TextSize { get; set; }


		public string StringDef { get; set; }

		public override bool ContainsPoint(Point point)
		{
			//TODO: is this needed? maybe for word import?
			return false;
		}

		public override void Draw(GameTime time, IServiceProvider services)
		{
			var screen = services.GetService<VirtualScreen>();


			screen.DrawText(Text, ForegroundColor, TextSize, Position);
		}

		public override void Update(GameTime time, IServiceProvider services)
		{
			var strings = services.GetService<Strings>();

			if (!string.IsNullOrEmpty(StringDef)
				&& Text != strings[StringDef])
			{
				Text = strings[StringDef];
			}
		}


		public override void AcceptVisitor(IControlVisitor visitor)
		{
			visitor.VisitText(this);
		}
	}
}
