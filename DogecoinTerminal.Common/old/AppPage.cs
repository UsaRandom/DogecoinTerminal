﻿using DogecoinTerminal.Common.old.Components;
using DogecoinTerminal.Common.old;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DogecoinTerminal.Common.old
{
	public abstract class AppPage
	{
		private bool _isBackable = false;

		private AppImage _backButton;

		protected Game Game { get; set; }

		protected AppPage(Game game, bool showDoge = false)
		{
			this.Game = game;

			Interactables = new List<Interactable>();

			_backButton = new AppImage(Images.ArrowImage, (3, 3), (9, 7), Images.ArrowImageData, (isFirst, self) =>
			{
				if (isFirst && _isBackable)
				{
					Game.Services.GetService<Router>().Back();
				}
			});

			if(showDoge )
			{
				Interactables.Add(
					new AppImage(Images.DogeImage,
						(85, 5), (95, 15), Images.DogeImageDim)
					);

				Interactables.Add(
					new AppText("Đogecoin\n Terminal", TerminalColor.White, 3, (90, 27))
					);
			}
		}


		public IList<Interactable> Interactables { get; set; }

		public virtual void Update()
		{

		}


		public virtual void Draw(VirtualScreen screen)
		{

		}


		public void DrawScreen(VirtualScreen screen)
		{
			foreach (var item in Interactables)
			{
				item.Draw(screen);
			}

			Draw(screen);
		}


		public abstract void OnBack();


		public virtual void Cleanup()
		{

		}

		protected abstract void OnNav(dynamic value, bool backable);

		public void OnNavigation(dynamic value, bool backable)
		{
			_isBackable = backable;

			if (_isBackable)
			{
				Interactables.Add(_backButton);
			}
			else
			{
				Interactables.Remove(_backButton);
			}

			OnNav(value, backable);
		}
	}
}