using System;

namespace ValidationScoringService.Api.Infrastructure;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SystemTextJsonFormatterAttribute : Attribute
{
}
