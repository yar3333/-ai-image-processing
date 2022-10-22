﻿using AiPainter.Helpers;
using System.Drawing.Imaging;
using AiPainter.Controls;

namespace AiPainter.Adapters.InvokeAi
{
    public partial class InvokeAiPanel : UserControl
    {
        private SmartPictureBox? pictureBox;

        public bool InProcess;

        public InvokeAiPanel()
        {
            InitializeComponent();
        }

        private void btGenerate_Click(object sender, EventArgs e)
        {
            if (InProcess)
            {
                InvokeAiClient.Cancel();
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

            var activeBox = pictureBox!.ActiveBox;

            var originalImage = pictureBox.Image!;

            if (!cbUseInitImage.Checked)
            {
                generate(null, _ => InProcess = pbIterations.Value < pbIterations.Maximum);
            }
            else if (activeBox.X == 0 && activeBox.Y == 0 && activeBox.Width == originalImage.Width && activeBox.Height == originalImage.Height)
            {
                generate(pictureBox.GetMaskedImage(0), _ => InProcess = pbIterations.Value < pbIterations.Maximum);
            }
            else
            {
                originalImage = BitmapTools.Clone(originalImage)!;
                var fullMaskedImage = pictureBox.GetMaskedImage(0)!;

                var croppedMaskedImage = BitmapTools.GetCropped(fullMaskedImage, -activeBox.X, -activeBox.Y, activeBox.Width, activeBox.Height, Color.Transparent)!;
                generate(croppedMaskedImage, resultFilePath =>
                {
                    try
                    {
                        var resultImage = BitmapTools.Load(resultFilePath)!;
                        resultImage = BitmapTools.ResizeIfNeed(resultImage, croppedMaskedImage.Width, croppedMaskedImage.Height)!;
                        BitmapTools.DrawBitmapAtPos(resultImage, originalImage, -activeBox.X, -activeBox.Y);
                        originalImage.Save(resultFilePath, ImageFormat.Png);
                        originalImage.Dispose();
                        resultImage.Dispose();
                    }
                    catch (Exception ee)
                    {
                        InvokeAiClient.Log.WriteLine(ee.ToString());
                    }

                    InProcess = pbIterations.Value < pbIterations.Maximum;
                });
            }
        }

        private void generate(Bitmap? image, Action<string> onGenerated)
        {
            var sdImage = new AiImageInfo
            {
                prompt = tbPrompt.Text.Trim() != "" ? tbPrompt.Text.Trim() : null,
                cfg_scale = numCfgScale.Value,
                gfpgan_strength = numGfpGan.Value,
                iterations = (int)numIterations.Value,
                seed = tbSeed.Text.Trim() == "" ? -1 : long.Parse(tbSeed.Text.Trim()),
                steps = (int)numSteps.Value,
                strength = numImg2img.Value,
                initimg = BitmapTools.GetBase64String(BitmapTools.ResizeIfNeed(image, 512, 512)),
            };

            InvokeAiClient.Generate(sdImage, progress =>
            {
                Invoke(() =>
                {
                    switch (progress.@event)
                    {
                        case "step":
                            pbSteps.Value = progress.step ?? 0;
                            pbSteps.CustomText = pbSteps.Value + " / " + sdImage.steps;
                            pbSteps.Refresh();
                            break;                    
                    
                        case "result":
                            pbSteps.Value = 0;
                            pbSteps.Refresh();
                            pbIterations.Value++;
                            pbIterations.CustomText = pbIterations.Value + " / " + sdImage.iterations;
                            pbIterations.Refresh();

                            var resultFilePath = Path.Combine(Program.Config.InvokeAiOutputFolderPath, progress.url!.Split('/', '\\').Last());
                            Task.Run(async () =>
                            {
                                while (!File.Exists(resultFilePath))
                                {
                                    if (!InProcess) return;
                                    await Task.Delay(500);
                                }
                                await Task.Delay(1000);
                                onGenerated(resultFilePath);
                            });
                            break;

                        case "canceled":
                            pbSteps.Value = 0;
                            pbSteps.CustomText = "";
                            InProcess = false;
                            break;
                    }
                });
            });
        }

        private void btReset_Click(object sender, EventArgs e)
        {
            var sdImage = new AiImageInfo();
            numImg2img.Value = sdImage.strength;
            numCfgScale.Value = sdImage.cfg_scale;
            numGfpGan.Value = sdImage.gfpgan_strength;
            numSteps.Value = sdImage.steps;
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

            btGenerate.Text = InProcess ? "CANCEL" 
                           : isPortOpen ? "Generate" 
              : InvokeAiProcess.Loading ? "LOADING..." 
                                        : "ERROR";

            tbPrompt.Enabled = !InProcess;

            btGenerate.Enabled = isPortOpen;
        }
    }
}
