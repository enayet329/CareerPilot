using CareerPilot.API.Agents;
using CareerPilot.API.Plugins;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using OpenAI;
using System.ClientModel;
using System.Diagnostics.CodeAnalysis;

namespace CareerPilot.API.AgentOrchestration;

[Experimental("SKEXP0110")]
public class JobAssistantSystem
{
	private readonly Kernel _kernel;
	private readonly AgentGroupOrchestrator _orchestrator;

	public JobAssistantSystem()
	{
		_kernel = CreateKernel();
		_orchestrator = new AgentGroupOrchestrator(_kernel);
	}

	private Kernel CreateKernel()
	{
		var builder = Kernel.CreateBuilder();
		var openAIClient = new OpenAIClient(
			new ApiKeyCredential("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"),
			new OpenAIClientOptions { Endpoint = new Uri("https://openrouter.ai/api/v1") });
		builder.AddOpenAIChatCompletion("openai/gpt-3.5-turbo", openAIClient);
		builder.Plugins.AddFromType<JobAssistantPlugin>();
		builder.Plugins.AddFromType<ResumeFetchPlugin>();
		return builder.Build();
	}

	public async IAsyncEnumerable<ChatMessageContent> RunAsync(int userId, string initialUserMessage)
	{
		var chat = CreateAgentGroupChat(userId);
		chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, initialUserMessage));

		await foreach (var content in chat.InvokeAsync())
		{
			if (!string.IsNullOrWhiteSpace(content.Content))
			{
				yield return content;
			}
		}
	}

	private AgentGroupChat CreateAgentGroupChat(int userId)
	{
		var resumeParserAgent = new ResumeParserAgent(_kernel).Create(userId);
		var jobMatcherAgent = new JobMatcherAgent(_kernel).Create();
		var careerAdvisorAgent = new CareerAdvisorAgent(_kernel).Create();
		var interviewCoachAgent = new InterviewCoachAgent(_kernel).Create();

		return new AgentGroupChat(resumeParserAgent, jobMatcherAgent, careerAdvisorAgent, interviewCoachAgent)
		{
			ExecutionSettings = _orchestrator.CreateExecutionSettings(userId)
		};
	}
}