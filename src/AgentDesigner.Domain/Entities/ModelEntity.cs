using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace AgentDesigner.Domain.Entities;

public partial class ModelEntity : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty]
    private string _name = "Entity";

    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    [ObservableProperty]
    private string _description = string.Empty;

    public ObservableCollection<EntityProperty> Properties { get; set; } = [];
}
