namespace nsteam.ConfigServer.DemoWinForm
{
    partial class Form1
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
            this.btnTree = new System.Windows.Forms.Button();
            this.txtQuery1 = new System.Windows.Forms.TextBox();
            this.txtResults1 = new System.Windows.Forms.TextBox();
            this.btnSave1 = new System.Windows.Forms.Button();
            this.btnNode = new System.Windows.Forms.Button();
            this.txtSaveResult = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnTree
            // 
            this.btnTree.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTree.Location = new System.Drawing.Point(436, 10);
            this.btnTree.Name = "btnTree";
            this.btnTree.Size = new System.Drawing.Size(75, 23);
            this.btnTree.TabIndex = 0;
            this.btnTree.Text = "Tree";
            this.btnTree.UseVisualStyleBackColor = true;
            this.btnTree.Click += new System.EventHandler(this.btnTree_Click);
            // 
            // txtQuery1
            // 
            this.txtQuery1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtQuery1.Location = new System.Drawing.Point(12, 12);
            this.txtQuery1.Name = "txtQuery1";
            this.txtQuery1.Size = new System.Drawing.Size(418, 20);
            this.txtQuery1.TabIndex = 1;
            this.txtQuery1.Text = "enviroments[@Test].applications[@LogServer]";
            // 
            // txtResults1
            // 
            this.txtResults1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtResults1.Location = new System.Drawing.Point(12, 41);
            this.txtResults1.Multiline = true;
            this.txtResults1.Name = "txtResults1";
            this.txtResults1.Size = new System.Drawing.Size(581, 349);
            this.txtResults1.TabIndex = 2;
            // 
            // btnSave1
            // 
            this.btnSave1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSave1.Location = new System.Drawing.Point(12, 398);
            this.btnSave1.Name = "btnSave1";
            this.btnSave1.Size = new System.Drawing.Size(75, 23);
            this.btnSave1.TabIndex = 3;
            this.btnSave1.Text = "Save";
            this.btnSave1.UseVisualStyleBackColor = true;
            this.btnSave1.Click += new System.EventHandler(this.btnSave1_Click);
            // 
            // btnNode
            // 
            this.btnNode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNode.Location = new System.Drawing.Point(518, 10);
            this.btnNode.Name = "btnNode";
            this.btnNode.Size = new System.Drawing.Size(75, 23);
            this.btnNode.TabIndex = 4;
            this.btnNode.Text = "Node";
            this.btnNode.UseVisualStyleBackColor = true;
            this.btnNode.Click += new System.EventHandler(this.btnNode_Click);
            // 
            // txtSaveResult
            // 
            this.txtSaveResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSaveResult.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSaveResult.Location = new System.Drawing.Point(441, 400);
            this.txtSaveResult.Name = "txtSaveResult";
            this.txtSaveResult.ReadOnly = true;
            this.txtSaveResult.Size = new System.Drawing.Size(152, 20);
            this.txtSaveResult.TabIndex = 5;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(605, 431);
            this.Controls.Add(this.txtSaveResult);
            this.Controls.Add(this.btnNode);
            this.Controls.Add(this.btnSave1);
            this.Controls.Add(this.txtResults1);
            this.Controls.Add(this.txtQuery1);
            this.Controls.Add(this.btnTree);
            this.Name = "Form1";
            this.Text = "Demo ConfigServer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnTree;
        private System.Windows.Forms.TextBox txtQuery1;
        private System.Windows.Forms.TextBox txtResults1;
        private System.Windows.Forms.Button btnSave1;
        private System.Windows.Forms.Button btnNode;
        private System.Windows.Forms.TextBox txtSaveResult;
    }
}

