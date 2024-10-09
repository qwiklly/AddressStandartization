using AddressStandartization.DTOs;

namespace AddressStandartization.Services
{
	public interface IAddressStandardizationService
	{
		Task<AddressResponseDTO> StandardizeAddressAsync(string rawAddress);
	}
}
