namespace EvoVI
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
            this.GameConfigWatcher = new System.IO.FileSystemWatcher();
            this.GameDataWatcher = new System.IO.FileSystemWatcher();
            ((System.ComponentModel.ISupportInitialize)(this.GameConfigWatcher)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GameDataWatcher)).BeginInit();
            this.SuspendLayout();
            // 
            // GameConfigWatcher
            // 
            this.GameConfigWatcher.EnableRaisingEvents = true;
            this.GameConfigWatcher.Filter = "sw.cfg";
            this.GameConfigWatcher.NotifyFilter = ((System.IO.NotifyFilters)((System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.CreationTime)));
            this.GameConfigWatcher.Path = "C:\\sw3dg\\EvochronMercenary";
            this.GameConfigWatcher.SynchronizingObject = this;
            this.GameConfigWatcher.Changed += new System.IO.FileSystemEventHandler(this.ConfigWatcher_Changed);
            // 
            // GameDataWatcher
            // 
            this.GameDataWatcher.EnableRaisingEvents = true;
            this.GameDataWatcher.Filter = "savedata.txt";
            this.GameDataWatcher.NotifyFilter = ((System.IO.NotifyFilters)((System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.CreationTime)));
            this.GameDataWatcher.Path = "C:\\sw3dg\\EvochronMercenary";
            this.GameDataWatcher.SynchronizingObject = this;
            this.GameDataWatcher.Changed += new System.IO.FileSystemEventHandler(this.GameDataWatcher_Changed);
            // 
            // Overlay
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.ClientSize = new System.Drawing.Size(600, 300);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Lucida Console", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Location = new System.Drawing.Point(40, 150);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Overlay";
            this.Opacity = 0.8D;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Evo VI - Overlay";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.Transparent;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Overlay_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.GameConfigWatcher)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GameDataWatcher)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.IO.FileSystemWatcher GameConfigWatcher;
        private System.IO.FileSystemWatcher GameDataWatcher;

    }
}

