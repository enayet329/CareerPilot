using CareerPilot.API.Agents;
using CareerPilot.API.Plugins;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using OpenAI;
using System.ClientModel;
using System.Diagnostics.CodeAnalysis;
using CareerPilot.API.Service.Interface;

#pragma warning disable SKEXP0110 // Suppress experimental warning for JobAssistantSystem

namespace CareerPilot.API.AgentOrchestration;

[Experimental("SKEXP0110")]
public class JobAssistantSystem
{
	private readonly IServiceProvider _serviceProvider;
	private readonly Kernel _kernel;
	private readonly AgentGroupOrchestrator _orchestrator;

	public JobAssistantSystem(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		_kernel = CreateKernel();
		_orchestrator = new AgentGroupOrchestrator();
	}

	private Kernel CreateKernel()
	{
		var builder = Kernel.CreateBuilder();
		var openAIClient = new OpenAIClient(
			new ApiKeyCredential("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"),
			new OpenAIClientOptions { Endpoint = new Uri("https://openrouter.ai/api/v1") });
		builder.AddOpenAIChatCompletion("openai/gpt-3.5-turbo", openAIClient);
		return builder.Build();
	}

	public async IAsyncEnumerable<ChatMessageContent> RunAsync(int userId, string initialUserMessage)
	{
		using var scope = _serviceProvider.CreateScope();
		var kernel = _kernel.Clone();

		var chat = CreateAgentGroupChat(userId, kernel, scope.ServiceProvider);
		chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, initialUserMessage));

		await foreach (var content in chat.InvokeAsync())
		{
			if (!string.IsNullOrWhiteSpace(content.Content))
			{
				yield return content;
			}
		}
	}

	private AgentGroupChat CreateAgentGroupChat(int userId, Kernel kernel, IServiceProvider serviceProvider)
	{
		var userService = serviceProvider.GetRequiredService<IUserService>();

		var resumeFetchPluginInstance = new ResumeFetchPlugin(userService);
		var jobAssistantPluginInstance = new JobAssistantPlugin(userService);

		kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(resumeFetchPluginInstance, "ResumeFetchPlugin"));
		kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(jobAssistantPluginInstance, "JobAssistantPlugin"));

		// Debug: Verify plugins are registered
		if (!kernel.Plugins.TryGetPlugin("ResumeFetchPlugin", out var plugin))
		{
			throw new InvalidOperationException("ResumeFetchPlugin not found in kernel after registration.");
		}

		var resumeParserAgent = new ResumeParserAgent(kernel).Create(userId);
		var jobMatcherAgent = new JobMatcherAgent(kernel).Create();
		var careerAdvisorAgent = new CareerAdvisorAgent(kernel).Create();
		var interviewCoachAgent = new InterviewCoachAgent(kernel).Create();

		return new AgentGroupChat(resumeParserAgent, jobMatcherAgent, careerAdvisorAgent, interviewCoachAgent)
		{
			ExecutionSettings = _orchestrator.CreateExecutionSettings(userId, kernel)
		};
	}
}