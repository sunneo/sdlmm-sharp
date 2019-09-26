namespace SDLMMSharp.CustomControl
{
    partial class ProgressSlider
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.sdlmmControl1 = new SDLMMSharp.SDLMMControl();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // sdlmmControl1
            // 
            this.sdlmmControl1.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.sdlmmControl1.CausesValidation = false;
            this.sdlmmControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sdlmmControl1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            this.sdlmmControl1.Location = new System.Drawing.Point(0, 0);
            this.sdlmmControl1.Margin = new System.Windows.Forms.Padding(0);
            this.sdlmmControl1.Name = "sdlmmControl1";
            this.sdlmmControl1.Size = new System.Drawing.Size(752, 53);
            this.sdlmmControl1.SmoothMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            this.sdlmmControl1.TabIndex = 0;
            // 
            // ProgressSlider
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.sdlmmControl1);
            this.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ProgressSlider";
            this.Size = new System.Drawing.Size(752, 53);
            this.SizeChanged += new System.EventHandler(this.ProgressSlider_SizeChanged);
            this.VisibleChanged += new System.EventHandler(this.ProgressSlider_VisibleChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private SDLMMSharp.SDLMMControl sdlmmControl1;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
