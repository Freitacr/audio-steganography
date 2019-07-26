namespace AudioSteganography
{
    partial class PasswordEntryForm
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
            this.Password1Box = new System.Windows.Forms.TextBox();
            this.Password2Box = new System.Windows.Forms.TextBox();
            this.Password1Label = new System.Windows.Forms.Label();
            this.Password2Label = new System.Windows.Forms.Label();
            this.OkButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Password1Box
            // 
            this.Password1Box.Location = new System.Drawing.Point(12, 51);
            this.Password1Box.Name = "Password1Box";
            this.Password1Box.Size = new System.Drawing.Size(171, 20);
            this.Password1Box.TabIndex = 0;
            this.Password1Box.UseSystemPasswordChar = true;
            // 
            // Password2Box
            // 
            this.Password2Box.Location = new System.Drawing.Point(231, 51);
            this.Password2Box.Name = "Password2Box";
            this.Password2Box.Size = new System.Drawing.Size(171, 20);
            this.Password2Box.TabIndex = 1;
            this.Password2Box.UseSystemPasswordChar = true;
            // 
            // Password1Label
            // 
            this.Password1Label.AutoSize = true;
            this.Password1Label.Location = new System.Drawing.Point(13, 32);
            this.Password1Label.Name = "Password1Label";
            this.Password1Label.Size = new System.Drawing.Size(65, 13);
            this.Password1Label.TabIndex = 0;
            this.Password1Label.Text = "Password 1:";
            // 
            // Password2Label
            // 
            this.Password2Label.AutoSize = true;
            this.Password2Label.Location = new System.Drawing.Point(231, 31);
            this.Password2Label.Name = "Password2Label";
            this.Password2Label.Size = new System.Drawing.Size(65, 13);
            this.Password2Label.TabIndex = 3;
            this.Password2Label.Text = "Password 2:";
            // 
            // OkButton
            // 
            this.OkButton.Location = new System.Drawing.Point(85, 88);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(245, 42);
            this.OkButton.TabIndex = 3;
            this.OkButton.TabStop = false;
            this.OkButton.Text = "Ok";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // PasswordEntryForm
            // 
            this.AcceptButton = this.OkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(414, 142);
            this.Controls.Add(this.OkButton);
            this.Controls.Add(this.Password2Label);
            this.Controls.Add(this.Password1Label);
            this.Controls.Add(this.Password2Box);
            this.Controls.Add(this.Password1Box);
            this.Name = "PasswordEntryForm";
            this.Text = "Password Input";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Password1Box;
        private System.Windows.Forms.TextBox Password2Box;
        private System.Windows.Forms.Label Password1Label;
        private System.Windows.Forms.Label Password2Label;
        private System.Windows.Forms.Button OkButton;
    }
}

