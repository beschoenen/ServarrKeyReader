using System.Net;
using System.Text;
using k8s;
using k8s.Autorest;
using k8s.Exceptions;
using k8s.Models;

namespace ServarrKeyReader;

public class KubernetesHelper
{
    private static string Namespace => Environment.GetEnvironmentVariable("KUBERNETES_NAMESPACE") ?? "default";

    private static string AppName => Environment.GetEnvironmentVariable("SERVARR_APP_NAME")
                                     ?? throw new KeyNotFoundException("Please set the SERVARR_APP_NAME env variable");

    private static bool AppMode => Environment.GetEnvironmentVariable("APP_MODE") == "true";

    private static string SecretName
    {
        get
        {
            var envVar = Environment.GetEnvironmentVariable("KUBERNETES_SECRETNAME");

            if (envVar != null) {
                return envVar;
            }

            return AppMode ? AppName.ToLower() + "-api-key" : "servarr-api-keys";
        }
    }

    private readonly Kubernetes _client = CreateClient();

    public void WriteSecret(string apiKey)
    {
        var secret = ReadExistingSecret();

        if (secret == null) {
            secret = FillApiKey(CreateEmptySecret(), apiKey);
            _client.CreateNamespacedSecret(secret, Namespace);
        }
        else if (GetApiKey(secret) != apiKey) {
            secret = FillApiKey(secret, apiKey);
            _client.ReplaceNamespacedSecret(secret, SecretName, Namespace);
        }
    }

    private static Kubernetes CreateClient()
    {
        KubernetesClientConfiguration? config;

        try {
            config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
        }
        catch (KubeConfigException) {
            config = KubernetesClientConfiguration.InClusterConfig();
        }
        catch {
            throw new KubernetesException("Could not connect to kubernetes cluster");
        }

        return new Kubernetes(config);
    }

    private V1Secret? ReadExistingSecret()
    {
        try {
            return _client.ReadNamespacedSecret(SecretName, Namespace);
        }
        catch (HttpOperationException e) {
            if (e.Response.StatusCode == HttpStatusCode.NotFound) {
                return null;
            }

            throw;
        }
    }

    private static V1Secret CreateEmptySecret()
    {
        return new V1Secret
        {
            ApiVersion = $"{V1Secret.KubeGroup}/{V1Secret.KubeApiVersion}",
            Kind = V1Secret.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = SecretName,
            },
        };
    }

    private static V1Secret FillApiKey(V1Secret secret, string apiKey)
    {
        secret.Data ??= new Dictionary<string, byte[]>();
        secret.Data[AppMode ? "API_KEY" : AppName.ToUpper()] = Encoding.Default.GetBytes(apiKey);

        return secret;
    }

    private static string? GetApiKey(V1Secret secret)
    {
        return secret.Data.TryGetValue(AppMode ? "API_KEY" : AppName.ToUpper(), out var value)
            ? Encoding.Default.GetString(value)
            : null;
    }
}