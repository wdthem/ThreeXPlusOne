namespace ThreeXPlusOne.App.Interfaces.Services;

public interface IAppSettingsService : IScopedService
{
    Task UpdateAppSettingsFile();
}