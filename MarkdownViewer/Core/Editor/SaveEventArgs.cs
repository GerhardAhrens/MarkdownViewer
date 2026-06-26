namespace MarkdownViewer.Core.Editor
{
    using System;

    public class SaveEventArgs : EventArgs
    {
        public SaveEventArgs(string flatFileName)
        {
            this.FlatfileName = flatFileName;
        }
        public string FlatfileName { get; set; }
    }
}
