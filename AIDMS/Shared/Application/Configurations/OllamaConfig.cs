namespace AIDMS.Shared.Application.Configurations
{
    public class OllamaConfig
    {
        public string OllamaUrl { get; set; }

        public string InferenceModel { get; set; }

        public string EmbedingModel { get; set; }

        public float Temperature { get; set; }

        public float TopP { get; set; }

        public int TopK { get; set; }

        public int MaxTokens { get; set; }

        public int NumContext { get; set; }

        public int NumCpus { get; set; }
    }
}
