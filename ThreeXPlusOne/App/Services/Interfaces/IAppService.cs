namespace ThreeXPlusOne.App.Services.Interfaces;

public interface IAppService : IScopedService
{
    Task Run(List<string> commandParsingMessages);
}