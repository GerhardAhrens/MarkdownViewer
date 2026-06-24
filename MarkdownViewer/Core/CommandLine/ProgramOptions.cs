namespace MarkdownViewer.Core
{
    using System.Diagnostics;

    public enum ModulTyp
    {
        None,
        Viewer,
        Editor
    }


    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public sealed class ProgramOptions
    {
        [CommandLineOption("dateiname", ShortName = "f", HelpText = "Markdown Datei zum anzeigen")]
        public string Dateiname { get; set; }

        [CommandLineOption("modul", ShortName = "m", HelpText = "Verarbeitungsmodus")]
        public ModulTyp Modul { get; set; }

        [CommandLineOption("register", ShortName = "r", HelpText = "Registrieren des Dateityp md")]
        public bool Register { get; set; }

        [CommandLineOption("unregister", ShortName = "u", HelpText = "Deregistrieren des Dateityp md")]
        public bool Unregister { get; set; }

        private string GetDebuggerDisplay()
        {
            return this.ToString();
        }
    }
}
