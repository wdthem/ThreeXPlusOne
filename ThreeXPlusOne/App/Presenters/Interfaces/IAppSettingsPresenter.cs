namespace ThreeXPlusOne.App.Presenters.Interfaces;

public interface IAppSettingsPresenter
{
    void WriteSettingsSavedMessage(bool savedSettings);
    void DisplayHeader(string header);
    bool GetConfirmation(string message);
}