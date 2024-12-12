namespace ThreeXPlusOne.App.Services.Interfaces;
public interface IAppSettingsService : IScopedService
{
    Task UpdateAppSettingsFile();
}