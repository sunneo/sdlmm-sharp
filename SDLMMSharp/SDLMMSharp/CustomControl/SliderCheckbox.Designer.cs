namespace SDLMMSharp.CustomControl
{
    partial class SliderCheckbox
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
            this.sdlmmControl1 = new SDLMMSharp.SharpDXControl();
            this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // sdlmmControl1
            // 
            this.sdlmmControl1.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.sdlmmControl1.BackColor = System.Drawing.SystemColors.Control;
            this.sdlmmControl1.CausesValidation = false;
            this.sdlmmControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sdlmmControl1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            this.sdlmmControl1.Location = new System.Drawing.Point(0, 0);
            this.sdlmmControl1.Margin = new System.Windows.Forms.Padding(0);
            this.sdlmmControl1.Name = "sdlmmControl1";
            this.sdlmmControl1.Selectable = true;
            this.sdlmmControl1.Size = new System.Drawing.Size(175, 43);
            this.sdlmmControl1.SmoothMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            this.sdlmmControl1.TabIndex = 0;
            this.sdlmmControl1.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            // 
            // SliderCheckbox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.sdlmmControl1);
            this.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "SliderCheckbox";
            this.Size = new System.Drawing.Size(175, 43);
            this.SizeChanged += new System.EventHandler(this.SliderCheckbox_SizeChanged);
            this.VisibleChanged += new System.EventHandler(this.SliderCheckbox_VisibleChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private SDLMMSharp.SharpDXControl sdlmmControl1;
        public System.Windows.Forms.ToolTip ToolTip;
    }
}
