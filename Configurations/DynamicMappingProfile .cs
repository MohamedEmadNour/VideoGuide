using AutoMapper;
using System.Linq.Expressions;

namespace VideoGuide.Configurations
{
    using AutoMapper;
    using System;
    using System.Linq.Expressions;

    public class DynamicMappingProfile : Profile
    {
        public DynamicMappingProfile(Type sourceType, Type destinationType)
        {
            CreateMap(sourceType, destinationType).ReverseMap();

            // Get all properties of the source and destination types
            var sourceProperties = sourceType.GetProperties();
            var destinationProperties = destinationType.GetProperties();

            // Map properties with matching names and types
            foreach (var sourceProperty in sourceProperties)
            {
                var destinationProperty = destinationProperties.FirstOrDefault(p => p.Name == sourceProperty.Name && p.PropertyType == sourceProperty.PropertyType);
                if (destinationProperty != null)
                {
                    CreateMap(sourceType, destinationType, MemberList.None)
                        .ForMember(destinationProperty.Name, opt => opt.MapFrom(sourceProperty.Name));
                }
            }
        }
    }
}
