using ServarrKeyReader;

var apiKey = KeyReader.ReadApiKey();

KubernetesHelper.WriteSecret(apiKey);
