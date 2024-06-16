using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Univ.UI.Models;

namespace Univ.UI.Controllers
{
    public class GroupController:Controller
    {
		private HttpClient _client;
		public GroupController()
		{
			_client = new HttpClient();
		}
		public async Task<IActionResult> Index(int page = 1)
        {
            using (HttpClient client = new HttpClient())
            {
                using (var response = await client.GetAsync("https://localhost:7068/api/Groups?page=" + page + "&size=2"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var bodyStr = await response.Content.ReadAsStringAsync();

                        var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                        PaginatedResponse<GroupListItemGetResponse> data = JsonSerializer.Deserialize<PaginatedResponse<GroupListItemGetResponse>>(bodyStr, options);
                        return View(data);
                    }
                    else
                    {
                        return RedirectToAction("error", "home");
                    }
                }
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(GroupCreateRequest create)
        {
			//_client.DefaultRequestHeaders.Add(HeaderNames.Authorization, Request.Cookies["token"]);
			if (!ModelState.IsValid) return View();
            var content = new StringContent(JsonSerializer.Serialize(create),Encoding.UTF8,"application/json");
            using(HttpResponseMessage response = await _client.PostAsync("https://localhost:7068/api/Groups", content))
            {
                if (response.IsSuccessStatusCode) return RedirectToAction("index");
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) return RedirectToAction("login", "account");
                else if(response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                    ErrorResponse errorResponse = JsonSerializer.Deserialize<ErrorResponse>(await response.Content.ReadAsStringAsync(), options);
					foreach (var err in errorResponse.Errors)
						ModelState.AddModelError(err.Key, err.Message);

					return View();
				}
				else
				{
					TempData["Error"] = "Something went wrong!";
				}
			}
			return View(create);

		}

		public async Task<IActionResult> Edit(int id)
		{
			//_client.DefaultRequestHeaders.Add(HeaderNames.Authorization, Request.Cookies["token"]);

			using (var response = await _client.GetAsync("https://localhost:7068/api/Groups/" + id))
			{
				if (response.IsSuccessStatusCode)
				{
					var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
					GroupCreateRequest request = JsonSerializer.Deserialize<GroupCreateRequest>(await response.Content.ReadAsStringAsync(), options);
					return View(request);
				}
				else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					return RedirectToAction("login", "account");
				}
				else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
					TempData["Error"] = "Group not found";
				else
					TempData["Error"] = "Something went wrong!";
			}
			return RedirectToAction("index");
		}

		[HttpPost]
		public async Task<IActionResult> Edit(GroupCreateRequest edit, int id)
		{
			//_client.DefaultRequestHeaders.Add(HeaderNames.Authorization, Request.Cookies["token"]);

			if (!ModelState.IsValid) return View();

			var content = new StringContent(JsonSerializer.Serialize(edit), Encoding.UTF8, "application/json");
			using (HttpResponseMessage response = await _client.PutAsync("https://localhost:7068/api/Groups/" + id, content))
			{
				if (response.IsSuccessStatusCode)
				{
					return RedirectToAction("index");
				}
				else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					return RedirectToAction("login", "account");
				}
				else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
				{
					var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
					ErrorResponse errorResponse = JsonSerializer.Deserialize<ErrorResponse>(await response.Content.ReadAsStringAsync(), options);

					foreach (var item in errorResponse.Errors)
						ModelState.AddModelError(item.Key, item.Message);

					return View();
				}
				else
					TempData["Error"] = "Something went wrong!";
			}

			return View(edit);
		}


        public async Task<IActionResult> Delete(int id)
        {
            //_client.DefaultRequestHeaders.Add(HeaderNames.Authorization, Request.Cookies["token"]);

            using (var response = await _client.DeleteAsync("https://localhost:7068/api/Groups/" + id))
            {
                if (response.IsSuccessStatusCode)
                    return Ok();
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return Unauthorized();
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return NotFound();
                else
                    return StatusCode(500);
            }
        }

    }
}
