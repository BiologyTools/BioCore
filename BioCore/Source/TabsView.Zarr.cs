using AForge;
using BioLib;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace BioCore
{
    public partial class TabsView
    {
        private void InitializeZarrMenu()
        {
            var menu = new ToolStripMenuItem("Open OME-Zarr");
            menu.DropDownItems.Add("Local folder...", null, async (_, _) =>
            {
                using var dialog = new FolderBrowserDialog
                {
                    Description = "Select an OME-Zarr folder",
                    UseDescriptionForTitle = true,
                    ShowNewFolderButton = false
                };
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    await OpenZarrSourceAsync(dialog.SelectedPath);
            });
            menu.DropDownItems.Add("Remote URL...", null, async (_, _) =>
            {
                using var dialog = new Form
                {
                    Text = "Open OME-Zarr URL",
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MinimizeBox = false,
                    MaximizeBox = false,
                    ClientSize = new Size(520, 100)
                };
                var label = new Label { Text = "HTTPS or S3 URL:", AutoSize = true, Location = new Point(12, 12) };
                var input = new TextBox { Location = new Point(12, 32), Width = 496 };
                var ok = new Button { Text = "Open", DialogResult = DialogResult.OK, Location = new Point(352, 65) };
                var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(433, 65) };
                dialog.Controls.AddRange(new Control[] { label, input, ok, cancel });
                dialog.AcceptButton = ok;
                dialog.CancelButton = cancel;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    string url = input.Text.Trim();
                    if (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                        !url.StartsWith("s3://", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show(this, "Enter an HTTPS or S3 URL.", "Open OME-Zarr", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    await OpenZarrSourceAsync(url);
                }
            });
            fileToolStripMenuItem.DropDownItems.Insert(1, menu);
            var saveMenu = new ToolStripMenuItem("Save OME-Zarr...");
            saveMenu.Click += async (_, _) => await SaveZarrAsync();
            fileToolStripMenuItem.DropDownItems.Insert(fileToolStripMenuItem.DropDownItems.IndexOf(saveToolStripMenuItem) + 1, saveMenu);
        }

        private static bool IsZarrSource(string source) =>
            source.Contains(".zarr", StringComparison.OrdinalIgnoreCase) ||
            source.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            source.StartsWith("s3://", StringComparison.OrdinalIgnoreCase);

        private async Task OpenZarrSourceAsync(string source)
        {
            try
            {
                if (source.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    source = "https://" + source.Substring(8);
                else if (source.StartsWith("s3://", StringComparison.OrdinalIgnoreCase))
                    source = "s3://" + source.Substring(5);
                if (!source.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                    !source.StartsWith("s3://", StringComparison.OrdinalIgnoreCase) &&
                    !source.Contains(".zarr", StringComparison.Ordinal))
                    throw new ArgumentException("The folder path must contain .zarr for the current BioLib reader.");

                UseWaitCursor = true;
                BioImage image = await BioImage.OpenURL(source, new ZCT(0, 0, 0), 0, 0, 800, 600);
                if (Directory.Exists(source))
                    image.Annotations.AddRange(Zarr.LoadROIs(source));
                AddTab(image);
                // Avoid persisting potentially sensitive signed URL query parameters.
                bool hasCredentials = Uri.TryCreate(source, UriKind.Absolute, out var uri) &&
                    !string.IsNullOrEmpty(uri.UserInfo);
                if (!source.Contains('?') && !hasCredentials && !App.recent.Contains(source))
                    App.recent.Add(source);
                SaveProperties();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not open OME-Zarr: {ex.Message}", "Open OME-Zarr", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private async Task SaveZarrAsync()
        {
            BioImage image = Image;
            if (image == null)
            {
                MessageBox.Show(this, "Open an image first.", "Save OME-Zarr", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var dialog = new SaveFileDialog
            {
                Title = "Save OME-Zarr folder",
                Filter = "OME-Zarr folders (*.ome.zarr)|*.ome.zarr",
                DefaultExt = "ome.zarr",
                AddExtension = true,
                OverwritePrompt = false,
                FileName = Path.GetFileNameWithoutExtension(image.filename) + ".ome.zarr"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                Enabled = false;
                UseWaitCursor = true;
                await Task.Run(() => ZarrExporter.SaveAsync(image, dialog.FileName));
                MessageBox.Show(this, "OME-Zarr saved successfully.", "Save OME-Zarr", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not save OME-Zarr: {ex.Message}", "Save OME-Zarr", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
                Enabled = true;
            }
        }
    }
}
