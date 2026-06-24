namespace MarkdownViewer.Core
{
    using Microsoft.Win32;

    using System.Runtime.InteropServices;

    public static class FileAssociationManager
    {
        private const string Extension = ".md";
        private const string ProgId = "MarkdownViewer.SourceFile";

        public static bool IsRegistered()
        {
            using var key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{Extension}");

            return string.Equals(key?.GetValue("")?.ToString(), ProgId, StringComparison.Ordinal);
        }

        public static bool IsOwnedByApplication()
        {
            using var key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{Extension}");

            return string.Equals(key?.GetValue("")?.ToString(), ProgId, StringComparison.OrdinalIgnoreCase);
        }

        public static void Register()
        {
            string exePath = Environment.ProcessPath ?? throw new InvalidOperationException("EXE-Pfad konnte nicht ermittelt werden.");

            try
            {
                // .source -> ProgId
                using (var extKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{Extension}"))
                {
                    extKey.SetValue("", ProgId);
                }

                // ProgId
                using (var progKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProgId}"))
                {
                    progKey.SetValue("", "Source-Datei");

                    using (var iconKey = progKey.CreateSubKey("DefaultIcon"))
                    {
                        iconKey.SetValue("", $"{exePath},0");
                    }

                    using (var commandKey = progKey.CreateSubKey(@"shell\open\command"))
                    {
                        commandKey.SetValue("", $"\"{exePath}\" \"%1\"");
                    }
                }
            }
            catch (Exception ex)
            {

                throw new InvalidOperationException($"Fehler beim Erstellen der Dateizuordnung ProgId: {ProgId}; Typ: {Extension}", ex);
            }

            RefreshExplorer();
        }

        public static void Unregister()
        {
            try
            {
                using (var extKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes", writable: true))
                {
                    extKey?.DeleteSubKeyTree(Extension, false);
                    extKey?.DeleteSubKeyTree(ProgId, false);
                }

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Fehler beim Entfernen der Dateizuordnung ProgId: {ProgId}; Typ: {Extension}", ex);
            }

            RefreshExplorer();
        }

        private static void RefreshExplorer()
        {
            /* SHCNE_ASSOCCHANGED */
            SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero);
        }

        [DllImport("shell32.dll")]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
    }
}
