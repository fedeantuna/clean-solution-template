using System.Diagnostics.CodeAnalysis;
using CleanSolutionTemplate.Application.Common.Wrappers;

namespace CleanSolutionTemplate.Infrastructure.Wrappers;

[ExcludeFromCodeCoverage]
public class DateTimeOffsetWrapper : IDateTimeOffsetWrapper
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
