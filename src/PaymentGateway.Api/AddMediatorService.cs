﻿using FluentValidation;

using MediatR;

using PaymentGateway.Api.Application;

namespace PaymentGateway.Api
{
    public static class AddMediatorService
    {
        public static IServiceCollection AddMediator(this IServiceCollection services)
        {
            services.AddMediatR(x => x.RegisterServicesFromAssemblies((typeof(Program).Assembly)));
            services.AddApplication();

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddValidatorsFromAssembly(typeof(AddMediatorService).Assembly);
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
            return services;
        }
    }
}
