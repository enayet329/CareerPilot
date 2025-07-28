using CareerPilot.API.Plugins;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel;

namespace CareerPilot.API.Agents;

public class ResumeParserAgent
{
	private readonly Kernel _kernel;
	public const string AgentName = "ResumeParser";

	public ResumeParserAgent(Kernel kernel)
	{
		_kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
	}

	public ChatCompletionAgent Create(int userId)
	{
		var getResume = _kernel.Plugins.GetFunction(nameof(ResumeFetchPlugin), nameof(ResumeFetchPlugin.FetchUserResumeAsync));

		return new ChatCompletionAgent
		{
			Name = AgentName,
			Instructions = GetInstructions(),
			Kernel = _kernel,
			Arguments = new KernelArguments(new PromptExecutionSettings
			{
				FunctionChoiceBehavior = FunctionChoiceBehavior.Required([getResume])
			})
			{
				{ "userId", userId }
			}
		};
	}

	private static string GetInstructions()
	{
		return """
            You are a Resume Parser specializing in extracting structured information from resume text.
            Your job is to carefully analyze the provided resume and extract key information including:
            - Full name
            - Email and phone
            - Current job title
            - Years of experience
            - Skills (technical and soft skills)
            - Education history
            - Work experience
            Be precise and comprehensive.
            Do not engage in chitchat or provide additional commentary.
            """;
	}
}