using oliejournal.lib.Enums;

namespace oliejournal.lib.Models;

public class AudioProcessQueueItemModel
{
    public int Id { get; set; }
    public AudioProcessStepEnum Step { get; set; }
}
