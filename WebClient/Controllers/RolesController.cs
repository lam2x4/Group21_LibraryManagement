using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebClient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RolesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private string? GetAuthToken()
        {
            return Request.Cookies["JWToken"];
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("api/Roles");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Ok(JsonSerializer.Deserialize<object>(content));
                }
                else
                {
                    return StatusCode((int)response.StatusCode, content);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi gọi API: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRole(string id)
        {
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"api/Roles/{id}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Ok(JsonSerializer.Deserialize<object>(content));
                }
                else
                {
                    return StatusCode((int)response.StatusCode, content);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi gọi API: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] object roleData)
        {
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var json = JsonSerializer.Serialize(roleData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/Roles", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Ok(responseContent);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, responseContent);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi gọi API: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] object roleData)
        {
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var json = JsonSerializer.Serialize(roleData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"api/Roles/{id}", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Ok(responseContent);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, responseContent);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi gọi API: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.DeleteAsync($"api/Roles/{id}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Ok(content);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, content);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi gọi API: {ex.Message}");
            }
        }

        [HttpGet("{id}/users")]
        public async Task<IActionResult> GetUsersInRole(string id)
        {
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"api/Roles/{id}/users");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Ok(JsonSerializer.Deserialize<object>(content));
                }
                else
                {
                    return StatusCode((int)response.StatusCode, content);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi gọi API: {ex.Message}");
            }
        }
    }
}
