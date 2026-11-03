using AutoMapper;
using SmartDto;
using SmartDto.Business;
using SmartDto.Delivery;
using SmartDto.Driver;
using SmartDto.Inventory;
using SmartModels;

namespace SmartCore
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            CreateMap<ApplicationUser, UserDto>().ReverseMap(); 

            CreateMap<Availability, AvailabilityDto>()
                .ReverseMap();

            CreateMap<BusinessNotificationPreference, BusinessNotificationPreferenceDto>()
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.BusinessNotificationPreferenceId, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<BusinessProfile, BusinessProfileDto>()
                .ForMember(dest => dest.BusinessProfileId, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<BusinessTeamMembers, BusinessTeamMembersDto>()
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.BusinessTeamMembersId, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Delivery, DeliveryDto>()
                .ReverseMap();

            CreateMap<DeliveryConfiguration, DeliveryConfigurationDto>()
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryConfigurationId, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<DeliveryItems, DeliveryItemsDto>()
                .ReverseMap();

            CreateMap<DeliveryRatings, DeliveryRatingsDto>()
                .ReverseMap();

            CreateMap<DeliveryTracking, DeliveryTrackingDto>();

            CreateMap<DeliveryTrackingDto, DeliveryTracking>()
                .ForMember(dest => dest.DeliveryTrackingId, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.Updated, opt => opt.Ignore());

            CreateMap<Drivers, DriversDto>()
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.DriverId, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<DriversAvailability, DriverAvailabilityDto>()
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.DriversAvailabilityId, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Navigation, NavigationDto>()
                .ReverseMap();

            CreateMap<OperatingHours, OperatingHoursDto>()
                .ForMember(dest => dest.OperatingHoursId, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<SupplierBusinessInfo, SupplierBusinessInfoDto>()
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierBusinessInfoId, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<SupplierBusinessNotification, SupplierBusinessNotificationDto>()
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierBusinessNotificationId, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<SystemNotification, SystemNotifiationDto>()
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.SystemNotificationId, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Vehicle, VehicleDto>()
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.Vid, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<ShopsOwner, ShopOwnerDto>()
                .ReverseMap();

            CreateMap<Supplier, SupplierDto>()
                .ReverseMap();

            CreateMap<UserDto, Drivers>()
                .ForMember(dest => dest.DriverId, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumbers, opt => opt.MapFrom(src => src.WhatsAppNumber))
                .ReverseMap();

            CreateMap<UserDto, Supplier>()
                .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
                .ForMember(dest => dest.ContactNumber, opt => opt.MapFrom(src => src.WhatsAppNumber))
                .ReverseMap();

            CreateMap<UserDto, ShopsOwner>()
                .ForMember(dest => dest.ShopOwnerId, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.WhatsAppNumber))
                .ReverseMap();

            CreateMap<RegisterDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            CreateMap<RegisterDto, Drivers>()
                .ForMember(dest => dest.DriverId, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumbers, opt => opt.MapFrom(src => src.WhatsAppNumber));

            CreateMap<RegisterDto, Supplier>()
                .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src =>
                    string.IsNullOrWhiteSpace(src.BusinessName) ? src.FullName : src.BusinessName))
                .ForMember(dest => dest.ContactName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.ContactNumber, opt => opt.MapFrom(src => src.WhatsAppNumber));

            CreateMap<RegisterDto, ShopsOwner>()
                .ForMember(dest => dest.ShopOwnerId, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.NameOfTheShops, opt => opt.MapFrom(src =>
                    string.IsNullOrWhiteSpace(src.BusinessName) ? src.FullName : src.BusinessName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.WhatsAppNumber));

            CreateMap<Product, ProductDto>()
                .ReverseMap();
        }
    }
}
