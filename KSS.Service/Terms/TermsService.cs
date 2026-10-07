#nullable enable
using KSS.Data.DbContexts;
using KSS.Dto;
using KSS.Entity;
using Microsoft.EntityFrameworkCore;

namespace KSS.Service.Terms
{
    public enum TermsOutcome
    {
        Ok,
        UnknownApplication,
        Stale
    }

    public sealed record TermsResult(TermsOutcome Outcome, TermsStatus? Status = null, string? CurrentVersion = null);

    /// <summary>
    /// Reads and records a person's acceptance of an application's current terms.
    /// It uses the context directly rather than a generic repository: the generic add/update/remove
    /// surface is exactly what this table must not expose. Rows are inserted once and never changed.
    /// </summary>
    public sealed class TermsService
    {
        private readonly MainDbContext _db;
        private readonly TermsOptions _options;
        private readonly TimeProvider _time;

        public TermsService(MainDbContext db, TermsOptions options, TimeProvider time)
        {
            _db = db;
            _options = options;
            _time = time;
        }

        public async Task<TermsResult> GetAsync(Guid personId, string applicationKey, CancellationToken cancellationToken = default)
        {
            var application = await ResolveAsync(applicationKey, cancellationToken);
            if (application is null)
                return new TermsResult(TermsOutcome.UnknownApplication);

            var (key, version, moduleId) = application.Value;
            var row = await FindAsync(personId, moduleId, version, cancellationToken);
            return new TermsResult(TermsOutcome.Ok, new TermsStatus(key, version, row is not null, row?.AcceptedAt));
        }

        /// <summary>
        /// Idempotent: a repeat or concurrent accept returns the original acceptance time, and there is one row.
        /// </summary>
        public async Task<TermsResult> AcceptAsync(Guid personId, string applicationKey, string version, CancellationToken cancellationToken = default)
        {
            var application = await ResolveAsync(applicationKey, cancellationToken);
            if (application is null)
                return new TermsResult(TermsOutcome.UnknownApplication);

            var (key, currentVersion, moduleId) = application.Value;

            // Only the current version can be accepted; nothing is written otherwise.
            if (!string.Equals(version, currentVersion, StringComparison.Ordinal))
                return new TermsResult(TermsOutcome.Stale, CurrentVersion: currentVersion);

            var existing = await FindAsync(personId, moduleId, currentVersion, cancellationToken);
            if (existing is not null)
                return Accepted(key, currentVersion, existing);

            var row = new TermsAcceptance
            {
                Id = Guid.CreateVersion7(),
                PersonId = personId,
                ModuleId = moduleId,
                TermsVersion = currentVersion,
                AcceptedAt = _time.GetUtcNow().UtcDateTime,
                CreatedBy = personId
            };
            _db.TermsAcceptances.Add(row);

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // A concurrent accept may have inserted the same (person, module, version) between the
                // check above and this save; the unique key refuses the second row. Re-read: if the row
                // now exists, return it. Otherwise this is a genuine failure and stays one.
                _db.Entry(row).State = EntityState.Detached;
                var winner = await FindAsync(personId, moduleId, currentVersion, cancellationToken);
                if (winner is null)
                    throw;
                return Accepted(key, currentVersion, winner);
            }

            return Accepted(key, currentVersion, row);
        }

        private static TermsResult Accepted(string key, string version, TermsAcceptance row) =>
            new(TermsOutcome.Ok, new TermsStatus(key, version, true, row.AcceptedAt));

        /// <summary>
        /// An application is enabled only when it has a configured version AND an active, not deleted Module row.
        /// The key is matched trimmed and case-insensitively; the canonical lowercase code is returned.
        /// </summary>
        private async Task<(string Key, string Version, Guid ModuleId)?> ResolveAsync(string applicationKey, CancellationToken cancellationToken)
        {
            var key = applicationKey.Trim().ToLowerInvariant();
            if (!_options.CurrentVersions.TryGetValue(key, out var version))
                return null;

            var moduleId = await _db.Modules
                .AsNoTracking()
                .Where(m => m.Code == key && m.IsActive && m.DeletedAt == null)
                .Select(m => (Guid?)m.Id)
                .FirstOrDefaultAsync(cancellationToken);

            return moduleId is null ? null : (key, version, moduleId.Value);
        }

        private Task<TermsAcceptance?> FindAsync(Guid personId, Guid moduleId, string version, CancellationToken cancellationToken) =>
            _db.TermsAcceptances
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PersonId == personId && x.ModuleId == moduleId && x.TermsVersion == version, cancellationToken);
    }
}
