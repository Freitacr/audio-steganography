namespace AudioSteganography
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AddDataFileButton = new System.Windows.Forms.Button();
            this.AddAudioFileButton = new System.Windows.Forms.Button();
            this.DataMultiplicityToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.InsertDataButton = new System.Windows.Forms.Button();
            this.ExtractDataButton = new System.Windows.Forms.Button();
            this.MainProgressBar = new System.Windows.Forms.ProgressBar();
            this.ProgressLabel = new System.Windows.Forms.Label();
            this.DataFileLabel = new System.Windows.Forms.Label();
            this.AudioFileLabel = new System.Windows.Forms.Label();
            this.DisplayDataFileBox = new System.Windows.Forms.TextBox();
            this.DisplayAudioFileBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // AddDataFileButton
            // 
            this.AddDataFileButton.Location = new System.Drawing.Point(12, 12);
            this.AddDataFileButton.Name = "AddDataFileButton";
            this.AddDataFileButton.Size = new System.Drawing.Size(145, 109);
            this.AddDataFileButton.TabIndex = 0;
            this.AddDataFileButton.Text = "Add Data File";
            this.AddDataFileButton.UseVisualStyleBackColor = true;
            this.AddDataFileButton.Click += new System.EventHandler(this.AddDataFileButton_Click);
            // 
            // AddAudioFileButton
            // 
            this.AddAudioFileButton.Location = new System.Drawing.Point(188, 12);
            this.AddAudioFileButton.Name = "AddAudioFileButton";
            this.AddAudioFileButton.Size = new System.Drawing.Size(141, 109);
            this.AddAudioFileButton.TabIndex = 3;
            this.AddAudioFileButton.Text = "Add Audio File";
            this.AddAudioFileButton.UseVisualStyleBackColor = true;
            this.AddAudioFileButton.Click += new System.EventHandler(this.AddAudioFileButton_Click);
            // 
            // DataMultiplicityToolTip
            // 
            this.DataMultiplicityToolTip.IsBalloon = true;
            // 
            // InsertDataButton
            // 
            this.InsertDataButton.Location = new System.Drawing.Point(12, 127);
            this.InsertDataButton.Name = "InsertDataButton";
            this.InsertDataButton.Size = new System.Drawing.Size(145, 109);
            this.InsertDataButton.TabIndex = 6;
            this.InsertDataButton.Text = "InsertData";
            this.InsertDataButton.UseVisualStyleBackColor = true;
            this.InsertDataButton.Click += new System.EventHandler(this.InsertDataButton_Click);
            // 
            // ExtractDataButton
            // 
            this.ExtractDataButton.Location = new System.Drawing.Point(188, 127);
            this.ExtractDataButton.Name = "ExtractDataButton";
            this.ExtractDataButton.Size = new System.Drawing.Size(141, 109);
            this.ExtractDataButton.TabIndex = 7;
            this.ExtractDataButton.Text = "Extract Data";
            this.ExtractDataButton.UseVisualStyleBackColor = true;
            this.ExtractDataButton.Click += new System.EventHandler(this.ExtractDataButton_Click);
            // 
            // MainProgressBar
            // 
            this.MainProgressBar.Location = new System.Drawing.Point(12, 295);
            this.MainProgressBar.Name = "MainProgressBar";
            this.MainProgressBar.Size = new System.Drawing.Size(520, 33);
            this.MainProgressBar.TabIndex = 8;
            // 
            // ProgressLabel
            // 
            this.ProgressLabel.AutoSize = true;
            this.ProgressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ProgressLabel.Location = new System.Drawing.Point(12, 272);
            this.ProgressLabel.Name = "ProgressLabel";
            this.ProgressLabel.Size = new System.Drawing.Size(76, 20);
            this.ProgressLabel.TabIndex = 9;
            this.ProgressLabel.Text = "Progress:";
            // 
            // DataFileLabel
            // 
            this.DataFileLabel.AutoSize = true;
            this.DataFileLabel.Location = new System.Drawing.Point(336, 13);
            this.DataFileLabel.Name = "DataFileLabel";
            this.DataFileLabel.Size = new System.Drawing.Size(97, 13);
            this.DataFileLabel.TabIndex = 10;
            this.DataFileLabel.Text = "Selected Data File:";
            // 
            // AudioFileLabel
            // 
            this.AudioFileLabel.AutoSize = true;
            this.AudioFileLabel.Location = new System.Drawing.Point(336, 127);
            this.AudioFileLabel.Name = "AudioFileLabel";
            this.AudioFileLabel.Size = new System.Drawing.Size(101, 13);
            this.AudioFileLabel.TabIndex = 11;
            this.AudioFileLabel.Text = "Selected Audio File:";
            // 
            // DisplayDataFileBox
            // 
            this.DisplayDataFileBox.Location = new System.Drawing.Point(339, 30);
            this.DisplayDataFileBox.Multiline = true;
            this.DisplayDataFileBox.Name = "DisplayDataFileBox";
            this.DisplayDataFileBox.ReadOnly = true;
            this.DisplayDataFileBox.Size = new System.Drawing.Size(193, 91);
            this.DisplayDataFileBox.TabIndex = 14;
            // 
            // DisplayAudioFileBox
            // 
            this.DisplayAudioFileBox.Location = new System.Drawing.Point(339, 145);
            this.DisplayAudioFileBox.Multiline = true;
            this.DisplayAudioFileBox.Name = "DisplayAudioFileBox";
            this.DisplayAudioFileBox.ReadOnly = true;
            this.DisplayAudioFileBox.Size = new System.Drawing.Size(193, 91);
            this.DisplayAudioFileBox.TabIndex = 15;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 339);
            this.Controls.Add(this.DisplayAudioFileBox);
            this.Controls.Add(this.DisplayDataFileBox);
            this.Controls.Add(this.AudioFileLabel);
            this.Controls.Add(this.DataFileLabel);
            this.Controls.Add(this.ProgressLabel);
            this.Controls.Add(this.MainProgressBar);
            this.Controls.Add(this.ExtractDataButton);
            this.Controls.Add(this.InsertDataButton);
            this.Controls.Add(this.AddAudioFileButton);
            this.Controls.Add(this.AddDataFileButton);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button AddDataFileButton;
        private System.Windows.Forms.Button AddAudioFileButton;
        private System.Windows.Forms.ToolTip DataMultiplicityToolTip;
        private System.Windows.Forms.Button InsertDataButton;
        private System.Windows.Forms.Button ExtractDataButton;
        private System.Windows.Forms.ProgressBar MainProgressBar;
        private System.Windows.Forms.Label ProgressLabel;
        private System.Windows.Forms.Label DataFileLabel;
        private System.Windows.Forms.Label AudioFileLabel;
        private System.Windows.Forms.TextBox DisplayDataFileBox;
        private System.Windows.Forms.TextBox DisplayAudioFileBox;
    }
}