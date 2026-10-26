using BioLib;

namespace BioCore
{
    public partial class TabsView
    {
        private string[] commandMenuNames = Array.Empty<string>();
        private bool commandMenuBuilt;

        private void InitializeCommandMenu()
        {
            commandToolStripMenuItem.DropDownOpening += (_, _) => RefreshCommandMenu();
            RefreshCommandMenu();
        }

        private void RefreshCommandMenu()
        {
            // Fiji can initialize after this window was constructed. Its live
            // command table reflects installed plugins when it is available.
            IEnumerable<string> names = Fiji.Macro.Commands.Values
                .Where(command => command != null)
                .Select(command => command.Name);
            if (Fiji.Macro.Commands.Count == 0)
                names = ReadBundledCommandNames();

            string[] commands = names.Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(name => name, StringComparer.Ordinal)
                .ToArray();
            if (commandMenuBuilt && commandMenuNames.SequenceEqual(commands, StringComparer.Ordinal))
                return;
            commandMenuBuilt = true;
            commandMenuNames = commands;
            commandToolStripMenuItem.DropDownItems.Clear();

            var groups = new Dictionary<char, ToolStripMenuItem>();
            foreach (char letter in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                var group = new ToolStripMenuItem(letter.ToString());
                groups.Add(letter, group);
                commandToolStripMenuItem.DropDownItems.Add(group);
            }
            ToolStripMenuItem? numbers = null, other = null;
            foreach (string command in commands)
            {
                char first = char.ToUpperInvariant(command[0]);
                ToolStripMenuItem group;
                if (groups.TryGetValue(first, out var letterGroup))
                    group = letterGroup;
                else if (char.IsDigit(first))
                {
                    if (numbers == null)
                    {
                        numbers = new ToolStripMenuItem("0–9");
                        commandToolStripMenuItem.DropDownItems.Insert(0, numbers);
                    }
                    group = numbers;
                }
                else
                {
                    if (other == null)
                    {
                        other = new ToolStripMenuItem("Other");
                        commandToolStripMenuItem.DropDownItems.Add(other);
                    }
                    group = other;
                }
                var item = new ToolStripMenuItem(command.Replace("&", "&&")) { Tag = command };
                item.Click += MenuItem_Click;
                group.DropDownItems.Add(item);
            }
        }

        private static IEnumerable<string> ReadBundledCommandNames()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "macro-commands.csv");
            if (!File.Exists(path))
                path = Path.Combine(Path.GetDirectoryName(typeof(TabsView).Assembly.Location) ?? "", "macro-commands.csv");
            if (!File.Exists(path))
                return Array.Empty<string>();
            try
            {
                // The bundled file has an unquoted command name in the first
                // column; plugin arguments may contain commas in later columns.
                return File.ReadLines(path).Skip(1)
                    .Select(line => line.IndexOf(',') is int comma && comma > 0 ? line[..comma].Trim() : "")
                    .Where(name => name.Length > 0).ToArray();
            }
            catch (IOException ex)
            {
                System.Diagnostics.Trace.WriteLine($"Could not load Fiji commands: {ex.Message}");
                return Array.Empty<string>();
            }
            catch (UnauthorizedAccessException ex)
            {
                System.Diagnostics.Trace.WriteLine($"Could not load Fiji commands: {ex.Message}");
                return Array.Empty<string>();
            }
        }
    }
}
