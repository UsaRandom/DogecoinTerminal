﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DogecoinTerminal.Common.Pages;

namespace DogecoinTerminal.Common
{
    public class Navigation : IReceiver<PromptResult>
	{
		private ConcurrentStack<IPage> _pageHistory = new();
		private PromptResult _promptResult;
		private IServiceProvider _serviceProvider;



		public Navigation(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;


			Messenger.Default.Register(this);
		}

		public void Receive(PromptResult message)
		{
			_promptResult = message;
		}


		public IPage CurrentPage
		{
			get
			{
				if (_pageHistory.TryPeek(out var currentPage))
				{
					return currentPage;
				}
				return default;
			}
		}

		public void Pop()
		{
			if (_pageHistory.TryPop(out var previousPage))
			{
				Messenger.Default.Deregister(previousPage);
				Messenger.Default.Register(CurrentPage);
			}
		}

		public Task PushAsync<T>(params (string key, object value)[] options) where T : IPage
		{
			var newPage = Page.Create<T>(new PageOptions(options), _serviceProvider);

			Messenger.Default.Deregister(CurrentPage);
			_pageHistory.Push(newPage);
			Messenger.Default.Register(newPage);

			return Task.FromResult<object>(null);
		}


		public Task PopToPage<TPageToPopTo>()
		{
			return Task.Run(() =>
			{
				var targetType = typeof(TPageToPopTo);

				lock (_pageHistory)
				{
					var targetTypeExists = false;
					foreach (var page in _pageHistory.ToArray())
					{
						if (targetType.IsInstanceOfType(page))
						{
							targetTypeExists = true;
							break;
						}
					}

					if (!targetTypeExists)
					{
						throw new Exception($"Page {targetType.Name} does not exist in history stack.");
					}



					Messenger.Default.Deregister(CurrentPage);

					while (_pageHistory.TryPeek(out var nextPageToPop))
					{
						if (!targetType.IsInstanceOfType(nextPageToPop))
						{
							if (!_pageHistory.TryPop(out _))
							{
								//we should have already checked for this. should never happen?
								throw new Exception($"Page {targetType.Name} does not exist in history stack.");
							}
						}
					}

					Messenger.Default.Register(CurrentPage);
				}
			});
		}

		public async Task<bool> TryInsertBeforeAsync<TPageToInsert, TPageToLookFor>(params (string key, object value)[] options) where TPageToInsert : IPage where TPageToLookFor : IPage
		{
			var newPage = Page.Create<TPageToInsert>(new PageOptions(options), _serviceProvider);

			var beforePageType = typeof(TPageToLookFor);

			return await Task.Run<bool>(() =>
			{
				lock (_pageHistory)
				{

					var beforePageExists = false;
					foreach (var page in _pageHistory.ToArray())
					{
						if (beforePageType.IsInstanceOfType(page))
						{
							beforePageExists = true;
							break;
						}
					}

					if (!beforePageExists)
					{
						throw new Exception($"Page {beforePageType.Name} does not exist in history stack.");
					}


					var foundTarget = false;
					var pulledOffPages = new Stack<IPage>();

					//pull off a page, put it in a separate stack, check if it's our type.
					while (_pageHistory.TryPop(out var nextPagePulledOff))
					{
						pulledOffPages.Push(nextPagePulledOff);

						if (beforePageType.IsInstanceOfType(nextPagePulledOff))
						{
							foundTarget = true;
							break;
						}
					}

					//if we found our target, add our new page.
					if (foundTarget)
					{
						_pageHistory.Push(newPage);
					}
					else
					{
						//we should have already checked for this. should never happen?
						throw new Exception($"Page {beforePageType.Name} does not exist in history stack.");
					}


					//rebuild our history stack by adding backed the pulled off pages.
					while (pulledOffPages.TryPop(out var pulledOffPage))
					{
						_pageHistory.Push(pulledOffPage);
					}

					return foundTarget;
				}
			});
		}

		public async Task<PromptResult> PromptAsync<T>(params (string key, object value)[] options) where T : IPage
		{
			_promptResult = default;

			await PushAsync<T>(options);

			return await Task.Run(() =>
			{
				while (_promptResult == default) { }
				Pop();
				return _promptResult;
			});
		}


	}
}
