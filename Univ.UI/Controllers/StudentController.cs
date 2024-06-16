using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Univ.UI.Models;
using Univ.UI.Filters;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Univ.UI.Controllers
{
	[ServiceFilter(typeof(AuthFilter))]
	public class StudentController : Controller
    {
		private HttpClient _client;
		public StudentController()
		{
			_client = new HttpClient();
		}
		public async Task<IActionResult> Index(int page = 1, int size = 4)
		{
			_client.DefaultRequestHeaders.Add(HeaderNames.Authorization, Request.Cookies["token"]);
			var queryString = new StringBuilder();
			queryString.Append("?page=").Append(Uri.EscapeDataString(page.ToString()));
			queryString.Append("&size=").Append(Uri.EscapeDataString(size.ToString()));

			string requestUrl = "https://localhost:7068/api/students" + queryString;
			using (var response = await _client.GetAsync(requestUrl))
			{
				if (response.IsSuccessStatusCode)
				{
					var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
					var data = JsonSerializer.Deserialize<PaginatedResponse<StudentListItemGetResponse>>(await response.Content.ReadAsStringAsync(), options);
					if (data.TotalPages < page) return RedirectToAction("index", new { page = data.TotalPages });

					return View(data);
				}
				else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
					return RedirectToAction("login", "account");
				else
					return RedirectToAction("error", "home");
			}
		}


		public async Task<IActionResult> Create()
		{
			_client.DefaultRequestHeaders.Add(HeaderNames.Authorization, Request.Cookies["token"]);

			var response = await _client.GetAsync("https://localhost:7068/api/groups");
			if (response.IsSuccessStatusCode)
			{
				var bodyStr = await response.Content.ReadAsStringAsync();

				var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
				List<GroupResponse> groups = JsonSerializer.Deserialize<List<GroupResponse>>(bodyStr, options);
				ViewBag.Groups = groups;
				Console.WriteLine("Groups fetched from API:");
				groups.ForEach(g => Console.WriteLine($"Id: {g.Id}, No: {g.No}"));
			}
			else
			{
				ViewBag.Groups = new List<GroupResponse>();
				TempData["Error"] = "Failed to load groups.";
			}

			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(StudentCreateRequest create)
		{
			_client.DefaultRequestHeaders.Add(HeaderNames.Authorization, Request.Cookies["token"]);
			if (!ModelState.IsValid)return View(create);

			using var content = new MultipartFormDataContent();
			content.Add(new StringContent(create.Fullname), "Fullname");
			content.Add(new StringContent(create.Email), "Email");
			content.Add(new StringContent(create.Birthdate.ToString("dd MMM,yyyy")), "Birthdate"); 
			content.Add(new StringContent(create.GroupId.ToString()), "GroupId");

			if (create.File != null)
			{
				var streamContent = new StreamContent(create.File.OpenReadStream());
				streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
				{
					Name = "File",
					FileName = create.File.FileName
				};
				streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(create.File.ContentType);

				content.Add(streamContent, "File", create.File.FileName);
			}

			using HttpResponseMessage response = await _client.PostAsync("https://localhost:7068/api/students", content);

			if (response.IsSuccessStatusCode)
				return RedirectToAction("Index");
			else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				return RedirectToAction("login", "account");
			else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
			{
				var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(await response.Content.ReadAsStringAsync(), options);
				foreach (var err in errorResponse.Errors)
					ModelState.AddModelError(err.Key, err.Message);

				var groupsResponse = await _client.GetAsync("https://localhost:7068/api/groups");
				if (groupsResponse.IsSuccessStatusCode)
				{
					var bodyStr = await response.Content.ReadAsStringAsync();
					List<GroupResponse> groups = JsonSerializer.Deserialize<List<GroupResponse>>(bodyStr, options);
					ViewBag.Groups = groups;

				}
				else
				{
					ViewBag.Groups = new List<GroupResponse>();
					TempData["Error"] = "Failed to load groups.";
				}

				return View(create);
			}
			else
			{
				TempData["Error"] = "Something went wrong!";
				return View(create);
			}
		}

	}
}
