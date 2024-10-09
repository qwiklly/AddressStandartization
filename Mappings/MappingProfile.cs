using AddressStandartization.DTOs;
using AutoMapper;

namespace AddressStandartization.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<AddressResponseDTO, AddressResponseDTO>(); 
		}
	}
}
