namespace Evo_VI
{
    partial class Overlay
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
            this.lbl_OverlayTitle = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbl_OverlayTitle
            // 
            this.lbl_OverlayTitle.AutoSize = true;
            this.lbl_OverlayTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F);
            this.lbl_OverlayTitle.Location = new System.Drawing.Point(12, 9);
            this.lbl_OverlayTitle.Name = "lbl_OverlayTitle";
            this.lbl_OverlayTitle.Size = new System.Drawing.Size(227, 37);
            this.lbl_OverlayTitle.TabIndex = 0;
            this.lbl_OverlayTitle.Text = "Evo VI Overlay";
            // 
            // Overlay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Magenta;
            this.ClientSize = new System.Drawing.Size(555, 305);
            this.Controls.Add(this.lbl_OverlayTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Overlay";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Evo VI - Overlay";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.Magenta;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_OverlayTitle;
    }
}

