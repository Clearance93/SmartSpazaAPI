using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartCore.UnitOfWork;
using SmartInterfaces.IRepository;
using SmartInterfaces.IRepository.IBusiness;
using SmartInterfaces.IRepository.IDelivery;
using SmartInterfaces.IRepository.IDriver;
using SmartInterfaces.IService;
using SmartModels;
using SmartRepository;
using SmartRepository.Business;
using SmartRepository.Deliveries;
using SmartRepository.Driver;
using SmartServices;

namespace SmartUtility
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationDependancies(this IServiceCollection services, 
                                                                         IConfiguration configuration)
        {
            services.AddHttpClient();

            services.AddSingleton(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var configExpression = new MapperConfigurationExpression();
                configExpression.AddProfile<SmartCore.AutoMapper>();

                var configuration = new MapperConfiguration(configExpression, loggerFactory);

                return configuration;
            });

            services.AddSingleton<IMapper>(provider =>
            {
                var configuration = provider.GetRequiredService<MapperConfiguration>();

                return new Mapper(configuration);
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAvailabilityRepository, AvailabilityRepository>();
            services.AddScoped<IDriverAvailabilityRepository, DriverAvailabilityRepository>();
            services.AddScoped<IDriverRepostory, DriverRepostory>();
            services.AddScoped<INavigationRepository, NavigationRepository>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IBusinessNotificationPreferenceRepository, BusinessNotificationPreferenceRepository>();
            services.AddScoped<IBusinessProfileRepository, BusinessProfileRepository>();
            services.AddScoped<IBusinesTeamMembersRepository, BusinessTeamMembersRepository>();
            services.AddScoped<IOperatingHoursRepository, OperatingHoursRepository>();
            services.AddScoped<IShopOwnerRepository, ShopOwnerRepository>();
            services.AddScoped<ISupplierBusinessInfoRepository, SupplierBusinessInfoRepository>();

            services.AddScoped<ISupplierBusinessNotofocationRepository, SupplierBusinessNotofocationRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<ISystemNotificationRepository, SystemNotificationRepository>();
            services.AddScoped<IDeliveryConfigurationRepository, DeliveryConfigurationRepository>();
            services.AddScoped<IDeliveryRepository, DeliveryRepository>();
            services.AddScoped<IDeliveryItemsRepository, DeliveryItemsRepository>();
            services.AddScoped<IDeliveryPreferenceRepository, DeliveryPreferenceRepository>();
            services.AddScoped<IDeliveryRecordRepository, DeliveryRecordRepository>();
            services.AddScoped<IDeliveryTrackingRepository, DeliveryTrackingRepository>();

            services.AddScoped<IDriverAvailabilityRepository, DriverAvailabilityRepository>();
            services.AddScoped<IDriverRepostory, DriverRepostory>();
            services.AddScoped<INavigationRepository, NavigationRepository>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}