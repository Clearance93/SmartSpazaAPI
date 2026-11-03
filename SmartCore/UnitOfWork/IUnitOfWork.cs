using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using SmartInterfaces.IRepository;
using SmartInterfaces.IRepository.IBusiness;
using SmartInterfaces.IRepository.IDelivery;
using SmartInterfaces.IRepository.IDriver;
using SmartInterfaces.IRepository.IInventory;

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

        IShopOwnerRepository ShopOwner { get; }

        ISupplierBusinessInfoRepository SupplierBusinessInfo { get; }

        ISupplierBusinessNotofocationRepository SupplierBusinessNotofocation { get; }

        ISystemNotificationRepository SystemNotification { get; }

        IDeliveryConfigurationRepository DeliveryConfiguration { get; }

        IDeliveryItemsRepository DeliveryItems { get; }

        IDeliveryRepository Delivery { get; }

        IDeliveryRatingsRepository DeliveryRatings { get; }

        IDeliveryTrackingRepository DeliveryTracking { get; }

        IDriverAvailabilityRepository AvailabilityRepository { get; }

        IDriverRepostory Driver { get; }

        INavigationRepository Navigation { get; }

        IVehicleRepository Vehicle { get; }

        ISupplierRepository Supplier { get; }

        IProductRepository Product { get; }

        IRefreshTokenRepository RefreshToken { get; }

        Task<int> SaveChangesAsync();

        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    }
}
