using System.Xml;

namespace ServarrKeyReader;

public static class KeyReader
{
    private static string ConfigFolder => Environment.GetEnvironmentVariable("CONFIG_FOLDER") ?? "/config";

    private static string ConfigPath => Path.Combine(ConfigFolder, "config.xml");

    public static string ReadApiKey()
    {
        if (!File.Exists(ConfigPath)) {
            throw new FileNotFoundException($"The config file was not found at {ConfigPath}.");
        }

        var document = new XmlDocument();

        document.Load(ConfigPath);

        var apiKey = document.SelectSingleNode("/Config/ApiKey");

        if (apiKey == null) {
            throw new KeyNotFoundException("Api key was not found.");
        }

        return apiKey.InnerText;
    }
}