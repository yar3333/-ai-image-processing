﻿namespace AiPainter.Adapters.StableDiffusion.SdBackends.ComfyUI.WorkflowNodes;

class EmptyLatentImageInputs : IComfyNodeInputs
{
    public int width { get; set; } // 512,
    public int height { get; set; } // 512,
    public int batch_size { get; set; } // 1
}