using System;

namespace BusinessService.Api.Infrastructure;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SystemTextJsonFormatterAttribute : Attribute
{
}
