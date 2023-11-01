using AutoMapper;
using VideoGuide.Data;
//using VideoGuide.Models;
//using VideoGuide.View_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoGuide.View_Model;

namespace VideoGuide.Configurations
{
    public class MapperInitilizer : Profile
    {
        public MapperInitilizer()
        {
            //CreateMap<Country, CountryDTO>().ReverseMap();
            //CreateMap<Country, CreateCountryDTO>().ReverseMap();
            //CreateMap<Hotel, HotelDTO>().ReverseMap();
            //CreateMap<Phone_List_MasterDTO , Phone_List_Master>().
            //    ForMember(dest => dest.Phone_List_Details, opt => opt.Ignore())
            //    .ReverseMap();
            //CreateMap<UpdatePhone_List_DetailDTO, Phone_List_Detail>()
            //    .ForMember(dest=>dest.Phone_List_Master_id , opt=>opt.Ignore())
            //    .ReverseMap();
            CreateMap<UserDTO, ApplicationUser>().ReverseMap();
            //CreateMap<UpdatePhone_List_Detail_TypeDTO, Phone_List_Detail_Type>().ReverseMap();
            //CreateMap<T_Hospital, HospitalDTO>().ReverseMap();
            //CreateMap<BuildDTO, T_Build>()
            //    .ForMember(dest=>dest.Hospital , opt=>opt.Ignore())
            //    .ReverseMap();
            //CreateMap<FloorDTO, T_Floor>()
            //    .ForMember(dest => dest.Floor_Name, opt => opt.Ignore())
            //    .ForMember(dest => dest.Build, opt => opt.Ignore())
            //    .ReverseMap();
            //CreateMap<DepartmentDTO, T_Department>()
            //    .ForMember(dest => dest.Floor, opt => opt.Ignore())
            //    .ReverseMap();
        }
    }
}
