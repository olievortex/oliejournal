namespace oliejournal.lib.Processes.DeleteOldContentProcess;

public class BackupFile
{
    public string BackupFilePath { get; set; } = string.Empty;

    public DateTime Effective
    {
        get
        {
            try
            {
                var filename = Path.GetFileName(BackupFilePath);
                var parts = filename.Split('_');
                var year = parts[0][..4];
                var month = parts[0][4..6];
                var day = parts[0][6..8];

                var hour = parts[1][..2];

                return new DateTime(
                    int.Parse(year),
                    int.Parse(month),
                    int.Parse(day),
                    int.Parse(hour),
                    0,
                    0,
                    DateTimeKind.Utc);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }

    public bool IsCompressed => BackupFilePath.EndsWith(".gz");
}
