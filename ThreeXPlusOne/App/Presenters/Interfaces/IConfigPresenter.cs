namespace ThreeXPlusOne.App.Presenters.Interfaces;

public interface IConfigPresenter
{
    void WriteSettings(Type? type = null,
                      object? instance = null,
                      string? sectionName = null,
                      bool includeHeader = true,
                      bool isJson = false);

    void WriteConfigText();
}