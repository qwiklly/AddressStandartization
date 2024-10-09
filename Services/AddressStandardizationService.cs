using AddressStandartization.DTOs;
using AutoMapper;
using Newtonsoft.Json;
using System.Text;

namespace AddressStandartization.Services
{
	public class AddressStandardizationService : IAddressStandardizationService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<AddressStandardizationService> _logger;
		private readonly IConfiguration _configuration;
		private readonly IMapper _mapper;

		public AddressStandardizationService(HttpClient httpClient, ILogger<AddressStandardizationService> logger, IConfiguration configuration, IMapper mapper)
		{
			_httpClient = httpClient;
			_logger = logger;
			_configuration = configuration;
			_mapper = mapper;
		}

		public async Task<AddressResponseDTO> StandardizeAddressAsync(string rawAddress)
		{
			var apiKey = _configuration["Dadata:ApiKey"];
			var secretKey = _configuration["Dadata:SecretKey"];  
			var apiUrl = _configuration["Dadata:ApiUrl"];

			var requestData = new List<string> { rawAddress };
			var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

			var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl)
			{
				Headers =
				{
					{ "Authorization", $"Token {apiKey}" },
					{ "X-Secret", secretKey },  
					{ "Accept", "application/json" }
				},
				Content = content
			};

			try
			{
				var response = await _httpClient.SendAsync(requestMessage);
				response.EnsureSuccessStatusCode();
				var jsonResponse = await response.Content.ReadAsStringAsync();

				var addressList = JsonConvert.DeserializeObject<List<AddressResponseDTO>>(jsonResponse);

				return _mapper.Map<AddressResponseDTO>(addressList?.FirstOrDefault());
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "Ошибка во время вызова Dadata API");
				throw;
			}
		}
	}
}