using AIDMS.Shared.Application.Configurations;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace AIDMS.Server.Extensions
{
    internal static class MvcBuilderExtensions
    {
        internal static IMvcBuilder AddValidators(this IMvcBuilder builder)
        {
            builder.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<AppConfiguration>());
            return builder;
        }
    }
}
