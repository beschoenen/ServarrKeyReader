using ServarrKeyReader;

var kubernetesHelper = new KubernetesHelper();

KeyReader.WatchConfigFile(apiKey => {
    kubernetesHelper.WriteSecret(apiKey);

    return true;
});

