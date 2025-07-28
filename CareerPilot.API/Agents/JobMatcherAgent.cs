using CareerPilot.API.Plugins;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel;

namespace CareerPilot.API.Agents
{
	public class JobMatcherAgent(Kernel kernel)
	{
		public const string AgentName = "JobMatcher";

		public ChatCompletionAgent Create()
		{
			var getJobs = kernel.Plugins.GetFunction(nameof(JobAssistantPlugin), nameof(JobAssistantPlugin.GetJobsAsync));

			return new ChatCompletionAgent
			{
				Name = AgentName,
				Instructions = GetInstructions(),
				Kernel = kernel,
				Arguments = new KernelArguments(new PromptExecutionSettings()
				{
					FunctionChoiceBehavior = FunctionChoiceBehavior.Required([getJobs])
				})
			};
		}

		private static string GetInstructions()
		{
			return """
               Get list of available jobs to find the best job matches based on a candidate's resume.
               Be direct and professional. Do not engage in chitchat.
               """;
		}
	}
}
