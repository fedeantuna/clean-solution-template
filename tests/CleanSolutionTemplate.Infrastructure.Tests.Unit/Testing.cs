﻿using System.Diagnostics.CodeAnalysis;

namespace CleanSolutionTemplate.Infrastructure.Tests.Unit;

[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
public static class Testing
{
    public static readonly string TestUserId = "test-user-id";
    public static readonly string TestUserEmail = "test-user-email";

    public static DateTimeOffset UtcNow { get; } = DateTimeOffset.UtcNow;
}
