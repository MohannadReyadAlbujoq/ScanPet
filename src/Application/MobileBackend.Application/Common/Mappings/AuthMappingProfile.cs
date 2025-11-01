using AutoMapper;
using MobileBackend.Application.DTOs.Auth;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile for Authentication-related mappings
/// Maps between Domain entities and DTOs for Auth operations
/// </summary>
public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        // User -> UserInfoDto (used in LoginResponse)
        CreateMap<User, UserInfoDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore()) // Set manually in handler
            .ForMember(dest => dest.PermissionsBitmask, opt => opt.Ignore()); // Set manually in handler

        // RegisterRequestDto -> User
        CreateMap<RegisterRequestDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Generated in handler
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Hashed in handler
            .ForMember(dest => dest.IsEnabled, opt => opt.MapFrom(src => false)) // Always false for new users
            .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => false)) // Always false for new users
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Set in handler
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore());

        // RefreshToken -> (no direct DTO mapping needed, handled in handler)
    }
}
