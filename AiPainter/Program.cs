using System.Text.Json;
using AiPainter.Adapters.LamaCleaner;
using AiPainter.Adapters.StableDiffusion.SdBackends;
using AiPainter.Adapters.StableDiffusion.SdCheckpointStuff;
using AiPainter.Adapters.StableDiffusion.SdVaeStuff;
using AiPainter.Helpers;
using AiPainter.SiteClients.CivitaiClientStuff;

namespace AiPainter;

static class Program
{
    public static readonly GlobalConfig Config = GlobalConfig.Load();
    public static readonly Log Log = new("_general");
    public static readonly Job Job = new();

    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            SdBackend.Instance.ProcessStart(SdCheckpointsHelper.GetPathToMainCheckpoint(Config.StableDiffusionCheckpoint), SdVaeHelper.GetPathToVae(Config.StableDiffusionVae));
            LamaCleanerProcess.Start();

            ApplicationConfiguration.Initialize();

            var form = new MainForm();
            Config.MainWindowPosition?.ApplyToForm(form);
            Application.Run(form);
            Config.Save();

            LamaCleanerProcess.Stop();
            SdBackend.Instance.ProcessStop();
        }
        else if (args.Length == 1 && args[0] == "--update-metadata-from-civitai")
        {
            CivitaiHelper.UpdateAsync(Log).Wait();
        }
        else if (args.Length == 2 && args[0] == "--embed-json-into-png")
        {
            JsonIntoPngEmbedder.ProcessDir(args[1]);
        }
    }
}
