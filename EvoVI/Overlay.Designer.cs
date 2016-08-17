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
            this.ConfigWatcher = new System.IO.FileSystemWatcher();
            this.GameDataWatcher = new System.IO.FileSystemWatcher();
            ((System.ComponentModel.ISupportInitialize)(this.ConfigWatcher)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GameDataWatcher)).BeginInit();
            this.SuspendLayout();
            // 
            // ConfigWatcher
            // 
            this.ConfigWatcher.EnableRaisingEvents = true;
            this.ConfigWatcher.Filter = "sw.cfg";
            this.ConfigWatcher.NotifyFilter = ((System.IO.NotifyFilters)((System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.CreationTime)));
            this.ConfigWatcher.Path = "C:\\sw3dg\\EvochronMercenary";
            this.ConfigWatcher.SynchronizingObject = this;
            this.ConfigWatcher.Changed += new System.IO.FileSystemEventHandler(this.ConfigWatcher_Changed);
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
            this.Location = new System.Drawing.Point(20, 20);
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
            ((System.ComponentModel.ISupportInitialize)(this.ConfigWatcher)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GameDataWatcher)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.IO.FileSystemWatcher ConfigWatcher;
        private System.IO.FileSystemWatcher GameDataWatcher;

    }
}

