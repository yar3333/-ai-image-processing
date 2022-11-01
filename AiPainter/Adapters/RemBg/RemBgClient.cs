﻿using System.Drawing.Imaging;
using System.Net;
using System.Net.Http.Headers;

namespace AiPainter.Adapters.RemBg;

static class StableDiffusionClient
{
    public static readonly Log Log = new("rembg");

    public static async Task<Bitmap?> RunAsync(Bitmap? image)
    {
        if (image == null) return null;

        var httpClient = new HttpClient(new LoggerHttpClientHandler(Log))
        {
            BaseAddress = new Uri(Program.Config.RemBgUrl)
        };

        using var memBufferImage = new MemoryStream();
        image.Save(memBufferImage, ImageFormat.Png);
        memBufferImage.Position = 0;
        var fileContent = new StreamContent(memBufferImage);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        using var multipartFormContent = new MultipartFormDataContent
        {
            { fileContent, "file", "image.png" },
        };
        var response = await httpClient.PostAsync("/", multipartFormContent);

        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception(await response.Content.ReadAsStringAsync());

        return (Bitmap)Image.FromStream(await response.Content.ReadAsStreamAsync());
    }
}