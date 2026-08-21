using DeliveryControlCenter.Application.Abstractions;

namespace DeliveryControlCenter.UnitTests.Application;

public sealed class CacheKeyTests
{
    [Fact]
    public void AllKeys_ShareTheVersionedPrefix()
    {
        // The version segment is what lets a schema change invalidate everything at once.
        CacheKeys.Overview.ShouldStartWith("dcc:v1:");
        CacheKeys.WorkItemList.ShouldStartWith("dcc:v1:");
        CacheKeys.SyncHealth.ShouldStartWith("dcc:v1:");
        CacheKeys.WorkItemDetail(Guid.Empty).ShouldStartWith("dcc:v1:");
    }

    [Fact]
    public void WorkItemDetail_IsStableAndUnique()
    {
        var a = Guid.CreateVersion7();
        var b = Guid.CreateVersion7();

        CacheKeys.WorkItemDetail(a).ShouldBe(CacheKeys.WorkItemDetail(a));
        CacheKeys.WorkItemDetail(a).ShouldNotBe(CacheKeys.WorkItemDetail(b));
    }
}
