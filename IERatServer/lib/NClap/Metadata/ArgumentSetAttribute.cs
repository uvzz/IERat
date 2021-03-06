using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using NClap.Exceptions;
using NClap.Utilities;

namespace NClap.Metadata
{
    /// <summary>
    /// Attributes that may be used on classes representing argument sets
    /// (i.e. classes including fields and/or properties with
    /// ArgumentAttributes).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class ArgumentSetAttribute : Attribute
    {
        private string[] _namedArgumentPrefixes;
        private string[] _shortNameArgumentPrefixes;
        private char[] _argumentValueSeparators;
        private string _logo;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ArgumentSetAttribute()
        {
            // Detect whether '/' is a file-system separator; if it is, then we
            // choose not to include it in any default prefix lists.
            var useForwardSlashAsPrefix = !Path.DirectorySeparatorChar.Equals('/');

            // Initialize properties with basic defaults.
            AnswerFileArgumentPrefix = null;

            // Initialize style, with a guess as to the default.
            if (useForwardSlashAsPrefix)
            {
                Style = ArgumentSetStyle.PowerShell;
            }
            else
            {
                Style = ArgumentSetStyle.GetOpt;
            }
        }

        /// <summary>
        /// If this is non-null, it is included as the top-level description summary
        /// for the argument set in any help/usage messages generated.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optionally provides examples for usage information.
        /// </summary>
        public string[] Examples { get; set; }

        /// <summary>
        /// If this is non-null, it is the argument prefix that references an
        /// argument answer file.
        /// </summary>
        public string AnswerFileArgumentPrefix { get; set; }

        /// <summary>
        /// True to indicate that all writable, public members (in the type
        /// annotated with this attribute) should be treated as optional named
        /// arguments.
        /// </summary>
        public bool PublicMembersAreNamedArguments { get; set; }

        /// <summary>
        /// If this is non-null, it is the set of prefixes that indicate named
        /// arguments indicated by long name.  The first separator listed
        /// in this array is considered "preferred" and will be used in
        /// generated usage help information.  This array may not be null.
        /// </summary>
        public string[] NamedArgumentPrefixes
        {
            get => _namedArgumentPrefixes;
            set => _namedArgumentPrefixes = value ?? Array.Empty<string>();
        }

        /// <summary>
        /// If this is non-null, it is the set of prefixes that indicate named
        /// arguments' short names.  The first separator listed in this array
        /// is considered "preferred" and will be used in generated usage help
        /// information.  This array may not be null.
        /// </summary>
        public string[] ShortNameArgumentPrefixes
        {
            get => _shortNameArgumentPrefixes;
            set => _shortNameArgumentPrefixes = value ?? Array.Empty<string>();
        }

        /// <summary>
        /// The set of characters that separate a named argument from the
        /// value associated with it.  The first separator listed in this
        /// array is considered "preferred" and will be used in generated
        /// usage help information.  This array may not be null.
        /// </summary>
        public char[] ArgumentValueSeparators
        {
            get => _argumentValueSeparators;
            set => _argumentValueSeparators = value ?? Array.Empty<char>();
        }

        /// <summary>
        /// General argument parsing style; setting this property impacts other properties as well.
        /// </summary>
        public ArgumentSetStyle Style
        {
            get => throw new NotSupportedException($"{nameof(Style)} property is not readable");

            set
            {
                switch (value)
                {
                case ArgumentSetStyle.Unspecified:
                    break;

                case ArgumentSetStyle.WindowsCommandLine:
                    AllowNamedArgumentValueAsSucceedingToken = false;
                    AllowMultipleShortNamesInOneToken = false;
                    AllowElidingSeparatorAfterShortName = false;
                    NameGenerationFlags = ArgumentNameGenerationFlags.UseOriginalCodeSymbol;
                    NamedArgumentPrefixes = new[] { "/", "-" };
                    ShortNameArgumentPrefixes = new[] { "/", "-" };
                    ArgumentValueSeparators = new[] { '=', ':' };
                    PreferNamedArgumentValueAsSucceedingToken = false;
                    break;

                case ArgumentSetStyle.PowerShell:
                    AllowNamedArgumentValueAsSucceedingToken = true;
                    AllowMultipleShortNamesInOneToken = false;
                    AllowElidingSeparatorAfterShortName = false;
                    NameGenerationFlags = ArgumentNameGenerationFlags.UseOriginalCodeSymbol;
                    NamedArgumentPrefixes = new[] { "-" };
                    ShortNameArgumentPrefixes = new[] { "-" };
                    ArgumentValueSeparators = new[] { ':' };
                    PreferNamedArgumentValueAsSucceedingToken = true;
                    break;

                case ArgumentSetStyle.GetOpt:
                    NamedArgumentPrefixes = new[] { "--" };
                    ShortNameArgumentPrefixes = new[] { "-" };
                    ArgumentValueSeparators = new[] { '=' };
                    AllowNamedArgumentValueAsSucceedingToken = true;
                    AllowMultipleShortNamesInOneToken = true;
                    AllowElidingSeparatorAfterShortName = true;
                    NameGenerationFlags =
                        ArgumentNameGenerationFlags.GenerateHyphenatedLowerCaseLongNames |
                        ArgumentNameGenerationFlags.PreferLowerCaseForShortNames;
                    PreferNamedArgumentValueAsSucceedingToken = false;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), $"Unknown style: {value}");
                }
            }
        }

        /// <summary>
        /// True to indicate that a named argument's value may be present in
        /// the succeeding token after the name; false to indicate that it
        /// must be part of the same token.
        /// </summary>
        public bool AllowNamedArgumentValueAsSucceedingToken { get; set; }

        /// <summary>
        /// True to indicate that this argument set prefers for named argument
        /// values to be provided in a succeeding token rather than in the same
        /// token after a separator; should only be set to true if
        /// <see cref="AllowNamedArgumentValueAsSucceedingToken"/> is also set
        /// to true.  Primarily used for formatting arguments (unparsing) back
        /// into a command line, or for generating help information.
        /// </summary>
        public bool PreferNamedArgumentValueAsSucceedingToken { get; set; }

        /// <summary>
        /// Flags indicating how to auto-generate names.
        /// </summary>
        public ArgumentNameGenerationFlags NameGenerationFlags { get; set; }

        /// <summary>
        /// True to allow a command-line argument to indicate multiple short
        /// names in one token, or false to disable this behavior. This
        /// behavior is only useful with arguments that take no values,
        /// when the short name prefixes are disjoint from the long name
        /// prefixes, and when short names are constrained to be one character
        /// long. Enabling this behavior will fail if any of these conditions
        /// are not true.
        /// </summary>
        public bool AllowMultipleShortNamesInOneToken { get; set; }

        /// <summary>
        /// True to indicate that a short-name argument's value may be present in
        /// the same token as the name, without a separator between the name and
        /// it; false to indicate that it must be part of the same token. This
        /// behavior requires that short names be only one character long.
        /// </summary>
        public bool AllowElidingSeparatorAfterShortName { get; set; }

        /// <summary>
        /// Indicates whether short names of named arguments are constrained to
        /// being one character long.
        /// </summary>
        public bool ShortNamesAreOneCharacterLong =>
            AllowMultipleShortNamesInOneToken || AllowElidingSeparatorAfterShortName;

        /// <summary>
        /// Optionally provides logo text to be displayed at the top of help
        /// output. Expected to be a <see cref="string"/>.
        /// </summary>
        public object Logo
        {
            get => _logo;

            set
            {
                if (value != null && !(value is string))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _logo = (string)value;
            }
        }

        /// <summary>
        /// The logo, as a <see cref="ColoredMultistring"/>.
        /// </summary>
        [Obsolete("This property is no longer implemented and will always return null.")]
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "[Legacy]")]
        public ColoredMultistring LogoString => null;

        /// <summary>
        /// Whether or not <see cref="Logo"/> is a format string that should
        /// be expanded.
        /// </summary>
        public bool ExpandLogo { get; set; }

        /// <summary>
        /// True for names to be case sensitive; false for them to be case
        /// insensitive.
        /// </summary>
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// Validate that this object's properties, when considered as a whole,
        /// are valid. This is exposed as a separate operation, and not checked
        /// as part of property implementations to provide better diagnostics
        /// and to delay validation until all property changes have been made
        /// by the calling application.
        /// </summary>
        internal void Validate()
        {
            var problems = new List<string>();

            // Validate name prefixes.
            if (NamedArgumentPrefixes.Overlaps(ShortNameArgumentPrefixes))
            {
                if (AllowMultipleShortNamesInOneToken)
                {
                    problems.Add($"Cannot enable {nameof(AllowMultipleShortNamesInOneToken)}; long name prefixes are not disjoint from short name prefixes (e.g. {string.Join(" ", NamedArgumentPrefixes.Intersect(ShortNameArgumentPrefixes))}).");
                }

                if (AllowElidingSeparatorAfterShortName)
                {
                    problems.Add($"Cannot enable {nameof(AllowElidingSeparatorAfterShortName)}; long name prefixes are not disjoint from short name prefixes (e.g. {string.Join(" ", NamedArgumentPrefixes.Intersect(ShortNameArgumentPrefixes))}).");
                }
            }

            // Throw exception if we found multiple problems.
            if (problems.Count > 0)
            {
                throw new InvalidArgumentSetException($"Invalid {nameof(ArgumentSetAttribute)} specified for argument set:" +
                    Environment.NewLine +
                    string.Join(Environment.NewLine, problems));
            }
        }
    }
}
