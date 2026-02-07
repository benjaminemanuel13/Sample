using AgentDesigner.Domain.Entities;
using AgentDesigner.Domain.Enums;
using AgentDesigner.Infrastructure.Assemblies;
using WorkflowExecutionContext = AgentDesigner.Domain.Entities.ExecutionContext;

namespace AgentDesigner.Application.Services;

/// <summary>
/// Service for executing workflows.
/// </summary>
public class WorkflowExecutionService
{
    private readonly FunctionAssemblyLoader _assemblyLoader;
    private readonly Dictionary<Guid, WorkflowExecutionContext> _activeExecutions = [];

    public WorkflowExecutionService(FunctionAssemblyLoader assemblyLoader)
    {
        _assemblyLoader = assemblyLoader;
    }

    /// <summary>
    /// Starts executing a workflow.
    /// </summary>
    public async Task<WorkflowExecutionContext> ExecuteAsync(
        Workflow workflow,
        Dictionary<string, object?>? initialVariables = null,
        CancellationToken cancellationToken = default)
    {
        var context = new WorkflowExecutionContext
        {
            WorkflowId = workflow.Id,
            Variables = initialVariables ?? [],
            IsRunning = true,
            StartedAt = DateTime.UtcNow
        };

        _activeExecutions[context.Id] = context;

        try
        {
            // Initialize all nodes to Pending
            foreach (var node in workflow.Nodes)
            {
                context.NodeStates[node.Id] = NodeExecutionState.Pending;
            }

            context.Logs.Add(new ExecutionLogEntry
            {
                Level = "Info",
                Message = $"Starting workflow execution: {workflow.Name}"
            });

            // Find entry nodes (nodes with no incoming edges)
            var entryNodes = workflow.Nodes
                .Where(n => !workflow.Edges.Any(e => e.TargetNodeId == n.Id))
                .ToList();

            if (entryNodes.Count == 0)
            {
                throw new InvalidOperationException("Workflow has no entry nodes");
            }

            // Execute from entry nodes
            foreach (var entryNode in entryNodes)
            {
                await ExecuteNodeAsync(workflow, entryNode, context, cancellationToken);
            }

            context.IsRunning = false;
            context.CompletedAt = DateTime.UtcNow;

            context.Logs.Add(new ExecutionLogEntry
            {
                Level = "Info",
                Message = $"Workflow execution completed in {(context.CompletedAt.Value - context.StartedAt).TotalSeconds:F2}s"
            });
        }
        catch (OperationCanceledException)
        {
            context.IsCancelled = true;
            context.IsRunning = false;
            context.CompletedAt = DateTime.UtcNow;
            context.ErrorMessage = "Execution was cancelled";

            context.Logs.Add(new ExecutionLogEntry
            {
                Level = "Warning",
                Message = "Workflow execution cancelled"
            });
        }
        catch (Exception ex)
        {
            context.IsRunning = false;
            context.CompletedAt = DateTime.UtcNow;
            context.ErrorMessage = ex.Message;

            context.Logs.Add(new ExecutionLogEntry
            {
                Level = "Error",
                Message = $"Workflow execution failed: {ex.Message}"
            });
        }
        finally
        {
            _activeExecutions.Remove(context.Id);
        }

        return context;
    }

    /// <summary>
    /// Executes a single node and its downstream nodes.
    /// </summary>
    private async Task ExecuteNodeAsync(
        Workflow workflow,
        WorkflowNode node,
        WorkflowExecutionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Skip if already completed
        if (context.NodeStates[node.Id] == NodeExecutionState.Completed)
        {
            return;
        }

        context.NodeStates[node.Id] = NodeExecutionState.Running;
        context.Logs.Add(new ExecutionLogEntry
        {
            NodeId = node.Id,
            Level = "Info",
            Message = $"Executing node: {node.Name} ({node.NodeType})"
        });

        try
        {
            switch (node.NodeType)
            {
                case NodeType.InputNode:
                    await ExecuteInputNodeAsync(node, context);
                    break;

                case NodeType.OutputNode:
                    await ExecuteOutputNodeAsync(node, context);
                    break;

                case NodeType.FunctionNode:
                    await ExecuteFunctionNodeAsync(node, context);
                    break;

                case NodeType.AgentNode:
                    await ExecuteAgentNodeAsync(node, context);
                    break;

                case NodeType.DecisionNode:
                    await ExecuteDecisionNodeAsync(node, context);
                    break;

                default:
                    throw new NotImplementedException($"Node type {node.NodeType} not implemented");
            }

            context.NodeStates[node.Id] = NodeExecutionState.Completed;

            // Execute downstream nodes
            var outgoingEdges = workflow.Edges.Where(e => e.SourceNodeId == node.Id).ToList();
            foreach (var edge in outgoingEdges)
            {
                var targetNode = workflow.Nodes.FirstOrDefault(n => n.Id == edge.TargetNodeId);
                if (targetNode != null)
                {
                    await ExecuteNodeAsync(workflow, targetNode, context, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            context.NodeStates[node.Id] = NodeExecutionState.Failed;
            context.Logs.Add(new ExecutionLogEntry
            {
                NodeId = node.Id,
                Level = "Error",
                Message = $"Node execution failed: {ex.Message}"
            });
            throw;
        }
    }

    private Task ExecuteInputNodeAsync(WorkflowNode node, WorkflowExecutionContext context)
    {
        // Input nodes just pass through their configured values
        context.Logs.Add(new ExecutionLogEntry
        {
            NodeId = node.Id,
            Level = "Info",
            Message = "Input node executed"
        });
        return Task.CompletedTask;
    }

    private Task ExecuteOutputNodeAsync(WorkflowNode node, WorkflowExecutionContext context)
    {
        // Output nodes collect results
        context.Logs.Add(new ExecutionLogEntry
        {
            NodeId = node.Id,
            Level = "Info",
            Message = "Output node executed"
        });
        return Task.CompletedTask;
    }

    private async Task ExecuteFunctionNodeAsync(WorkflowNode node, WorkflowExecutionContext context)
    {
        if (node.FunctionId == null)
        {
            throw new InvalidOperationException("FunctionNode has no FunctionId");
        }

        // Map parameters from context variables
        var parameters = new List<object?>();
        if (node.ParameterMapping != null)
        {
            foreach (var (paramName, varName) in node.ParameterMapping)
            {
                if (context.Variables.TryGetValue(varName, out var value))
                {
                    parameters.Add(value);
                }
            }
        }

        // Invoke the function
        var result = await _assemblyLoader.InvokeFunctionAsync(node.FunctionId.Value, parameters.ToArray());

        // Map return value to context variables
        if (node.ReturnMapping != null && result != null)
        {
            foreach (var (returnName, varName) in node.ReturnMapping)
            {
                context.Variables[varName] = result;
            }
        }

        context.Logs.Add(new ExecutionLogEntry
        {
            NodeId = node.Id,
            Level = "Info",
            Message = $"Function executed successfully",
            Data = result?.ToString()
        });
    }

    private Task ExecuteAgentNodeAsync(WorkflowNode node, WorkflowExecutionContext context)
    {
        // Placeholder for agent execution
        // Will be implemented with Microsoft.Extensions.AI integration
        context.Logs.Add(new ExecutionLogEntry
        {
            NodeId = node.Id,
            Level = "Info",
            Message = "Agent node executed (placeholder)"
        });
        return Task.CompletedTask;
    }

    private Task ExecuteDecisionNodeAsync(WorkflowNode node, WorkflowExecutionContext context)
    {
        // Placeholder for decision logic
        context.Logs.Add(new ExecutionLogEntry
        {
            NodeId = node.Id,
            Level = "Info",
            Message = "Decision node executed (placeholder)"
        });
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets an active execution context by ID.
    /// </summary>
    public WorkflowExecutionContext? GetExecutionContext(Guid executionId)
    {
        _activeExecutions.TryGetValue(executionId, out var context);
        return context;
    }

    /// <summary>
    /// Cancels a running execution.
    /// </summary>
    public void CancelExecution(Guid executionId)
    {
        if (_activeExecutions.TryGetValue(executionId, out var context))
        {
            context.CancellationTokenSource.Cancel();
        }
    }
}
