﻿namespace AiPainter.Controls
{
    partial class SmartPictureBox
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SmartPictureBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.DoubleBuffered = true;
            this.Name = "SmartPictureBox";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.SmartPictureBox_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SmartPictureBox_MouseDown);
            this.MouseEnter += new System.EventHandler(this.SmartPictureBox_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.SmartPictureBox_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SmartPictureBox_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SmartPictureBox_MouseUp);
            this.Resize += new System.EventHandler(this.SmartPictureBox_Resize);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
