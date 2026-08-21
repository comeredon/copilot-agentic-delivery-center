using DeliveryControlCenter.Application.Abstractions;
using DeliveryControlCenter.Contracts.Realtime;
using DeliveryControlCenter.Domain.Audit;
using DeliveryControlCenter.Domain.Common;

namespace DeliveryControlCenter.Application.Delivery;

/// <summary>
/// Applies the human authorization signal. This is the only path to
/// <c>AwaitingHumanAuthorization</c>, and it refuses any actor that is not the repository
/// owner, so an agent can never authorize its own work.
/// </summary>
public sealed class AuthorizeWorkItemHandler(
    IWorkItemRepository workItems,
    IUnitOfWork unitOfWork,
    IDeliveryCache cache,
    IDeliveryNotifier notifier,
    IAuditLog auditLog)
{
    public async Task<bool> HandleAsync(AuthorizeWorkItemCommand command, CancellationToken cancellationToken)
    {
        var item = await workItems.FindByIdAsync(command.WorkItemId, cancellationToken)
            ?? throw new DomainException($"Work item {command.WorkItemId} was not found.");

        if (!command.ActorIsRepositoryOwner)
        {
            await auditLog.RecordAsync(
                AuditEvent.Blocked(
                    command.Actor,
                    "workitem.authorize",
                    $"workitem:{item.Number}",
                    "Only the repository owner may authorize a merge."),
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return false;
        }

        item.AuthorizeByHuman();

        await auditLog.RecordAsync(
            AuditEvent.Allowed(command.Actor, "workitem.authorize", $"workitem:{item.Number}", "authorized"),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.InvalidateAsync(CacheKeys.Overview, cancellationToken);
        await cache.InvalidateAsync(CacheKeys.WorkItemList, cancellationToken);
        await cache.InvalidateAsync(CacheKeys.WorkItemDetail(item.Id), cancellationToken);
        await notifier.NotifyAsync(DeliveryHubContract.WorkItemChanged, item.Id, cancellationToken);

        return true;
    }
}

public sealed record AuthorizeWorkItemCommand(Guid WorkItemId, string Actor, bool ActorIsRepositoryOwner);
