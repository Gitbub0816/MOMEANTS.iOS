namespace Momeants.Mobile.Services.Haptics;

public interface IHapticsService
{
    void Light();
    void Medium();
    void Heavy();
    void Success();
    void Error();
}
