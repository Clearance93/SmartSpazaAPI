// Temporary helper to run AssertConfigurationIsValid
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AutoMapper;
using System;

class Temp
{
    public static void Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
        try
        {
            mapper.ConfigurationProvider.AssertConfigurationIsValid();
            Console.WriteLine("AutoMapper configuration is valid.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("AutoMapper configuration invalid: " + ex.Message);
            Console.WriteLine(ex);
        }
    }
}
