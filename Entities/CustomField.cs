using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Fare;

namespace TruliooExtension.Entities;

public class CustomField : INotifyPropertyChanged
{
    [JsonIgnore]
    private readonly Random _random = new ();
    public string DataField { get; set; }
    public bool IsCustomize { get; set; }
    
    private string _staticValue;

    public string StaticValue
    {
        get => _staticValue;
        set
        {
            if (SetField(ref _staticValue, value))
            {
                SetField(ref _generateValue, value);
            }
        }
    }
    
    private string _template;

    public string Template
    {
        get => _template;
        set
        {
            if (SetField(ref _template, value))
            {
                string val = new Xeger(value, _random).Generate();
                SetField(ref _generateValue, val);
            }
        }
    }
    public string Match { get; set; }
    
    private string _generateValue;
    public string GenerateValue
    {
        get => _generateValue;
        set
        {
            _generateValue = value;
            if (IsIgnore)
            {
                _generateValue = string.Empty;
            }
        }
    }

    public bool IsIgnore { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}