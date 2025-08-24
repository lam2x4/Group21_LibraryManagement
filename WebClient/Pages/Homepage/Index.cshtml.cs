using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WebClient.Dto.Book;
using static System.Reflection.Metadata.BlobBuilder;

namespace WebClient.Pages.Homepage
{
	public class IndexModel : PageModel
	{

		private readonly IHttpClientFactory _httpClientFactory;

		public List<BookRequest> Books { get; set; } = new();

		public IndexModel(IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory;
		}

		public async Task OnGetAsync()
		{
			var token = HttpContext.Request.Cookies["JWToken"];
			var client = _httpClientFactory.CreateClient("Api");

			if (!string.IsNullOrEmpty(token))
			{
				client.DefaultRequestHeaders.Authorization =
					new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
			}

			var response = await client.GetAsync("odata/Books");
			if (response.IsSuccessStatusCode)
			{
				var json = await response.Content.ReadAsStringAsync();

				var result = JsonSerializer.Deserialize<ODataResponse<BookRequest>>(json,
					new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

				Books = result?.Value ?? new();
			}

		}
	}
}
