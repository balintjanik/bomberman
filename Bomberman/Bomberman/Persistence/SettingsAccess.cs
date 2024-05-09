using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Input;

namespace Bomberman.Persistence
{
    public static class SettingsAccess
    {
        private static string _path = "Files/settings.json";
        private static Key[] _defaultKeys = new Key[] {Key.W, Key.S, Key.A, Key.D, Key.E,
                                        Key.Up, Key.Down, Key.Left, Key.Right, Key.Space,
                                        Key.NumPad8, Key.NumPad2, Key.NumPad4, Key.NumPad6, Key.NumPad9};

        public static void SaveSettings(Key[] keys) {
            string json = JsonSerializer.Serialize(keys);
            File.WriteAllText(_path, json);
        }

        public static Key[] LoadSettings()
        {
            if (File.Exists(_path))
            {
                string json = File.ReadAllText(_path);
                return JsonSerializer.Deserialize<Key[]>(json);
            }
            else
            {
                Key[] copy = new Key[_defaultKeys.Length];
                Array.Copy(_defaultKeys, copy, _defaultKeys.Length);
                return copy;
            }
        }
    }
}
