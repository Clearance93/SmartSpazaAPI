using SmartData;
using SmartInterfaces.IRepository;
using SmartInterfaces.IRepository.IBusiness;
using SmartInterfaces.IRepository.IDelivery;
using SmartInterfaces.IRepository.IDriver;

namespace SmartCore.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _Context;

        public IAvailabilityRepository Availability { get; private set; }

        public IUserRepository User { get; private set; }

        public IBusinessNotificationPreferenceRepository BusinessNotificationPreference { get; private set; }

        public IBusinessProfileRepository BusinessProfile { get; private set; }

        public IBusinesTeamMembersRepository BusinesTeamMembers { get; private set; }

        public IOperatingHoursRepository OperatingHours { get; private set; }

        public ISupplierBusinessInfoRepository SupplierBusinessInfo { get; private set; }

        public ISupplierBusinessNotofocationRepository SupplierBusinessNotofocation { get; private set; }

        public ISystemNotificationRepository SystemNotification { get; private set; }

        public IDeliveryConfigurationRepository EverythingConfiguration { get; private set; }

        public IDeliveryItemsRepository DeliveryItems { get; private set; }

        public IDeliveryPreferenceRepository DeliveryPreference { get; private set; }

        public IDeliveryRecordRepository DeliveryRecord { get; private set; }

        public IDeliveryTrackingRepository DeliveryTracking { get; private set; }

        public IDriverAvailabilityRepository AvailabilityRepository { get; private set; }

        public IDriverRepostory Driver { get; private set; }

        public INavigationRepository Navigation { get; private set; }

        public IVehicleRepository Vehicle { get; private set; }

        public IShopOwnerRepository ShopOwner { get; private set; }

        public ISupplierRepository Supplier { get; private set; }

        public UnitOfWork(ApplicationDbContext context,
                          IAvailabilityRepository availability,
                          IUserRepository userRepository,
                          IBusinessNotificationPreferenceRepository businessNotificationPreferenceRepository,
                          IBusinessProfileRepository businessProfileRepository,
                          IBusinesTeamMembersRepository businesTeamMembersRepository,
                          IOperatingHoursRepository operatingHoursRepository,
                          ISupplierBusinessInfoRepository supplierBusinessInfoRepository,
                          ISupplierBusinessNotofocationRepository supplierBusinessNotofocationRepository,
                          ISystemNotificationRepository systemNotificationRepository,
                          IDeliveryConfigurationRepository everythingConfigurationRepository,
                          IDeliveryItemsRepository deliveryItemsRepository,
                          IDeliveryPreferenceRepository deliveryPreferenceRepository,
                          IDeliveryRecordRepository deliveryRecordRepository,
                          IDeliveryTrackingRepository deliveryTrackingRepository,
                          IDriverAvailabilityRepository availabilityRepository,
                          IDriverRepostory driverRepository,
                          INavigationRepository navigationRepository,
                          IVehicleRepository vehicleRepository,
                          IShopOwnerRepository shopOwnerRepository,
                          ISupplierRepository supplierRepository)
        {
            _Context = context;
            Availability = availability ?? throw new ArgumentNullException(nameof(availability));
            User = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            BusinessNotificationPreference = businessNotificationPreferenceRepository ?? throw new ArgumentNullException(nameof(businessNotificationPreferenceRepository));
            BusinessProfile = businessProfileRepository ?? throw new ArgumentNullException(nameof(businessProfileRepository));
            BusinesTeamMembers = businesTeamMembersRepository ?? throw new ArgumentNullException(nameof(businesTeamMembersRepository));
            OperatingHours = operatingHoursRepository ?? throw new ArgumentNullException(nameof(operatingHoursRepository));
            SupplierBusinessInfo = supplierBusinessInfoRepository ?? throw new ArgumentNullException(nameof(supplierBusinessInfoRepository));
            SupplierBusinessNotofocation = supplierBusinessNotofocationRepository ?? throw new ArgumentNullException(nameof(supplierBusinessNotofocationRepository));
            SystemNotification = systemNotificationRepository ?? throw new ArgumentNullException(nameof(systemNotificationRepository));
            EverythingConfiguration = everythingConfigurationRepository ?? throw new ArgumentNullException(nameof(everythingConfigurationRepository));
            DeliveryItems = deliveryItemsRepository ?? throw new ArgumentNullException(nameof(deliveryItemsRepository));
            DeliveryPreference = deliveryPreferenceRepository ?? throw new ArgumentNullException(nameof(deliveryPreferenceRepository));
            DeliveryRecord = deliveryRecordRepository ?? throw new ArgumentNullException(nameof(deliveryRecordRepository));
            DeliveryTracking = deliveryTrackingRepository ?? throw new ArgumentNullException(nameof(deliveryTrackingRepository));
            AvailabilityRepository = availabilityRepository ?? throw new ArgumentNullException(nameof(availabilityRepository));
            Driver = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            Navigation = navigationRepository ?? throw new ArgumentNullException(nameof(navigationRepository));
            Vehicle = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
            ShopOwner = shopOwnerRepository ?? throw new ArgumentNullException(nameof(shopOwnerRepository));
            Supplier = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));
        }

        public void Dispose()
        {
            _Context.Dispose();
        }

        public async Task<int> SaveChangesAsyc()
        {
            return await _Context.SaveChangesAsync();
        }
    }
}
