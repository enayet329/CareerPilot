using CareerPilot.API.Service.Interface;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace CareerPilot.API.Plugins
{
	public class ResumeFetchPlugin
	{
		private readonly IUserService _userService;

		public ResumeFetchPlugin(IUserService userService)
		{
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
		}

		[KernelFunction("FetchUserResume"), Description("Fetch user resume from database by user ID")]
		public async Task<string> FetchUserResumeAsync(int userId)
		{
			var userResume = (await _userService.GetUserFileInfoByIdAsync(userId))?.ResumeText;
			return string.IsNullOrEmpty(userResume)
				? "## ResumeFetchResult\nNo resume found for the user in the database. Proceed with job search."
				: "## ResumeFetchResult\n" + userResume;
		}
	}
}