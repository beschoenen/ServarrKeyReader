using System.Xml;

namespace ServarrKeyReader;

public static class KeyReader
{
    private static string ConfigFolder => Environment.GetEnvironmentVariable("CONFIG_FOLDER") ?? "/config";

    private static string ConfigFile => "config.xml";

    private static string ConfigPath => Path.Combine(ConfigFolder, ConfigFile);

    public static void WatchConfigFile(Func<string, bool> callback)
    {
        using var watcher = new FileSystemWatcher(ConfigFolder);

        watcher.Created += (object source, FileSystemEventArgs e) => OnChanged(callback, true);
        watcher.Changed += (object source, FileSystemEventArgs e) => OnChanged(callback);

        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
        watcher.Filter = ConfigFile;
        watcher.EnableRaisingEvents = true;

        Console.WriteLine($"Watching config file at {ConfigPath}");

        new AutoResetEvent(false).WaitOne();
    }

    private static string? ReadApiKey()
    {
        if (!File.Exists(ConfigPath)) {
            return null;
        }

        var document = new XmlDocument();

        try {
            document.Load(ConfigPath);

            var apiKey = document.SelectSingleNode("/Config/ApiKey");

            return apiKey?.InnerText;
        }
        catch (XmlException) {
            Console.WriteLine("Invalid xml");
        }

        return null;
    }

    private static void OnChanged(Func<string, bool> callback, bool created = false)
    {
        Console.WriteLine(created ? "Config file created" : "Config file changed");

        var apiKey = ReadApiKey();

        if (apiKey != null) {
            callback(apiKey);
        }
    }
}