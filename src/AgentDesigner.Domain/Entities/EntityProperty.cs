using System;

namespace AgentDesigner.Domain.Entities;

public class EntityProperty
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Property";
    public string Type { get; set; } = "String"; // String, Integer, Double, Boolean, DateTime
    public bool IsNullable { get; set; } = false;
}
