namespace Momeants.Mobile.Services.Haptics;

public class HapticsService : IHapticsService
{
    public void Light() => HapticFeedback.Default.Perform(HapticFeedbackType.Click);
    public void Medium() => HapticFeedback.Default.Perform(HapticFeedbackType.Click);
    public void Heavy() => HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
    public void Success() => HapticFeedback.Default.Perform(HapticFeedbackType.Click);
    public void Error() => HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
}
