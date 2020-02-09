using Microsoft.Extensions.DependencyInjection;

namespace SharpRabbit
{
    public static class Registration
    {
        public static IServiceCollection AddSharpRabbit(this IServiceCollection owner)
        {
            owner.AddSingleton<IRabbitConnection, RabbitConnection>();

            return owner;
        }
    }
}
