namespace ThreeXPlusOne.App.Services.Interfaces;

public interface IMarkupService : ISingletonService
{
    string GetDecoratedMessage(string message);
}