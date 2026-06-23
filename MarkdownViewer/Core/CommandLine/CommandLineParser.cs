namespace MarkdownViewer.Core
{
    using System;
    using System.Collections;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Reflection;
    using System.Text;

    public static class CommandLineParser
    {
        public static T Parse<T>(string[] args) where T : new()
        {
            if (args.Any(IsHelpArgument))
            {
                PrintHelp<T>();
                Environment.Exit(0);
            }

            var result = new T();

            var mappings = BuildMappings<T>();

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if (TryParseSwitch(arg, mappings, result))
                {
                    continue;
                }

                if (TryParseAssignment(args, ref i, mappings, result))
                {
                    continue;
                }

                throw new ArgumentException($"Unbekanntes Argument '{arg}'.");
            }

            ValidateRequiredProperties(mappings, result);

            ValidateDataAnnotations(result);

            return result;
        }

        private static Dictionary<string, PropertyMetadata> BuildMappings<T>()
        {
            var result = new Dictionary<string, PropertyMetadata>(StringComparer.OrdinalIgnoreCase);

            foreach (var property in typeof(T).GetProperties())
            {
                var option = property.GetCustomAttribute<CommandLineOptionAttribute>();

                if (option == null)
                {
                    continue;
                }

                var metadata = new PropertyMetadata(property, option);

                result[option.Name] = metadata;

                if (!string.IsNullOrWhiteSpace(option.ShortName))
                {
                    result[option.ShortName] = metadata;
                }
            }

            return result;
        }

        private static bool TryParseSwitch<T>(string arg, Dictionary<string, PropertyMetadata> mappings, T target)
        {
            string name = null;

            if (arg.StartsWith("--", StringComparison.OrdinalIgnoreCase))
            {
                name = arg[2..];
            }
            else if (arg.StartsWith("-", StringComparison.OrdinalIgnoreCase))
            {
                name = arg[1..];
            }
            else if (arg.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                name = arg[1..];
            }

            if (name == null)
            {
                return false;
            }

            if (!mappings.TryGetValue(name, out var metadata))
            {
                return false;
            }

            if (metadata.Property.PropertyType != typeof(bool))
            {
                return false;
            }

            metadata.Property.SetValue(target, true);

            return true;
        }

        private static bool TryParseAssignment<T>(string[] args, ref int index, Dictionary<string, PropertyMetadata> mappings, T target)
        {
            var current = args[index];

            string key = null;
            string value = null;

            if (current.StartsWith("--", StringComparison.OrdinalIgnoreCase))
            {
                ParseArgument(current[2..], out key, out value);
            }
            else if (current.StartsWith("-", StringComparison.OrdinalIgnoreCase))
            {
                ParseArgument(current[1..], out key, out value);
            }
            else
            {
                return false;
            }

            if (key == null)
            {
                return false;
            }

            if (mappings.TryGetValue(key, out var metadata) == false)
            {
                return false;
            }

            if (metadata.Property.PropertyType == typeof(bool))
            {
                return false;
            }

            if (value == null)
            {
                if (index + 1 >= args.Length)
                {
                    throw new ArgumentException( $"Wert für '{key}' fehlt.");
                }

                value = args[++index];
            }

            metadata.Property.SetValue(target,  ConvertToType(value, metadata.Property.PropertyType));

            return true;
        }

        private static void ParseArgument(string text, out string key, out string value)
        {
            var pos = text.IndexOf('=');

            if (pos < 0)
            {
                key = text;
                value = null;
                return;
            }

            key = text[..pos];
            value = text[(pos + 1)..];
        }

        private static bool IsHelpArgument(string arg)
        {
            return arg.Equals("/h",
                       StringComparison.OrdinalIgnoreCase)
                   || arg.Equals("/?",
                       StringComparison.OrdinalIgnoreCase)
                   || arg.Equals("-h",
                       StringComparison.OrdinalIgnoreCase)
                   || arg.Equals("--help",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static void ValidateRequiredProperties<T>(Dictionary<string, PropertyMetadata> mappings,
            T instance)
        {
            foreach (var property in mappings.Values.DistinctBy(x => x.Property.Name))
            {
                bool required = property.Option.Required || property.Property.GetCustomAttribute<RequiredAttribute>() != null;

                if (required == false)
                {
                    continue;
                }

                var value = property.Property.GetValue(instance);

                bool missing = value == null || value is string s && string.IsNullOrWhiteSpace(s);

                if (missing == true)
                {
                    throw new ValidationException($"Pflichtparameter '{property.Option.Name}' fehlt.");
                }
            }
        }

        private static void ValidateDataAnnotations(object instance)
        {
            var context = new ValidationContext(instance);

            Validator.ValidateObject(instance, context, validateAllProperties: true);
        }

        private static object ConvertToType(string value, Type type)
        {
            var nullable = Nullable.GetUnderlyingType(type);

            if (nullable != null)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return null;
                }

                return ConvertToType(value, nullable);
            }

            if (IsListType(type))
            {
                return ParseList(value, type);
            }

            if (type.IsEnum)
            {
                return Enum.Parse(type, value, true);
            }

            if (type == typeof(string))
            {
                return value;
            }

            if (type == typeof(bool))
            {
                return bool.Parse(value);
            }

            if (type == typeof(int))
            {
                return int.Parse(value, CultureInfo.InvariantCulture);
            }

            if (type == typeof(long))
            {
                return long.Parse(value, CultureInfo.InvariantCulture);
            }

            if (type == typeof(double))
                return double.Parse(value, CultureInfo.InvariantCulture);

            if (type == typeof(decimal))
            {
                return decimal.Parse(value, CultureInfo.InvariantCulture);
            }

            if (type == typeof(Guid))
            {
                return Guid.Parse(value);
            }

            if (type == typeof(DateTime))
            {
                return DateTime.Parse(value, CultureInfo.InvariantCulture);
            }

            if (type == typeof(TimeSpan))
            {
                return TimeSpan.Parse(value, CultureInfo.InvariantCulture);
            }

            return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }

        private static bool IsListType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        private static object ParseList(string value, Type listType)
        {
            var itemType = listType.GetGenericArguments()[0];

            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType))!;

            foreach (var item in value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                list.Add(ConvertToType(item, itemType)!);
            }

            return list;
        }

        public static void PrintHelp<T>()
        {
            StringBuilder helpSb = new StringBuilder();
            helpSb.AppendLine();
            helpSb.AppendLine("Verfügbare Parameter");
            helpSb.AppendLine();

            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                CommandLineOptionAttribute option = property.GetCustomAttribute<CommandLineOptionAttribute>();
                if (option == null)
                {
                    continue;
                }

                string aliases = option.ShortName == null ? string.Empty : $"-{option.ShortName}, ";

                string syntax = property.PropertyType == typeof(bool) ? $"{aliases}--{option.Name}" : $"{aliases}--{option.Name}={BuildTypeDescription(property.PropertyType)}";

                helpSb.AppendLine($"{syntax,-30} {option.HelpText}");
            }

            helpSb.AppendLine();
            helpSb.AppendLine("Hilfe:");
            helpSb.AppendLine("  /h");
            helpSb.AppendLine("  /?");
            helpSb.AppendLine("  -h");
            helpSb.AppendLine("  --help");

            App.InfoMessage(helpSb.ToString());
        }

        private static string BuildTypeDescription(Type type)
        {
            var nullable = Nullable.GetUnderlyingType(type);

            if (nullable != null)
            {
                return BuildTypeDescription(nullable) + "?";
            }

            if (IsListType(type))
            {
                var item = type.GetGenericArguments()[0];
                return $"<{item.Name},...>";
            }

            if (type.IsEnum)
            {
                return $"<{string.Join("|", Enum.GetNames(type))}>";
            }

            return $"<{type.Name}>";
        }

        private sealed record PropertyMetadata(PropertyInfo Property, CommandLineOptionAttribute Option);
    }
}
