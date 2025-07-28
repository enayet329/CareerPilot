using CareerPilot.API.Dto;
using CareerPilot.API.Service.Interface;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace CareerPilot.API.Plugins
{
	public class JobAssistantPlugin
	{
		private static readonly HttpClient _httpClient = new HttpClient();
		private readonly IUserService _userService;
		private const string ApiUrl = "https://arbeitnow.com/api/job-board-api?remote=true&visa_sponsorship=true";

		public JobAssistantPlugin(IUserService userService)
		{
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
		}

		[KernelFunction("GetJobs"), Description("Get list of available jobs based on search parameter")]
		public async Task<string> GetJobsAsync(string searchQuery)
		{
			try
			{
				var response = await _httpClient.GetAsync($"{ApiUrl}&page=1");
				response.EnsureSuccessStatusCode();

				using var stream = await response.Content.ReadAsStreamAsync();
				var data = await JsonSerializer.DeserializeAsync<ArbeitnowResponse>(stream);

				var allJobs = data?.data ?? new List<ArbeitnowJob>();

				var filteredJobs = allJobs
					.Where(job => job.title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
					.Select(job => new JobListing
					{
						Title = job.title,
						Company = job.company_name,
						Location = job.location,
						Url = job.url
					})
					.ToList();

				if (filteredJobs.Count < 10)
				{
					filteredJobs.AddRange(
						allJobs
							.Where(job => !job.title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
							.Select(job => new JobListing
							{
								Title = job.title,
								Company = job.company_name,
								Location = job.location,
								Url = job.url
							})
							.Take(10 - filteredJobs.Count)
					);
				}

				return JsonSerializer.Serialize(filteredJobs);
			}
			catch (Exception ex)
			{
				return $"Error fetching jobs: {ex.Message}";
			}
		}
	}



}