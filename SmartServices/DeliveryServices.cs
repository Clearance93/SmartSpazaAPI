using AutoMapper;
using SmartConfiguration;
using SmartCore.UnitOfWork;
using SmartDto.Business;
using SmartDto.Delivery;
using SmartEnums;
using SmartInterfaces.IService;
using SmartModels;

namespace SmartServices
{
    public class DeliveryServices : IDeliveryServices
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IMapper _Mapper;

        public DeliveryServices(IUnitOfWork unitOfWork,
                                IMapper mapper)
        {
            _UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> CreateDeliveryAsync(DeliveryDto dto)
        {
            var driver = await _UnitOfWork.Driver.GetByIdAsync(dto.DriverId);

            if (driver == null)
            {
                throw new NotFoundException($"The driver with the Id: {dto.DriverId} does not exist.");
            }

            var supplier = await _UnitOfWork.Supplier.GetByIdAsync(dto.SupplierId);

            if (supplier == null)
            {
                throw new NotFoundException($"The supplier with the Id: {dto.SupplierId} does not exist.");
            }

            var shop = await _UnitOfWork.ShopOwner.GetByIdAsync(dto.ShopId);

            if (shop == null)
            {
                throw new NotFoundException($"The shop with the Id: {dto.ShopId} does not exist.");
            }

            EnsureStatusDefined(dto.DeliveryStatus);

            var delivery = _Mapper.Map<Delivery>(dto);

            delivery.DeliveryId = Guid.NewGuid();
            delivery.DeliveryStatus = dto.DeliveryStatus == default ? DeliveryStatus.Created : dto.DeliveryStatus;
            delivery.DeliveryNumber = string.IsNullOrWhiteSpace(dto.DeliveryNumber)
                ? Delivery.NewDeliveryNumber()
                : dto.DeliveryNumber;
            delivery.CreationTime = DateTime.UtcNow;

            await _UnitOfWork.Delivery.AddAsync(delivery);
            await _UnitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpsertAsync(DeliveryRatingsDto dto, Guid shopId, Guid id = default)
        {
            EnsureRatingValue(dto.Ratings);

            if (id == Guid.Empty)
            {
                var delivery = await _UnitOfWork.Delivery.GetByIdAsync(dto.DeliveryId);
                var driver = await _UnitOfWork.Driver.GetByIdAsync(dto.DriverId);

                if (delivery == null || driver == null)
                {
                    throw new NotFoundException("The delivery or driver for this rating was not found.");
                }

                if (delivery.ShopId != shopId || delivery.DriverId != dto.DriverId)
                {
                    throw new ForbiddenException("You are not allowed to rate this delivery.");
                }

                var deliveryRating = _Mapper.Map<DeliveryRatings>(dto);

                deliveryRating.DeliveryRatingId = Guid.NewGuid();
                deliveryRating.ShopId = shopId;
                deliveryRating.Created = DateTime.UtcNow;

                await _UnitOfWork.DeliveryRatings.AddAsync(deliveryRating);
                await _UnitOfWork.SaveChangesAsync();

                return true;
            }

            var existingRating = await _UnitOfWork.DeliveryRatings.GetByIdAsync(id);

            if (existingRating == null)
            {
                throw new NotFoundException($"No ratings found with Id: {id}");
            }

            if (existingRating.ShopId != shopId)
            {
                throw new ForbiddenException("You are not allowed to update this rating.");
            }

            existingRating.Ratings = dto.Ratings;
            existingRating.Comments = dto.Comments;
            existingRating.Updated = DateTime.UtcNow;

            _UnitOfWork.DeliveryRatings.Update(existingRating);
            await _UnitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CreateTrackingAsync(DeliveryTrackingDto dto)
        {
            var driver = await _UnitOfWork.Driver.GetByIdAsync(dto.DriverId);

            if (driver == null)
            {
                throw new NotFoundException($"The driver with the Id: {dto.DriverId} was not found.");
            }

            if (dto.DeliveryId.HasValue)
            {
                var delivery = await _UnitOfWork.Delivery.GetByIdAsync(dto.DeliveryId.Value);

                if (delivery == null)
                {
                    throw new NotFoundException($"The delivery with Id: {dto.DeliveryId} was not found.");
                }

                if (delivery.DriverId != dto.DriverId)
                {
                    throw new ForbiddenException("The delivery is assigned to another driver.");
                }
            }

            EnsureStatusDefined(dto.DeliveryStatus);

            var deliveryTracking = _Mapper.Map<DeliveryTracking>(dto);

            deliveryTracking.DeliveryTrackingId = Guid.NewGuid();
            deliveryTracking.DeliveryId = dto.DeliveryId;
            deliveryTracking.DeliveryStatus = dto.DeliveryStatus == default ? DeliveryStatus.Accepted : dto.DeliveryStatus;
            deliveryTracking.CreationTime = DateTime.UtcNow;

            await _UnitOfWork.DeliveryTracking.AddAsync(deliveryTracking);
            await _UnitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<DeliveryDto>> GetAllDeliveriesForSupplierAsync(Guid supplierId)
        {
            var deliveries = await _UnitOfWork.Delivery.GetBySupplierIdAsync(supplierId);

            return _Mapper.Map<IEnumerable<DeliveryDto>>(deliveries);
        }

        public async Task<IEnumerable<DeliveryDto>> GetAllDeliveryByDriverIdAsync(Guid driverId)
        {
            var driver = await _UnitOfWork.Driver.GetByIdAsync(driverId);

            if (driver == null)
            {
                throw new NotFoundException($"The driver with Id: {driverId} was not found.");
            }

            var driverDeliveries = await _UnitOfWork.Delivery.GetActiveByDriverIdAsync(driverId);

            return _Mapper.Map<IEnumerable<DeliveryDto>>(driverDeliveries);
        }

        public async Task<IEnumerable<DeliveryRatingsDto>> GetAllDeliveryRecordsAsync(Guid driverId)
        {
            var driver = await _UnitOfWork.Driver.GetByIdAsync(driverId);

            if (driver == null)
            {
                throw new NotFoundException($"There is no driver with an id: {driverId}");
            }

            var ratings = await _UnitOfWork.DeliveryRatings.GetByDriverIdAsync(driverId);

            return _Mapper.Map<IEnumerable<DeliveryRatingsDto>>(ratings);
        }

        public async Task<DeliveryDto> GetDeliveryByIdAsync(Guid deliveryId)
        {
            var delivery = await _UnitOfWork.Delivery.GetByIdAsync(deliveryId);

            if (delivery == null)
            {
                throw new NotFoundException($"The delivery with id: {deliveryId} was not found.");
            }

            return _Mapper.Map<DeliveryDto>(delivery);
        }

        public async Task<ShopOwnerDto> GetShopOwnerByIdAsync(Guid shopId)
        {
            var shop = await _UnitOfWork.ShopOwner.GetByIdAsync(shopId);

            if (shop == null)
            {
                throw new NotFoundException($"The delivery shop with id: {shopId} was not found.");
            }

            return _Mapper.Map<ShopOwnerDto>(shop);
        }

        public async Task<SupplierDto> GetSupplierByIdAsync(Guid supplierId)
        {
            var supplier = await _UnitOfWork.Supplier.GetByIdAsync(supplierId);

            if (supplier == null)
            {
                throw new NotFoundException($"The supplier with id: {supplierId} was not found.");
            }

            return _Mapper.Map<SupplierDto>(supplier);
        }

        public async Task<DeliveryTrackingDto> GetTrackingAsync(Guid trackingId)
        {
            var tracking = await _UnitOfWork.DeliveryTracking.GetByIdAsync(trackingId);

            if (tracking == null)
            {
                throw new NotFoundException($"The tracking with the id: {trackingId} was not found.");
            }

            return _Mapper.Map<DeliveryTrackingDto>(tracking);
        }

        public async Task<IEnumerable<DeliveryTrackingDto>> GetTrackingByDriverAsync(Guid driverId)
        {
            var tracking = await _UnitOfWork.DeliveryTracking.GetTrackingByDriverAsync(driverId);

            return _Mapper.Map<IEnumerable<DeliveryTrackingDto>>(tracking);
        }

        public async Task<bool> UpdateDeliveryAsync(Guid id, DeliveryDto dto)
        {
            var delivery = await _UnitOfWork.Delivery.GetByIdAsync(id);

            if (delivery == null)
            {
                throw new NotFoundException($"The delivery with Id: {id} was not found.");
            }

            EnsureStatusDefined(dto.DeliveryStatus, allowDefault: false);

            delivery.DeliveryStatus = dto.DeliveryStatus;
            delivery.Priority = dto.Priority;
            delivery.ScheduledTime = dto.ScheduledTime;
            delivery.EstimatedArrival = dto.EstimatedArrival;
            delivery.ActualArrival = dto.ActualArrival;
            delivery.Notes = dto.Notes;
            delivery.Updated = DateTime.UtcNow;

            _UnitOfWork.Delivery.Update(delivery);
            await _UnitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateLocationAsync(Guid trackingId, DeliveryTrackingDto dto)
        {
            var tracking = await _UnitOfWork.DeliveryTracking.GetByIdAsync(trackingId);

            if (tracking == null)
            {
                throw new NotFoundException($"The tracking Id {trackingId} was not found.");
            }

            EnsureStatusDefined(dto.DeliveryStatus, allowDefault: false);

            tracking.DeliveryStatus = dto.DeliveryStatus;
            tracking.Latitude = dto.Latitude;
            tracking.Longitude = dto.Longitude;
            tracking.DistanceRemaining = dto.DistanceRemaining;
            tracking.EstimatedTime = dto.EstimatedTime;
            tracking.Updated = DateTime.UtcNow;

            _UnitOfWork.DeliveryTracking.Update(tracking);
            await _UnitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpsertDeliveryItemAsync(DeliveryItemsDto dto, Guid id = default)
        {
            var delivery = await _UnitOfWork.Delivery.GetByIdAsync(dto.DeliveryId);

            if (delivery == null)
            {
                throw new NotFoundException($"The delivery Id: {dto.DeliveryId} was not found.");
            }

            if (id == Guid.Empty)
            {
                var item = _Mapper.Map<DeliveryItems>(dto);

                item.DeliveryItemId = Guid.NewGuid();
                item.IsDeleted = false;
                item.CreationTime = DateTime.UtcNow;

                await _UnitOfWork.DeliveryItems.AddAsync(item);
                await _UnitOfWork.SaveChangesAsync();

                return true;
            }

            var deliveryItem = await _UnitOfWork.DeliveryItems.GetByIdAsync(id);

            if (deliveryItem == null)
            {
                throw new NotFoundException($"No delivery item with id: {id} was found.");
            }

            if (deliveryItem.DeliveryId != dto.DeliveryId)
            {
                throw new ForbiddenException("The delivery item does not belong to the supplied delivery.");
            }

            deliveryItem.Weight = dto.Weight;
            deliveryItem.Unit = dto.Unit;
            deliveryItem.Quantity = dto.Quantity;
            deliveryItem.StockKeepUnit = dto.StockKeepUnit;
            deliveryItem.ProductName = dto.ProductName;
            deliveryItem.Updated = DateTime.UtcNow;

            _UnitOfWork.DeliveryItems.Update(deliveryItem);
            await _UnitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<DeliveryItemsDto> GetSingleDeliveryItemAsync(Guid id)
        {
            var deliveryItem = await _UnitOfWork.DeliveryItems.GetByIdAsync(id);

            if (deliveryItem == null || deliveryItem.IsDeleted)
            {
                throw new NotFoundException($"No delivery item with id: {id} was found.");
            }

            return _Mapper.Map<DeliveryItemsDto>(deliveryItem);
        }

        public async Task<IEnumerable<DeliveryItemsDto>> GetAllDeliveryItemsByDeliveryId(Guid deliveryId)
        {
            var delivery = await _UnitOfWork.Delivery.GetByIdAsync(deliveryId);

            if (delivery == null)
            {
                throw new NotFoundException($"The delivery Id: {deliveryId} was not found.");
            }

            var deliveryItems = await _UnitOfWork.DeliveryItems.GetAllDeliveryItemsByDeliveryId(deliveryId);

            return _Mapper.Map<IEnumerable<DeliveryItemsDto>>(deliveryItems);
        }

        public async Task<bool> CanAccessShopAsync(Guid shopId, Guid userId)
        {
            var deliveries = await _UnitOfWork.Delivery.FindAsync(delivery =>
                delivery.ShopId == shopId && (delivery.SupplierId == userId || delivery.DriverId == userId));

            return deliveries.Any();
        }

        public async Task<bool> CanAccessSupplierAsync(Guid supplierId, Guid userId)
        {
            var deliveries = await _UnitOfWork.Delivery.FindAsync(delivery =>
                delivery.SupplierId == supplierId && (delivery.ShopId == userId || delivery.DriverId == userId));

            return deliveries.Any();
        }

        private static void EnsureStatusDefined(DeliveryStatus status, bool allowDefault = true)
        {
            if (status == default)
            {
                if (allowDefault)
                {
                    return;
                }

                throw new ValidationException("A delivery status is required.");
            }

            if (!Enum.IsDefined(status))
            {
                throw new ValidationException("The delivery status is not valid.");
            }
        }

        private static void EnsureRatingValue(decimal? rating)
        {
            if (rating is < 0 or > 5)
            {
                throw new ValidationException("A rating must be between 0 and 5.");
            }
        }
    }
}
