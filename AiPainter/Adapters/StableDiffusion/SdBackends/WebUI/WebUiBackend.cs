﻿namespace AiPainter.Adapters.StableDiffusion.SdBackends.WebUI;

class WebUiBackend : ISdBackend
{
    public void ProcessStart(string? checkpointFilePath, string? vaeFilePath)
    {
        WebUiProcess.Start(checkpointFilePath, vaeFilePath);
    }

    public void ProcessStop()
    {
        WebUiProcess.Stop();
    }

    public ISdGenerator CreateGeneratorMain(SdGenerationListItem control, string destDir)
    {
        return new WebUiGeneratorMain(control, destDir);
    }

    public ISdGenerator CreateGeneratorInpaint(SdGenerationListItem control, Bitmap originalImage, Rectangle activeBox, Bitmap? croppedMask, string originalFilePath)
    {
        return new WebUiGeneratorInpaint
        (
            control,
            originalImage,
            activeBox,
            croppedMask,
            originalFilePath
        );
    }

    public async Task<Bitmap?> UpscaleAsync(string upscaler, int resizeFactor, string imageBase64, Action<int> progressPercent, CancellationTokenSource cancellationTokenSource)
    {
        return await WebUiUpscaler.RunAsync(upscaler, resizeFactor, imageBase64, progressPercent, cancellationTokenSource);
    }

    public async Task<string?> InterrogateAsync(Bitmap image)
    {
        return await WebUiInterrogater.RunAsync(image);
    }
}
