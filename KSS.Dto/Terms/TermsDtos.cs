#nullable enable
using System.Text.Json.Serialization;

namespace KSS.Dto
{
    /// <summary>
    /// Body of POST Api/Terms/Accept. Unknown properties are refused rather than ignored, so a caller
    /// cannot slip a person id or any other field into the request.
    /// </summary>
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed class AcceptTermsRequest
    {
        public string? ApplicationKey { get; set; }

        /// <summary>The version of the terms text the user was shown.</summary>
        public string? Version { get; set; }
    }

    /// <summary>
    /// The caller's acceptance status for the application's CURRENT terms version.
    /// An acceptance of an older version reads as not accepted.
    /// </summary>
    public sealed record TermsStatus(string ApplicationKey, string Version, bool Accepted, DateTime? AcceptedAt);
}
