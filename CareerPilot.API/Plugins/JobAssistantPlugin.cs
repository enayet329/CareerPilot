using CareerPilot.API.Dto;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace CareerPilot.API.Plugins
{




	public class JobAssistantPlugin
	{
		private static readonly HttpClient httpClient = new HttpClient();
		private const string apiUrl = "https://arbeitnow.com/api/job-board-api";

		[KernelFunction, Description("Get list of available jobs based on search parameter")]
		public async Task<string> GetJobs(string searchQuery)
		{
			var response = await httpClient.GetAsync($"{apiUrl}?page=1");
			response.EnsureSuccessStatusCode();
			using var stream = await response.Content.ReadAsStreamAsync();
			var data = await JsonSerializer.DeserializeAsync<ArbeitnowResponse>(stream);

			var filteredJobs = data.data
				.Where(job => job.title.Contains(searchQuery, System.StringComparison.OrdinalIgnoreCase))
				.Select(job => new JobListing
				{
					Title = job.title,
					Company = job.company,
					Location = job.location,
					Url = job.url
				})
				.ToList();

			return JsonSerializer.Serialize(filteredJobs);
		}
	}
}
