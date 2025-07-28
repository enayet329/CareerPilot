using CareerPilot.API.Plugins;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel;

namespace CareerPilot.API.Agents;

public class JobMatcherAgent
{
	private readonly Kernel _kernel;
	public const string AgentName = "JobMatcher";

	public JobMatcherAgent(Kernel kernel)
	{
		_kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
	}

	public ChatCompletionAgent Create()
	{
		var getJobs = _kernel.Plugins.GetFunction("JobAssistantPlugin", "GetJobs");

		return new ChatCompletionAgent
		{
			Name = AgentName,
			Instructions = GetInstructions(),
			Kernel = _kernel,
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