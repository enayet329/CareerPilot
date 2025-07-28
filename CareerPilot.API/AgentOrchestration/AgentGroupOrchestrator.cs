using CareerPilot.API.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel;
using System.Diagnostics.CodeAnalysis;

namespace CareerPilot.API.AgentOrchestration;

public class AgentGroupOrchestrator
{
	private const string ResumeParserName = ResumeParserAgent.AgentName;
	private const string JobMatcherName = JobMatcherAgent.AgentName;
	private const string CareerAdvisorName = CareerAdvisorAgent.AgentName;
	private const string InterviewCoachName = InterviewCoachAgent.AgentName;

	public AgentGroupOrchestrator() { }

	[Experimental("SKEXP0110")]
	public AgentGroupChatSettings CreateExecutionSettings(int userId, Kernel kernel)
	{
		var resumeParserAgent = new ResumeParserAgent(kernel).Create(userId);
		var jobMatcherAgent = new JobMatcherAgent(kernel).Create();
		var careerAdvisorAgent = new CareerAdvisorAgent(kernel).Create();
		var interviewCoachAgent = new InterviewCoachAgent(kernel).Create();

		var agents = new[] { resumeParserAgent, jobMatcherAgent, careerAdvisorAgent, interviewCoachAgent };

		return new AgentGroupChatSettings
		{
			TerminationStrategy = CreateTerminationStrategy(agents, kernel),
			SelectionStrategy = CreateSelectionStrategy(kernel)
		};
	}

	[Experimental("SKEXP0110")]
	private KernelFunctionSelectionStrategy CreateSelectionStrategy(Kernel kernel)
	{
		var selectionFunction = KernelFunctionFactory.CreateFromPrompt(
			$$$"""
                Your job is to determine which participant takes the next turn in the job assistance conversation based on the current state.
                State ONLY the name of the participant to take the next turn, without any additional text.

                Choose only from these participants:
                - {{{ResumeParserName}}}
                - {{{JobMatcherName}}}
                - {{{CareerAdvisorName}}}
                - {{{InterviewCoachName}}}

                Always follow these rules when selecting the next participant:
                1) If the last message was from the user and contains a user ID, it is {{{ResumeParserName}}}'s turn to extract structured information, but only if {{{ResumeParserName}}} has not yet been called in the conversation history.
                2) If the last message was from {{{ResumeParserName}}} and contains extracted resume data (e.g., starts with "## ResumeFetchResult"), it's {{{JobMatcherName}}}'s turn to find matching jobs.
                3) If the last message was from {{{JobMatcherName}}} and contains job matching results, it's {{{CareerAdvisorName}}}'s turn to provide career advice.
                4) If the last message was from {{{CareerAdvisorName}}} and contains career advice, it's {{{InterviewCoachName}}}'s turn to offer interview preparation guidance.
                5) If the last message was from {{{InterviewCoachName}}} and contains interview guidance, the conversation is complete.

                History:
                {{$history}}

                Return only one of these names: {{{ResumeParserName}}}, {{{JobMatcherName}}}, {{{CareerAdvisorName}}}, or {{{InterviewCoachName}}}.
                Do not include any explanation, just the name.
                """
		);

		return new KernelFunctionSelectionStrategy(selectionFunction, kernel)
		{
			HistoryVariableName = "history"
		};
	}

	[Experimental("SKEXP0110")]
	private KernelFunctionTerminationStrategy CreateTerminationStrategy(ChatCompletionAgent[] agents, Kernel kernel)
	{
		var terminateFunction = KernelFunctionFactory.CreateFromPrompt(
			"""
                Determine if the job assistance process is complete based on whether the InterviewCoach has provided their recommendations. If the last message was from InterviewCoach and contains interview guidance, respond with a single word: yes

                History:
                {{$history}}
                """
		);

		return new KernelFunctionTerminationStrategy(terminateFunction, kernel)
		{
			Agents = agents,
			ResultParser = result => result.GetValue<string>()?.Contains("yes", StringComparison.OrdinalIgnoreCase) ?? false,
			HistoryVariableName = "history",
			MaximumIterations = 10
		};
	}
}