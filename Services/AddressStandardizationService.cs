using AddressStandartization.Configuration;
using AddressStandartization.DTOs;
using AutoMapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace AddressStandartization.Services
{
	public class AddressStandardizationService : IAddressStandardizationService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<AddressStandardizationService> _logger;
		private readonly IMapper _mapper;
		private readonly DadataSettings _dadataSettings;

		public AddressStandardizationService(HttpClient httpClient, ILogger<AddressStandardizationService> logger, IMapper mapper, IOptions<DadataSettings> options)
		{
			_httpClient = httpClient;
			_logger = logger;
			_mapper = mapper;
			_dadataSettings = options.Value;
		}

		public async Task<AddressResponseDTO> StandardizeAddressAsync(string rawAddress)
		{
			if (string.IsNullOrWhiteSpace(rawAddress))
			{
				throw new ArgumentException("Адрес не может быть пустым");
			}

			var requestData = new List<string> { rawAddress };
			var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

			try
			{
				// Используем URL и ключ API из _dadataSettings
				var request = new HttpRequestMessage(HttpMethod.Post, _dadataSettings.ApiUrl);
				request.Headers.Add("Authorization", $"Token {_dadataSettings.ApiKey}");
				request.Content = content;

				var response = await _httpClient.SendAsync(request);

				if (!response.IsSuccessStatusCode)
				{
					_logger.LogError("Ошибка API Dadata: {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
					
					if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
					{
						throw new ArgumentException("Неправильный запрос к Dadata API");
					}
					throw new HttpRequestException($"Ошибка API Dadata: {response.StatusCode} {response.ReasonPhrase}");
				}

				var jsonResponse = await response.Content.ReadAsStringAsync();
				var addressList = JsonConvert.DeserializeObject<List<AddressResponseDTO>>(jsonResponse);

				if (addressList == null || addressList.Count == 0)
				{
					_logger.LogWarning("Dadata API вернул пустой результат");
					throw new Exception("Dadata API вернул пустой результат");
				}

				return _mapper.Map<AddressResponseDTO>(addressList.FirstOrDefault());
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "Ошибка во время вызова Dadata API");
				throw new Exception("Ошибка при вызове внешнего API", ex);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Неизвестная ошибка");
				throw new Exception("Серверная ошибка", ex);
			}
		}
	}
}