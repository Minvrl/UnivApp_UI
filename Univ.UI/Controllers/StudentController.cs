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
			content.Add(new StringContent(create.BirthDate.ToString("dd MM yyyy HH:mm")), "BirthDate"); 
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



		public async Task<IActionResult> Delete(int id)
		{
			_client.DefaultRequestHeaders.Add(HeaderNames.Authorization, Request.Cookies["token"]);

			using (var response = await _client.DeleteAsync("https://localhost:7068/api/Students/" + id))
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


		public async Task<IActionResult> Edit(int id)
		{
			_client.DefaultRequestHeaders.Add(HeaderNames.Authorization, Request.Cookies["token"]);

			using (var response = await _client.GetAsync("https://localhost:7068/api/Students/" + id))
			{
				if (response.IsSuccessStatusCode)
				{
					var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
					StudentCreateRequest request = JsonSerializer.Deserialize<StudentCreateRequest>(await response.Content.ReadAsStringAsync(), options);

					var groupsResponse = await _client.GetAsync("https://localhost:7068/api/groups");
					if (groupsResponse.IsSuccessStatusCode)
					{
						var bodyStr = await groupsResponse.Content.ReadAsStringAsync();
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

					return View(request);
				}
				else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
					return RedirectToAction("login", "account");
				else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
					TempData["Error"] = "Student not found";
				else
					TempData["Error"] = "Something went wrong!";
			}

			return RedirectToAction("index");
		}


		[HttpPost]
		public async Task<IActionResult> Edit(StudentCreateRequest edit, int id)
		{
			_client.DefaultRequestHeaders.Add(HeaderNames.Authorization, Request.Cookies["token"]);

			if (!ModelState.IsValid)
			{
				await LoadGroups();
				return View(edit);
			}

			using var content = new MultipartFormDataContent();
			content.Add(new StringContent(edit.Fullname), "Fullname");
			content.Add(new StringContent(edit.Email), "Email");
			content.Add(new StringContent(edit.BirthDate.ToString("dd MM yyyy HH:mm")), "Birthdate");
			content.Add(new StringContent(edit.GroupId.ToString()), "GroupId");

			if (edit.File != null)
			{
				var streamContent = new StreamContent(edit.File.OpenReadStream());
				streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
				{
					Name = "File",
					FileName = edit.File.FileName
				};
				streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(edit.File.ContentType);

				content.Add(streamContent, "File", edit.File.FileName);
			}


			using (HttpResponseMessage response = await _client.PutAsync("https://localhost:7068/api/Students/"+id, content))
			{
				if (response.IsSuccessStatusCode)
				{
					return RedirectToAction("Index");
				}
				else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					return RedirectToAction("login", "account");
				}
				else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
				{
					var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
					var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(await response.Content.ReadAsStringAsync(), options);

					foreach (var err in errorResponse.Errors)
					{
						ModelState.AddModelError(err.Key, err.Message);
					}

					await LoadGroups();
					return View(edit);
				}
				else
				{
					TempData["Error"] = "Something went wrong!";
					return RedirectToAction("Index");
				}
			}
		}

		private async Task LoadGroups()
		{
			var groupsResponse = await _client.GetAsync("https://localhost:7068/api/groups");
			if (groupsResponse.IsSuccessStatusCode)
			{
				var bodyStr = await groupsResponse.Content.ReadAsStringAsync();
				var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				List<GroupResponse> groups = JsonSerializer.Deserialize<List<GroupResponse>>(bodyStr, options);
				ViewBag.Groups = new SelectList(groups, nameof(GroupResponse.Id), nameof(GroupResponse.No));
			}
			else
			{
				ViewBag.Groups = new SelectList(new List<GroupResponse>(), nameof(GroupResponse.Id), nameof(GroupResponse.No));
				TempData["Error"] = "Failed to load groups.";
			}
		}

	}
}
