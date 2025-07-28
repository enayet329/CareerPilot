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
		if (!_kernel.Plugins.TryGetPlugin("ResumeFetchPlugin", out var plugin))
		{
			throw new InvalidOperationException("Plugin ResumeFetchPlugin not found in kernel.");
		}

		var getResume = _kernel.Plugins.GetFunction("ResumeFetchPlugin", "FetchUserResume");
		if (getResume == null)
		{
			throw new InvalidOperationException("Function FetchUserResume not found in ResumeFetchPlugin.");
		}

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
            Your job is to carefully analyze the provided resume and extract key information in Markdown format:
            - **Full name**
            - **Email and phone**
            - **Current job title**
            - **Years of experience**
            - **Skills** (technical and soft skills)
            - **Education history**
            - **Work experience**
            If no resume is provided (e.g., message contains "No resume found"), output:
            ## ResumeParseResult
            No structured resume data available. Proceed with job search.
            Be precise and comprehensive. Output only the structured data in Markdown format.
            Do not engage in chitchat or provide additional commentary.
            """;
	}
}