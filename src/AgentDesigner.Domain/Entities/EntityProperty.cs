using CommunityToolkit.Mvvm.ComponentModel;

namespace AgentDesigner.Domain.Entities;

public partial class EntityProperty : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty]
    private string _name = "Property";

    [ObservableProperty]
    private string _type = "String"; // String, Integer, Double, Boolean, DateTime

    [ObservableProperty]
    private bool _isNullable = false;
}
