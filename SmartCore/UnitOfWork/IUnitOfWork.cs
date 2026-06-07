using SmartInterfaces.IRepository;
using SmartInterfaces.IRepository.IBusiness;
using SmartInterfaces.IRepository.IDelivery;
using SmartInterfaces.IRepository.IDriver;

namespace SmartCore.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IAvailabilityRepository Availability { get; }

        IUserRepository User { get; }

        IBusinessNotificationPreferenceRepository BusinessNotificationPreference { get; }

        IBusinessProfileRepository BusinessProfile { get; }

        IBusinesTeamMembersRepository BusinesTeamMembers { get; }

        IOperatingHoursRepository OperatingHours { get; }

        ISupplierBusinessInfoRepository SupplierBusinessInfo { get; }

        ISupplierBusinessNotofocationRepository SupplierBusinessNotofocation { get; }

        ISystemNotificationRepository SystemNotification { get; }

        IDeliveryConfigurationRepository EverythingConfiguration { get; }

        IDeliveryItemsRepository DeliveryItems { get; }

        IDeliveryPreferenceRepository DeliveryPreference { get; }

        IDeliveryRecordRepository DeliveryRecord { get; }

        IDeliveryTrackingRepository DeliveryTracking { get; }

        IDriverAvailabilityRepository AvailabilityRepository { get; }

        IDriverRepostory Driver { get; }

        INavigationRepository Navigation { get; }

        IVehicleRepository Vehicle { get; }

        IShopOwnerRepository ShopOwner { get; }

        ISupplierRepository Supplier { get; }

        Task<int> SaveChangesAsyc();
    }
}
