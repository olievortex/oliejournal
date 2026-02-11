namespace oliejournal.lib;

public static class OlieCommon
{
    public static string Left(this string str, int length)
    {
        return str.Length <= length ? str : str[..length];
    }
}
