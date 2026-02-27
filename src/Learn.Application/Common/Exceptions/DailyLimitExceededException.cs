namespace Learn.Application.Common.Exceptions;

public class DailyLimitExceededException : Exception
{
    public DateTime ResetTime { get; }

    public DailyLimitExceededException(DateTime resetTime)
        : base("Daily submission limit exceeded.")
    {
        ResetTime = resetTime;
    }
}
