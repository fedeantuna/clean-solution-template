namespace CleanSolutionTemplate.Application.Common.Wrappers;

public interface IDateTimeOffsetWrapper
{
    DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
