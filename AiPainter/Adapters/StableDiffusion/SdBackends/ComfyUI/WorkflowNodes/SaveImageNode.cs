﻿namespace AiPainter.Adapters.StableDiffusion.SdBackends.ComfyUI.WorkflowNodes;

[Serializable]
class SaveImageNode : BaseNode
{
    public override ComfyUiNodeType NodeType => ComfyUiNodeType.SaveImage;
    
    public object[]? images { get; set; } // [ "8", 0 ]

    public string filename_prefix { get; set; } = ""; // "ComfyUI",
}
