﻿namespace DogecoinTerminal.Common
{
	public interface ITerminalSettings
	{
		T Get<T>(string settingName);

		void Set(string settingName, object value);

		bool IsSet(string settingName);
	}
}