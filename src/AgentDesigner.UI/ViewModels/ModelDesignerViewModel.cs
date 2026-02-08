using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgentDesigner.Domain.Entities;
using AgentDesigner.UI.Services;
using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics;

namespace AgentDesigner.UI.ViewModels;

public partial class ModelDesignerViewModel : ObservableObject, IDrawable
{
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private DomainModel? _currentModel;

    [ObservableProperty]
    private ObservableCollection<ModelEntity> _entities = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEntitySelected))]
    [NotifyPropertyChangedFor(nameof(SelectedEntityName))]
    [NotifyPropertyChangedFor(nameof(SelectedEntityDescription))]
    private ModelEntity? _selectedEntity;

    public bool IsEntitySelected => SelectedEntity != null;

    public string SelectedEntityName
    {
        get => SelectedEntity?.Name ?? string.Empty;
        set
        {
            if (SelectedEntity != null && SelectedEntity.Name != value)
            {
                SelectedEntity.Name = value;
                OnPropertyChanged();
                Invalidate();
            }
        }
    }

    public string SelectedEntityDescription
    {
        get => SelectedEntity?.Description ?? string.Empty;
        set
        {
            if (SelectedEntity != null && SelectedEntity.Description != value)
            {
                SelectedEntity.Description = value;
                OnPropertyChanged();
            }
        }
    }

    [ObservableProperty]
    private double _canvasZoom = 1.0;

    [ObservableProperty]
    private double _canvasOffsetX = 0;

    [ObservableProperty]
    private double _canvasOffsetY = 0;

    public event EventHandler? RequestInvalidate;

    public ModelDesignerViewModel(NavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public void OnNavigatedTo()
    {
        var model = _navigationService.GetCurrentModel();
        if (model != null)
        {
            LoadModel(model);
        }
    }

    public void LoadModel(DomainModel model)
    {
        CurrentModel = model;
        Entities = new ObservableCollection<ModelEntity>(model.Entities);
        if (Entities.Count == 0)
        {
            AddEntity();
        }
        Invalidate();
    }

    [RelayCommand]
    private void AddEntity()
    {
        if (CurrentModel == null) return;

        var entity = new ModelEntity
        {
            Name = "NewEntity",
            X = 100 + Entities.Count * 20,
            Y = 100 + Entities.Count * 20
        };

        // Add default Id property
        entity.Properties.Add(new EntityProperty { Name = "Id", Type = "Guid" });

        CurrentModel.Entities.Add(entity);
        Entities.Add(entity);
        SelectedEntity = entity;
        Invalidate();
    }

    [RelayCommand]
    private void DeleteEntity()
    {
        if (CurrentModel == null || SelectedEntity == null) return;

        CurrentModel.Entities.Remove(SelectedEntity);
        Entities.Remove(SelectedEntity);
        SelectedEntity = null;
        Invalidate();
    }

    [RelayCommand]
    private void AddProperty()
    {
        if (SelectedEntity == null) return;

        var prop = new EntityProperty
        {
            Name = "NewProperty",
            Type = "String"
        };
        SelectedEntity.Properties.Add(prop);
        OnPropertyChanged(nameof(SelectedEntity)); // Refresh list
        OnPropertyChanged(nameof(SelectedEntity)); // Refresh list
        Invalidate();
    }

    [RelayCommand]
    private void DeleteProperty(EntityProperty property)
    {
        if (SelectedEntity == null || property == null) return;

        SelectedEntity.Properties.Remove(property);
        OnPropertyChanged(nameof(SelectedEntity));
        Invalidate();
    }

    // IDrawable Implementation
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Color.FromArgb("#F0F2F5");
        canvas.FillRectangle(dirtyRect);

        canvas.SaveState();
        canvas.Translate((float)CanvasOffsetX, (float)CanvasOffsetY);
        canvas.Scale((float)CanvasZoom, (float)CanvasZoom);

        foreach (var entity in Entities)
        {
            DrawEntity(canvas, entity);
        }

        canvas.RestoreState();
    }

    private void DrawEntity(ICanvas canvas, ModelEntity entity)
    {
        float width = 200;
        float headerHeight = 40;
        float propertyHeight = 25;
        float height = headerHeight + (entity.Properties.Count * propertyHeight) + 10;

        var rect = new RectF((float)entity.X, (float)entity.Y, width, height);

        // Shadow
        canvas.FillColor = Colors.Black.WithAlpha(0.1f);
        canvas.FillRoundedRectangle(rect.X + 4, rect.Y + 4, width, height, 4);

        // Background
        canvas.FillColor = Colors.White;
        canvas.FillRoundedRectangle(rect, 4);

        // Border (Selected Highlight)
        if (entity == SelectedEntity)
        {
            canvas.StrokeColor = Colors.Orange;
            canvas.StrokeSize = 2;
        }
        else
        {
            canvas.StrokeColor = Colors.Gray;
            canvas.StrokeSize = 1;
        }
        canvas.DrawRoundedRectangle(rect, 4);

        // Header Background
        canvas.FillColor = Color.FromArgb("#E1E4E8");
        canvas.FillRoundedRectangle(rect.X, rect.Y, width, headerHeight, 4, 4, 0, 0);

        // Header Text
        canvas.FontColor = Colors.Black;
        canvas.FontSize = 14;

        canvas.DrawString(entity.Name, rect.X, rect.Y, width, headerHeight, HorizontalAlignment.Center, VerticalAlignment.Center);

        // Separator
        canvas.StrokeColor = Colors.Gray;
        canvas.StrokeSize = 1;
        canvas.DrawLine(rect.X, rect.Y + headerHeight, rect.X + width, rect.Y + headerHeight);

        // Properties
        canvas.FontColor = Colors.Black;
        canvas.FontSize = 12;


        float y = rect.Y + headerHeight + 5;
        foreach (var prop in entity.Properties)
        {
            string text = $"{prop.Name} : {prop.Type}";
            if (prop.IsNullable) text += "?";

            canvas.DrawString(text, rect.X + 10, y, width - 20, propertyHeight, HorizontalAlignment.Left, VerticalAlignment.Top);
            y += propertyHeight;
        }
    }

    private void Invalidate()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            RequestInvalidate?.Invoke(this, EventArgs.Empty);
        });
    }
}
