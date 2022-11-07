﻿using AiPainter.Adapters.StableDiffusion;

namespace AiPainter.Controls
{
    public partial class GenerationList : UserControl
    {
        private readonly List<GenerationListItem> items = new();

        public GenerationList()
        {
            InitializeComponent();            
            
            ControlRemoved += (_, e) =>
            {
                items.RemoveAll(x => x == e.Control);
                arrangeItems();
            };
        }

        public void AddGeneration(StableDiffusionPanel sdPanel, SmartPictureBox pictureBox, MainForm mainForm)
        {
            var item = new GenerationListItem();
            item.Init(sdPanel, pictureBox, mainForm);
            item.Width = ClientSize.Width;
            item.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

            items.Add(item);
            item.Parent = this;

            arrangeItems();

            ScrollControlIntoView(item);
        }

        private void arrangeItems()
        {
            for (var i = 0; i < items.Count; i++)
            {
                items[i].Top = i * items[i].ClientSize.Height - VerticalScroll.Value;
            }
        }

        private void stateManager_Tick(object sender, EventArgs e)
        {
            items.RemoveAll(x => x.IsDisposed);

            if (items.Any(x => x.InProcess)) return;
            var item = items.Find(x => x.ImagesdInQueue > 0);
            item?.Run();
        }
    }
}
