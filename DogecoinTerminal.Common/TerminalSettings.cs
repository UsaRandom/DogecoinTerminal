﻿using DogecoinTerminal.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace DogecoinTerminal
{
	public class TerminalSettings : ITerminalSettings
    {
        private const string SETTINGS_FILE = "terminalsettings.json";
        private Dictionary<string, object> settings;

        public TerminalSettings()
        {
            LoadSettings();
        }

        public T Get<T>(string settingName, T valueIfDefault = default)
        {
            if (settings.ContainsKey(settingName))
            {
                if (typeof(T) == typeof(decimal))
                {
                    return (T)Convert.ChangeType(decimal.Parse(settings[settingName].ToString()), typeof(T));
                }
                else
                {
                    return (T)Convert.ChangeType(settings[settingName], typeof(T));
                }
            }
            else
            {
				Debug.WriteLine($"Setting '{settingName}' not found.");
				return valueIfDefault;
            }
        }

        public bool IsSet(string settingName)
        {
            return settings.ContainsKey(settingName);
        }

        public void Set(string settingName, object value)
        {
            settings[settingName] = value.ToString();
            SaveSettings();
        }

        private void LoadSettings()
        {
            if (File.Exists(SETTINGS_FILE))
            {
                string json = File.ReadAllText(SETTINGS_FILE);
                settings = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            else
            {
                settings = new Dictionary<string, object>();
            }
        }

        private void SaveSettings()
        {
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SETTINGS_FILE, json);
        }


        private void SetIfMissing(string settingName, object value)
        {
            if (!IsSet(settingName))
            {
                Set(settingName, value);
            }
        }
    }


}
