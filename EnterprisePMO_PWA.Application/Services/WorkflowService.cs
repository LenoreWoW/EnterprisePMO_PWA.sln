using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Represents a single workflow step.
    /// </summary>
    public class WorkflowStep
    {
        public required string StepName { get; set; }  // Name of the step
        public required string Role { get; set; }        // Role required to execute this step
        public required string Action { get; set; }      // Description of the action
        public required string NextStep { get; set; }    // Identifier for the next step
    }

    /// <summary>
    /// Represents an entire workflow configuration loaded from a JSON file.
    /// </summary>
    public class Workflow
    {
        public required string WorkflowName { get; set; }
        public required WorkflowStep[] Steps { get; set; }
    }

    /// <summary>
    /// Provides methods for loading and executing workflow configurations.
    /// </summary>
    public class WorkflowService
    {
        /// <summary>
        /// Loads a workflow configuration from a JSON file.
        /// </summary>
        public async Task<Workflow> LoadWorkflowAsync(string workflowFile)
        {
            var json = await File.ReadAllTextAsync(workflowFile);
            return JsonSerializer.Deserialize<Workflow>(json) ?? new Workflow 
            { 
                WorkflowName = "Empty",
                Steps = Array.Empty<WorkflowStep>()
            };
        }

        /// <summary>
        /// Executes a workflow step and returns the next step if the user has the required role.
        /// </summary>
        public string? ExecuteStep(Workflow workflow, string currentStep, string userRole)
        {
            var step = System.Array.Find(workflow.Steps, s => s.StepName.Equals(currentStep, System.StringComparison.OrdinalIgnoreCase));
            if (step == null)
                return null;

            if (step.Role.Equals(userRole, System.StringComparison.OrdinalIgnoreCase))
            {
                return step.NextStep;
            }
            else
            {
                throw new System.Exception("User does not have the required role to execute this step.");
            }
        }
    }
}