﻿using AiPainter.Helpers;
using System.Drawing.Imaging;
using AiPainter.Controls;

namespace AiPainter.Adapters.StableDiffusion
{
    public partial class StableDiffusionPanel : UserControl
    {
        private SmartPictureBox pictureBox = null!;

        public bool InProcess;

        public StableDiffusionPanel()
        {
            InitializeComponent();
        }

        private void btGenerate_Click(object sender, EventArgs e)
        {
            if (InProcess)
            {
                StableDiffusionClient.Cancel();
                InProcess = false;
                return;
            }

            if (tbPrompt.Text.Trim() == "") { tbPrompt.Focus(); return; }

            InProcess = true;

            pbIterations.Maximum = (int)numIterations.Value;
            pbIterations.Value = 0;
            pbIterations.CustomText = "0 / " + (int)numIterations.Value;
            pbIterations.Refresh();

            pbSteps.Value = 0;
            pbSteps.Maximum = (int)numSteps.Value;
            pbSteps.CustomText = "0 / " + (int)numSteps.Value;
            pbSteps.Refresh();

            if (!cbUseInitImage.Checked)
            {
                generate(null, null, (resultImage, resultFilePath) => 
                {
                    resultImage.Save(resultFilePath, ImageFormat.Png);
                    resultImage.Dispose();
                    
                    InProcess = pbIterations.Value < pbIterations.Maximum;
                });
            }
            else
            {
                var activeBox = pictureBox.ActiveBox;
                var originalImage = BitmapTools.Clone(pictureBox.Image!);

                using var croppedImage = BitmapTools.GetCropped(originalImage, activeBox, Color.Black);
                using var croppedMask = pictureBox.GetMaskCropped(Color.Black, Color.White);
                using var image512 = BitmapTools.GetResized(croppedImage, 512, 512);
                using var mask512 = croppedMask != null ? BitmapTools.GetResized(croppedMask, 512, 512) : null;
                
                generate(image512, mask512, (resultImage, resultFilePath) =>
                {
                    try
                    {
                        using var resultImageResized = BitmapTools.GetResized(resultImage, activeBox.Width, activeBox.Height)!;
                        resultImage.Dispose();
                        
                        using var tempOriginalImage = BitmapTools.Clone(originalImage);
                        BitmapTools.DrawBitmapAtPos(resultImageResized, tempOriginalImage, activeBox.X, activeBox.Y);
                        tempOriginalImage.Save(resultFilePath, ImageFormat.Png);
                    }
                    catch (Exception ee)
                    {
                        StableDiffusionClient.Log.WriteLine(ee.ToString());
                    }

                    InProcess = pbIterations.Value < pbIterations.Maximum;
                    if (!InProcess) originalImage.Dispose();
                });
            }
        }

        private void generate(Bitmap? initImage, Bitmap? maskImage, Action<Bitmap, string> onGenerated)
        {
            if (initImage == null)
            {
                var parameters = new SdGenerationRequest
                {
                    prompt = tbPrompt.Text.Trim() != "" ? tbPrompt.Text.Trim() : "",
                    cfg_scale = numCfgScale.Value,
                    n_iter = (int)numIterations.Value,
                    seed = tbSeed.Text.Trim() == "" 
                               ? -1
                               : long.Parse(tbSeed.Text.Trim()),
                    steps = (int)numSteps.Value,
                    //strength = numImg2img.Value,
                };

                StableDiffusionClient.txt2img
                (
                    parameters,
                    onProgress: ev => onProgress(parameters, ev),
                    onFinish: _ => onFinish(),
                    onSuccess: ev => onSuccess(parameters, ev, onGenerated)
                );
            }
            else
            {
                var parameters = new SdInpaintRequest
                {
                    prompt = tbPrompt.Text.Trim() != "" ? tbPrompt.Text.Trim() : "",
                    cfg_scale = numCfgScale.Value,
                    n_iter = (int)numIterations.Value,
                    seed = tbSeed.Text.Trim() == ""
                               ? -1
                               : long.Parse(tbSeed.Text.Trim()),
                    steps = (int)numSteps.Value,
                    //strength = numImg2img.Value,
                    init_images = new[] { BitmapTools.GetBase64String(initImage) },
                    mask = maskImage != null ? BitmapTools.GetBase64String(maskImage) : null,
                };

                StableDiffusionClient.img2img
                (
                    parameters,
                    onProgress: ev => onProgress(parameters, ev),
                    onFinish: _ => onFinish(),
                    onSuccess: ev => onSuccess(parameters, ev, onGenerated)
                );
            }
        }

        private void onProgress(SdBaseGenerationRequest parameters, SdGenerationProgess ev)
        {
            Invoke(() =>
            {
                pbSteps.Value = ev.state.sampling_step;
                pbSteps.CustomText = pbSteps.Value + " / " + parameters.steps;
                pbSteps.Refresh();
            });
        }

        private void onFinish()
        {
            Invoke(() =>
            {
                pbSteps.Value = 0;
                pbSteps.CustomText = "";
                InProcess = false;
            });
        }

        private void onSuccess(SdBaseGenerationRequest parameters, SdGenerationResponse ev, Action<Bitmap, string> onGenerated)
        {
            Invoke(() => {
                pbSteps.Value = 0;
                pbSteps.Refresh();
                pbIterations.Value++;
                pbIterations.CustomText = pbIterations.Value + " / " + parameters.n_iter;
                pbIterations.Refresh();

                var resultFilePath = Path.Combine(Program.Config.OutputFolder, ev.infoParsed.seed + ".png");
                var resultBitmap = BitmapTools.FromBase64(ev.images[0]);
                
                onGenerated(resultBitmap, resultFilePath);
            });
        }

        private void btReset_Click(object sender, EventArgs e)
        {
            var parameters = new SdGenerationRequest();
            //numImg2img.Value = generationParameters.strength; // TODO
            numCfgScale.Value = parameters.cfg_scale;
            numGfpGan.Value = parameters.restore_faces ? 1 : 0; // TODO
            numSteps.Value = parameters.steps;
        }

        public void UpdateState(SmartPictureBox pb, bool isPortOpen)
        {
            pictureBox = pb;

            if (pb.Image != null && pb.HasMask)
            {
                cbUseInitImage.Enabled = false;
                cbUseInitImage.Checked = true;
                numImg2img.Enabled = true;
            }
            else if (pb.Image == null)
            {
                cbUseInitImage.Enabled = false;
                cbUseInitImage.Checked = false;
                numImg2img.Enabled = false;
            }
            else
            {
                cbUseInitImage.Enabled = true;
                numImg2img.Enabled = cbUseInitImage.Checked;
            }

            btGenerate.Text =        InProcess ? "CANCEL" 
                                  : isPortOpen ? "Generate" 
              : StableDiffusionProcess.Loading ? "LOADING..." 
                                               : "ERROR";

            tbPrompt.Enabled = !InProcess;

            btGenerate.Enabled = isPortOpen;
        }
    }
}