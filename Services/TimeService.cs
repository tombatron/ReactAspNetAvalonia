using System;

namespace ReactAspNetAvalonia.Services;

public class TimeService : ITimeService
{
    public long GetEpochTime() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}