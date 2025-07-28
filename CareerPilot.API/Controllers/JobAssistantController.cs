using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.Agents;
using CareerPilot.API;
using System;
using System.Text;
using System.Threading.Tasks;
using CareerPilot.API.AgentOrchestration;


#pragma warning disable SKEXP0110 // Suppress experimental warning for JobAssistantSystem
#pragma warning disable SKEXP0001 // Suppress experimental warning for ChatMessageContent.AuthorName

namespace CareerPilot.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class JobAssistantController : ControllerBase
	{
		private readonly JobAssistantSystem _jobAssistantSystem;

		public JobAssistantController(JobAssistantSystem jobAssistantSystem)
		{
			_jobAssistantSystem = jobAssistantSystem ?? throw new ArgumentNullException(nameof(jobAssistantSystem));
		}

		[HttpPost("process")]
		public async Task ProcessJobAssistance([FromBody] JobAssistanceRequest request)
		{
			if (request == null || request.UserId <= 0 || string.IsNullOrEmpty(request.InitialMessage))
			{
				Response.StatusCode = 400;
				await Response.WriteAsync("Invalid request: UserId and InitialMessage are required.");
				return;
			}

			Response.ContentType = "text/event-stream";
			Response.Headers.Append("Cache-Control", "no-cache");
			Response.Headers.Append("Connection", "keep-alive");

			await foreach (var message in _jobAssistantSystem.RunAsync(request.UserId, request.InitialMessage))
			{
				var response = new ChatMessageResponse
				{
					Role = message.Role.ToString(),
					AuthorName = message.AuthorName ?? "*",
					Content = message.Content
				};

				var data = System.Text.Json.JsonSerializer.Serialize(response);
				var sseMessage = $"data: {data}\n\n";
				await Response.WriteAsync(sseMessage, Encoding.UTF8);
				await Response.Body.FlushAsync();
			}

			await Response.CompleteAsync();
		}
	}

	public class JobAssistanceRequest
	{
		public int UserId { get; set; }
		public string InitialMessage { get; set; }
	}

	public class ChatMessageResponse
	{
		public string Role { get; set; }
		public string AuthorName { get; set; }
		public string Content { get; set; }
	}
}