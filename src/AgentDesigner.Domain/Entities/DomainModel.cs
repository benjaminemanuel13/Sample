using System.Collections.ObjectModel;

namespace AgentDesigner.Domain.Entities;

public class DomainModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "New Model";
    public string Description { get; set; } = string.Empty;
    public ObservableCollection<ModelEntity> Entities { get; set; } = [];
}
