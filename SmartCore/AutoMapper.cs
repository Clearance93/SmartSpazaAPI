using AutoMapper;
using SmartDto;
using SmartDto.Business;
using SmartDto.Delivery;
using SmartDto.Driver;
using SmartModels;

namespace SmartCore
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            CreateMap<ApplicationUser, UserDto>().ReverseMap(); 

            CreateMap<Availability, AvailabilityDto>()
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.AvailabilityId, opt => opt.Ignore())
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
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.DriverId, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<DeliveryConfiguration, DeliveryConfigurationDto>()
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryConfigurationId, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<DeliveryItems, DeliveryItemsDto>()
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryItemId, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<DeliveryPreference, DeliveryPreferenceDto>()
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryPreferenceId, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<DeliveryPreference, DeliveryPreferenceDto>()
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryPreferenceId, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<DeliveryRecord, DeliveryRecordDto>()
                .ForMember(dest => dest.DeliveryRecordId, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<DeliveryTracking, DeliveryTrackingDto>()
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryTrackingId, opt => opt.Ignore())
                .ReverseMap();

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
                .ForMember(dest => dest.Updated, opt => opt.Ignore())
                .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
                .ForMember(dest => dest.NavigationId, opt => opt.Ignore())
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
				.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
				.ForMember(dest => dest.ShopOnwerId, opt => opt.Ignore())
				.ReverseMap();

            CreateMap<Supplier, SupplierDto>()
				.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
				.ForMember(dest => dest.SupplierId, opt => opt.Ignore())
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
        }
    }
}
