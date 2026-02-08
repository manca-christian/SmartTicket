using System;

namespace SmartTicket.API.Swagger;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequireIfMatchAttribute : Attribute;
