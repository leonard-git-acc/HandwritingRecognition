namespace HandwritingRecognition
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.train_button = new System.Windows.Forms.Button();
            this.outputNum_label = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.output_label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // train_button
            // 
            this.train_button.Location = new System.Drawing.Point(477, 361);
            this.train_button.Name = "train_button";
            this.train_button.Size = new System.Drawing.Size(75, 23);
            this.train_button.TabIndex = 0;
            this.train_button.Text = "Next";
            this.train_button.UseVisualStyleBackColor = true;
            this.train_button.Click += new System.EventHandler(this.train_button_Click);
            // 
            // cost_label
            // 
            this.outputNum_label.AutoSize = true;
            this.outputNum_label.Location = new System.Drawing.Point(474, 345);
            this.outputNum_label.Name = "cost_label";
            this.outputNum_label.Size = new System.Drawing.Size(34, 13);
            this.outputNum_label.TabIndex = 1;
            this.outputNum_label.Text = "Cost: ";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(396, 361);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // output_label
            // 
            this.output_label.AutoSize = true;
            this.output_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.output_label.Location = new System.Drawing.Point(474, 9);
            this.output_label.Name = "output_label";
            this.output_label.Size = new System.Drawing.Size(0, 25);
            this.output_label.TabIndex = 3;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(564, 396);
            this.Controls.Add(this.output_label);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.outputNum_label);
            this.Controls.Add(this.train_button);
            this.DoubleBuffered = true;
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button train_button;
        private System.Windows.Forms.Label outputNum_label;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label output_label;
    }
}

