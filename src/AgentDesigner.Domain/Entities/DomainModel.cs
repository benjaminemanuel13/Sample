using System;
using System.Collections.Generic;

namespace AgentDesigner.Domain.Entities;

public class DomainModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "New Model";
    public string Description { get; set; } = string.Empty;
    public List<ModelEntity> Entities { get; set; } = new();
}
