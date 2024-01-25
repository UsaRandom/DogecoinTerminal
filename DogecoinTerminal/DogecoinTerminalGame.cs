﻿
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DogecoinTerminal.Common;
using Microsoft.Xna.Framework.Input;
using System;
using DogecoinTerminal.Pages;
using DogecoinTerminal.Common.BackgroundScenes;
using System.Xml.Linq;
using DogecoinTerminal.Common.Controls;
using System.Runtime;
using DogecoinTerminal.old;

namespace DogecoinTerminal
{
    public class DogecoinTerminalGame : Game
    {   
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
		public FontSystem _fontSystem;

        public VirtualScreen _screen;

        private Navigation _nav;


		private ButtonControl _devButton;


		private ITerminalSettings _settings;

		public DogecoinTerminalGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _screen = new VirtualScreen();
            _fontSystem = new FontSystem();

		}

        protected override void Initialize()
        {

			_settings = new TerminalSettings();



			Strings.Current.SelectLanguage(Language.Languages["en"]);
			TerminalColor.Init(_graphics.GraphicsDevice);


            _screen.Init(_graphics, useFullScreen: _settings.GetBool("terminal-fullscreen", false));

			_nav = new Navigation(Services);


			Services.AddService(Strings.Current);
            Services.AddService(_nav);
			Services.AddService(_screen);
            Services.AddService(new Images(GraphicsDevice));
			Services.AddService(_settings);
			Services.AddService(GraphicsDevice);
			Services.AddService<ITerminalService>(new TerminalService(Services));
			Services.AddService<IMnemonicProvider>(new TPM2MnemonicProvider());
			Services.AddService<Game>(this);


			//background
			_background = new MoonBackgroundScene(Services, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);


			//dev tools
			var devButtonEl = new XElement("button");
			devButtonEl.Add(new XAttribute("Name", "DevButton"));
			devButtonEl.Add(new XAttribute("StartPosition", "0,97"));
			devButtonEl.Add(new XAttribute("EndPosition", "4,99"));
			devButtonEl.Add(new XAttribute("BackgroundColor", "Blue"));
			devButtonEl.Add(new XAttribute("ForegroundColor", "White"));
			devButtonEl.Add(new XAttribute("TextSize", "2"));
			devButtonEl.Add(new XAttribute("Text", "copy"));
			_devButton = new ButtonControl(devButtonEl);
			_moveHandler = new MoveHandlesControlVisitor(_screen);

			base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _screen.Load(_spriteBatch);

            _nav.PushAsync<UnlockTerminalPage>();
        }

        private ButtonState lastButtonState = ButtonState.Released;

        private MoveHandlesControlVisitor _moveHandler;

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			_nav.CurrentPage.Update(gameTime, Services);

            if (IsActive)
            {
				var mouseState = Mouse.GetState(Window);

				if (mouseState.LeftButton == ButtonState.Pressed &&
					lastButtonState == ButtonState.Released)
				{
					Messenger.Default.Send(
						new UserClickMessage(
							_screen.WindowCoordToVirtualCoord(
								new Point(mouseState.X, mouseState.Y))));

					if (_settings.GetBool("terminal-devmode", false))
					{
						if (_devButton.ContainsPoint(_screen.WindowCoordToVirtualCoord(
								new Point(mouseState.X, mouseState.Y))))
						{

							var xmlExporter = new XmlExporterControlVisitor();

							foreach(var control in _nav.CurrentPage.Controls)
							{
								control.AcceptVisitor(xmlExporter);
							}

							TextCopy.ClipboardService.SetText(xmlExporter.PageElement.ToString());
						}
					}

				}

				lastButtonState = mouseState.LeftButton;



				if (_settings.GetBool("terminal-devmode", false))
				{
					_moveHandler.UpdateMouse();

					foreach (var control in _nav.CurrentPage.Controls)
					{
						control.AcceptVisitor(_moveHandler);
					}

				}
			}


            if (_settings.GetBool("terminal-background", false))
            {
				_background.Update(gameTime, Services);
			}


			base.Update(gameTime);
        }


        IBackgroundScene _background;
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(TerminalColor.Grey.Color);


			_spriteBatch.Begin();


            if (_settings.GetBool("terminal-background", false))
            {
				_background.Draw(gameTime, _spriteBatch, Services);
			}

			_nav.CurrentPage.Draw(gameTime, Services);

            if(_settings.GetBool("terminal-devmode", false))
            {
				var handles = new DrawHandlesControlVisitor(_spriteBatch, _screen);

				foreach (var control in _nav.CurrentPage.Controls)
				{
					control.AcceptVisitor(handles);
				}

				_devButton.Draw(gameTime, Services);
				_screen.DrawText(_screen.WindowCoordToVirtualCoord(Mouse.GetState().Position).ToString(), TerminalColor.White, 2, new Point(50, 98));
			}


			_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}