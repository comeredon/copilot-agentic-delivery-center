using DeliveryControlCenter.Domain.Sync;

namespace DeliveryControlCenter.UnitTests.Domain;

public sealed class SyncCursorTests
{
    [Fact]
    public void RecordFailure_DeadLettersAfterFiveConsecutiveFailures()
    {
        var cursor = SyncCursor.ForFeed(SyncFeed.Issues);

        for (var i = 0; i < 4; i++)
        {
            cursor.RecordFailure("HttpRequestException");
        }

        cursor.IsDeadLettered.ShouldBeFalse();

        cursor.RecordFailure("HttpRequestException");

        cursor.IsDeadLettered.ShouldBeTrue();
    }

    [Fact]
    public void RecordSuccess_ClearsTheFailureStreak()
    {
        var cursor = SyncCursor.ForFeed(SyncFeed.PullRequests);
        cursor.RecordFailure("boom");
        cursor.RecordFailure("boom");

        cursor.RecordSuccess(DateTimeOffset.UtcNow, "\"etag\"");

        cursor.ConsecutiveFailures.ShouldBe(0);
        cursor.LastError.ShouldBeNull();
        cursor.ETag.ShouldBe("\"etag\"");
    }
}
