﻿using DogecoinTerminal.Common.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace DogecoinTerminal.Common
{
	public class DisplayQRPage : AppPage
	{
		private GraphicsDevice _graphicsDevice;
		private AppButton _confirmButton;
		private AppButton _cancelButton;
		private Texture2D _image;
		private AppText Title;

		public DisplayQRPage(Game game)
			: base(game, true)
		{
			_graphicsDevice = game.GraphicsDevice;
			Title = new AppText("Scan QR", TerminalColor.White, 6, (50, 10));

			Interactables.Add(Title);

			_confirmButton = new AppButton("Next", (88, 88), (98, 98), TerminalColor.Green, TerminalColor.White, 5, (isFirst, self) =>
			{
				Game.Services.GetService<Router>().Return(true);
			});

			_cancelButton = new AppButton("Cancel", (2, 88), (12, 98), TerminalColor.Red, TerminalColor.White, 5, (isFirst, self) =>
			{
				Game.Services.GetService<Router>().Return(false);
			});

			Interactables.Add(_confirmButton);
		}

		public override void OnBack()
		{
			Game.Services.GetService<Router>().Back();
		}

		public override void Draw(VirtualScreen screen)
		{
			screen.DrawImage(_image, (50, 50), (40, 40), (480, 480));
		}


		protected override void OnNav(dynamic value, bool backable)
		{
			var parameters = (DisplayQRPageSettings)value;


			Interactables.Remove(_cancelButton);
			Interactables.Remove(_confirmButton);

			if (parameters.EnableConfirm)
			{
				Interactables.Add(_confirmButton);
			}

			if (parameters.EnableCancel)
			{
				Interactables.Add(_cancelButton);
			}

			Title.Text = parameters.Title;

			_image = new Texture2D(_graphicsDevice, 480, 480);
			var barcodeWriter = new BarcodeWriterPixelData()
			{
				Format = BarcodeFormat.QR_CODE,
				Options = new QrCodeEncodingOptions
				{
					Width = 480,
					Height = 480
				}
			};

			var pixels = barcodeWriter.Write(parameters.QRData).Pixels;

			_image.SetData(pixels);
		}


		public struct DisplayQRPageSettings
		{
			public DisplayQRPageSettings(string qrData, string title, bool enableConfirm, bool enableCancel=false)
			{
				QRData = qrData;
				Title = title;
				EnableConfirm = enableConfirm;
				EnableCancel = enableCancel;
			}
			public string QRData;
			public string Title;
			public bool EnableConfirm;
			public bool EnableCancel;
		}
	}
}
