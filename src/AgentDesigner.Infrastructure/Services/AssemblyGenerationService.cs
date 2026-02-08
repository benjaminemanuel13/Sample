using Mono.Cecil;
using Mono.Cecil.Cil;
using AgentDesigner.Domain.Entities;
using AgentDesigner.Domain.Enums;
using System.Diagnostics;
using System.Text;

namespace AgentDesigner.Infrastructure.Services;

/// <summary>
/// Service for generating .NET assemblies.
/// </summary>
public class AssemblyGenerationService
{
    /// <summary>
    /// Generates a workflow assembly containing an AgentWorkflow class and agent classes for each Agent node.
    /// </summary>
    /// <param name="workflow">The workflow to generate assembly from</param>
    /// <param name="outputPath">Full path where the assembly should be saved (e.g., "C:\path\to\Agent.dll")</param>
    public void GenerateWorkflowAssembly(Workflow workflow, string outputPath)
    {
        // Generate C# source code for agent classes
        var sourceCode = GenerateAgentClassesSource(workflow);

        // Write source to temporary file
        var tempDir = Path.Combine(Path.GetTempPath(), "AgentDesigner_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var sourceFile = Path.Combine(tempDir, "Agent.cs");
            File.WriteAllText(sourceFile, sourceCode);

            // Create a temporary csproj
            var csprojContent = GenerateCsProj();
            var csprojFile = Path.Combine(tempDir, "Agent.csproj");
            File.WriteAllText(csprojFile, csprojContent);

            // Compile using dotnet publish (includes all dependencies)
            var outputDir = Path.GetDirectoryName(outputPath)!;
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"publish \"{csprojFile}\" -c Release -o \"{outputDir}\" --self-contained false",
                    WorkingDirectory = tempDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Compilation failed:\n{output}\n{error}");
            }

            // Verify that Agent.dll was created
            if (!File.Exists(outputPath))
            {
                throw new InvalidOperationException($"Agent.dll was not created at {outputPath}.\nBuild output:\n{output}");
            }

            // List all DLLs that were created for debugging
            var createdDlls = Directory.GetFiles(outputDir, "*.dll");
            Console.WriteLine($"Created {createdDlls.Length} DLL(s) in {outputDir}:");
            foreach (var dll in createdDlls)
            {
                Console.WriteLine($"  - {Path.GetFileName(dll)}");
            }

            // Also check for .deps.json and .runtimeconfig.json files
            var depsJson = Path.Combine(outputDir, "Agent.deps.json");
            var runtimeConfig = Path.Combine(outputDir, "Agent.runtimeconfig.json");
            if (File.Exists(depsJson))
            {
                Console.WriteLine("  - Agent.deps.json (dependency manifest)");
            }
            if (File.Exists(runtimeConfig))
            {
                Console.WriteLine("  - Agent.runtimeconfig.json (runtime config)");
            }
        }
        finally
        {
            // Delay cleanup to ensure files are fully written
            System.Threading.Thread.Sleep(500);

            // Clean up temp directory
            try
            {
                Directory.Delete(tempDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    private string GenerateCsProj()
    {
        // Get the Azure.AI.OpenAI package location
        var azureOpenAIVersion = "2.1.0";

        return $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AssemblyName>Agent</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Azure.AI.OpenAI"" Version=""{azureOpenAIVersion}"" />
  </ItemGroup>
</Project>";
    }

    private string GenerateAgentClassesSource(Workflow workflow)
    {
        var sb = new StringBuilder();

        // Add usings
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using Azure;");
        sb.AppendLine("using Azure.AI.OpenAI;");
        sb.AppendLine("using OpenAI.Chat;");
        sb.AppendLine();

        // Generate AgentBase class
        sb.AppendLine("public abstract class AgentBase");
        sb.AppendLine("{");
        sb.AppendLine("    public AgentBase() { }");
        sb.AppendLine();
        sb.AppendLine("    public virtual void Ask(string prompt)");
        sb.AppendLine("    {");
        sb.AppendLine("        Console.WriteLine(\"Base Ask: \" + prompt);");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();

        // Generate AgentWorkflow class
        sb.AppendLine("public class AgentWorkflow");
        sb.AppendLine("{");
        sb.AppendLine("    public AgentWorkflow() { }");
        sb.AppendLine();
        sb.AppendLine("    public void Start()");
        sb.AppendLine("    {");
        sb.AppendLine("        Console.WriteLine(\"Hello From Assembly\");");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();

        // Generate agent classes for each Agent node
        var agentNodes = workflow.Nodes.Where(n => n.NodeType == NodeType.AgentNode).ToList();
        foreach (var agentNode in agentNodes)
        {
            var className = SanitizeClassName(agentNode.Name);
            if (string.IsNullOrWhiteSpace(className))
            {
                className = $"Agent_{agentNode.Id.ToString().Replace("-", "")}";
            }

            sb.AppendLine($"public class {className} : AgentBase");
            sb.AppendLine("{");
            sb.AppendLine("    private AzureOpenAIClient _client;");
            sb.AppendLine();

            // Constructor
            sb.AppendLine($"    public {className}()");
            sb.AppendLine("    {");
            sb.AppendLine("        var endpoint = Environment.GetEnvironmentVariable(\"AZURE_OPENAI_ENDPOINT\");");
            sb.AppendLine("        var apiKey = Environment.GetEnvironmentVariable(\"AZURE_OPENAI_API_KEY\");");
            sb.AppendLine("        _client = new AzureOpenAIClient(new Uri(endpoint!), new AzureKeyCredential(apiKey!));");
            sb.AppendLine("    }");
            sb.AppendLine();

            // Ask method
            sb.AppendLine("    public override void Ask(string prompt)");
            sb.AppendLine("    {");
            sb.AppendLine("        var deployment = Environment.GetEnvironmentVariable(\"AZURE_OPENAI_CHAT_DEPLOYMENT_NAME\");");
            sb.AppendLine("        var chatClient = _client.GetChatClient(deployment!);");
            sb.AppendLine();
            sb.AppendLine("        var messages = new List<ChatMessage>");
            sb.AppendLine("        {");
            sb.AppendLine("            ChatMessage.CreateUserMessage(prompt)");
            sb.AppendLine("        };");
            sb.AppendLine();
            sb.AppendLine("        var response = chatClient.CompleteChat(messages);");
            sb.AppendLine("        Console.WriteLine(response.Value.Content[0].Text);");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string SanitizeClassName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        // Remove invalid characters and replace spaces with underscores
        var sanitized = new string(name
            .Replace(" ", "_")
            .Where(c => char.IsLetterOrDigit(c) || c == '_')
            .ToArray());

        // Ensure it starts with a letter or underscore
        if (sanitized.Length > 0 && !char.IsLetter(sanitized[0]) && sanitized[0] != '_')
        {
            sanitized = "_" + sanitized;
        }

        return sanitized;
    }
}
