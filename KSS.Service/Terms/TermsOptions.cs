#nullable enable
using System.Text.RegularExpressions;

namespace KSS.Service.Terms
{
    /// <summary>
    /// The current terms version per application, read from configuration
    /// (Terms:CurrentVersion:&lt;module code&gt;, i.e. env Terms__CurrentVersion__&lt;module code&gt;).
    /// A version is configuration, not a table row, so publishing new terms needs no database change.
    /// An application with no configured version is not enabled: every request for it returns 404.
    /// </summary>
    public sealed class TermsOptions
    {
        public const string SectionPath = "Terms:CurrentVersion";

        // The style of every Module code, and the width of Module.Code.
        private static readonly Regex KeyPattern = new("^[a-z0-9]{1,30}$", RegexOptions.CultureInvariant);

        public IReadOnlyDictionary<string, string> CurrentVersions { get; }

        public TermsOptions(IReadOnlyDictionary<string, string> currentVersions) => CurrentVersions = currentVersions;

        /// <summary>
        /// Builds the options from configuration entries and refuses any invalid entry, so the host fails
        /// at startup instead of writing a bad version later. Error messages name the key, never the value.
        /// </summary>
        public static TermsOptions FromEntries(IEnumerable<KeyValuePair<string, string?>> entries)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var (key, value) in entries)
            {
                var name = $"{SectionPath}:{key}";
                if (!KeyPattern.IsMatch(key))
                    throw new InvalidOperationException(
                        $"Configuration '{name}': the application key must be 1-30 lowercase letters or digits.");
                if (!IsValidVersion(value))
                    throw new InvalidOperationException(
                        $"Configuration '{name}': the version must be 1-32 characters, with no leading or trailing whitespace and no tab, CR or LF.");
                map[key] = value!;
            }
            return new TermsOptions(map);
        }

        /// <summary>The same rule the table's version check enforces, so an invalid version never reaches an insert.</summary>
        public static bool IsValidVersion(string? version) =>
            !string.IsNullOrEmpty(version)
            && version.Length <= 32
            && version.Trim() == version
            && version.IndexOfAny(new[] { '\t', '\r', '\n' }) < 0;
    }
}
