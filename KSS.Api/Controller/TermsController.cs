using System.Security.Claims;
using KSS.Dto;
using KSS.Service.Terms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KSS.Api.Controller
{
    /// <summary>
    /// Terms acceptance for the signed-in person only.
    ///
    /// Deliberately NOT derived from BaseController: that base exposes generic add/update/remove actions,
    /// which this table must never have. [ApiController] is also omitted, so a malformed body or an unknown
    /// property is answered with this API's own error code instead of the framework's automatic
    /// validation response; parameter sources are therefore declared explicitly.
    ///
    /// Any valid token may call it, for itself: there is no permission claim and no company requirement.
    /// The person is taken from the token's personId claim, never from the request.
    /// </summary>
    [Route("Api/[controller]/[action]")]
    [Authorize]
    public sealed class TermsController : ControllerBase
    {
        public const string PersonIdClaim = "personId";

        public const string BadRequestCode = "TERMS_BAD_REQUEST";
        public const string NoPersonCode = "TERMS_NO_PERSON";
        public const string UnknownApplicationCode = "TERMS_UNKNOWN_APPLICATION";
        public const string StaleCode = "TERMS_STALE";

        // Never a real person: the empty id, and the system id that seed rows carry as CreatedBy.
        private static readonly Guid SystemPersonId = new("00000000-0000-0000-0000-000000000001");

        private readonly TermsService _terms;

        public TermsController(TermsService terms) => _terms = terms;

        /// <summary>GET Api/Terms/MyAcceptance?applicationKey=...</summary>
        [HttpGet]
        public async Task<IActionResult> MyAcceptanceAsync([FromQuery] string? applicationKey, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(applicationKey))
                return Code(StatusCodes.Status400BadRequest, BadRequestCode);

            if (!TryGetPersonId(out var personId))
                return Code(StatusCodes.Status403Forbidden, NoPersonCode);

            return ToActionResult(await _terms.GetAsync(personId, applicationKey, cancellationToken));
        }

        /// <summary>POST Api/Terms/Accept {applicationKey, version}</summary>
        [HttpPost]
        public async Task<IActionResult> AcceptAsync([FromBody] AcceptTermsRequest? body, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid
                || body is null
                || string.IsNullOrWhiteSpace(body.ApplicationKey)
                || string.IsNullOrWhiteSpace(body.Version))
                return Code(StatusCodes.Status400BadRequest, BadRequestCode);

            if (!TryGetPersonId(out var personId))
                return Code(StatusCodes.Status403Forbidden, NoPersonCode);

            return ToActionResult(await _terms.AcceptAsync(personId, body.ApplicationKey, body.Version, cancellationToken));
        }

        private bool TryGetPersonId(out Guid personId) =>
            Guid.TryParse(User.FindFirstValue(PersonIdClaim), out personId)
            && personId != Guid.Empty
            && personId != SystemPersonId;

        private IActionResult ToActionResult(TermsResult result) => result.Outcome switch
        {
            TermsOutcome.Ok => Ok(result.Status),
            TermsOutcome.UnknownApplication => Code(StatusCodes.Status404NotFound, UnknownApplicationCode),
            TermsOutcome.Stale => StatusCode(StatusCodes.Status409Conflict, new { message = StaleCode, currentVersion = result.CurrentVersion }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };

        private ObjectResult Code(int statusCode, string code) => StatusCode(statusCode, new { message = code });
    }
}
