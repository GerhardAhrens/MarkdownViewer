namespace MarkdownViewer.Core
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CommandLineOptionAttribute : Attribute
    {
        public string Name { get; }

        public string ShortName { get; init; }

        public string HelpText { get; init; } = string.Empty;

        public bool Required { get; init; }

        public CommandLineOptionAttribute(string name)
        {
            Name = name;
        }
    }
}
