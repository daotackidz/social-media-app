using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Social.WebApi.Infrastructure.Services
{
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;

        public AuditSaveChangesInterceptor(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            SetCreatedBy(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            SetCreatedBy(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void SetCreatedBy(DbContext? context)
        {
            if (context == null) return;

            var userId = _currentUserService?.UserId ?? Guid.Empty;

            foreach (var entry in context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
            {
                var prop = entry.Metadata.FindProperty("CreatedByUserId");
                if (prop != null)
                {
                    var current = entry.CurrentValues[prop.Name];
                    if (current == null || (current is Guid g && g == Guid.Empty))
                    {
                        entry.CurrentValues[prop.Name] = userId;
                    }
                }
            }
        }
    }
}
