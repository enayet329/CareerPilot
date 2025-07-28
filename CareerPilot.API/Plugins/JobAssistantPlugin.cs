using CareerPilot.API.Dto;
using CareerPilot.API.Service.Interface;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CareerPilot.API.Plugins
{
	public class JobAssistantPlugin
	{
		private static readonly HttpClient _httpClient = new HttpClient();
		private readonly IUserService _userService;
		private const string ApiUrl = "https://arbeitnow.com/api/job-board-api";

		public JobAssistantPlugin(IUserService userService)
		{
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
		}

		[KernelFunction, Description("Get list of available jobs based on search parameter")]
		public async Task<string> GetJobsAsync(string searchQuery)
		{
			var response = await _httpClient.GetAsync($"{ApiUrl}?page=1");
			response.EnsureSuccessStatusCode();

			using var stream = await response.Content.ReadAsStreamAsync();
			var data = await JsonSerializer.DeserializeAsync<ArbeitnowResponse>(stream);

			var filteredJobs = data?.data
				.Where(job => job.title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
				.Select(job => new JobListing
				{
					Title = job.title,
					Company = job.company,
					Location = job.location,
					Url = job.url
				})
				.ToList() ?? [];

			return JsonSerializer.Serialize(filteredJobs);
		}
	}
}