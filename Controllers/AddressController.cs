using AddressStandartization.Services;
using Microsoft.AspNetCore.Mvc;

namespace AddressStandartization.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AddressController : ControllerBase
	{
		private readonly IAddressStandardizationService _addressService;

		public AddressController(IAddressStandardizationService addressService)
		{
			_addressService = addressService;
		}
		/// <summary>
		/// Стандартизирует указанный адрес с использованием Dadata API.
		/// </summary>
		/// <param name="rawAddress">Необработанный адрес, который нужно стандартизировать.</param>
		/// <returns>Возвращает стандартизированный адрес или ошибку</returns>
		[HttpGet("standardize")]
		public async Task<IActionResult> StandardizeAddress([FromQuery] string rawAddress)
		{
			if (string.IsNullOrWhiteSpace(rawAddress))
			{
				return BadRequest("Адрес не должен быть пустым");
			}

			try
			{
				var standardizedAddress = await _addressService.StandardizeAddressAsync(rawAddress);
				return standardizedAddress != null ? Ok(standardizedAddress) : NotFound("Не удалось стандартизировать адрес");
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message); 
			}
			catch (HttpRequestException)
			{
				return StatusCode(502, "Ошибка внешнего API"); 
			}
			catch (Exception)
			{
				return StatusCode(500, "Внутренняя ошибка сервера"); 
			}
		}
	}
}