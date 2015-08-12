using nsteam.Windows.Commons.DataGrid;
using nsteam.Windows.Commons.Grid;
namespace nsteam.ConfigServer
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
            this.leftPane = new System.Windows.Forms.Panel();
            this.tree = new nsteam.Windows.Commons.Tree.ReflectionTree();
            this.panel5 = new System.Windows.Forms.Panel();
            this.btnAddCollection = new System.Windows.Forms.Button();
            this.btnRemoveNode = new System.Windows.Forms.Button();
            this.btnAddNode = new System.Windows.Forms.Button();
            this.mainPane = new System.Windows.Forms.Panel();
            this.mainContentPane = new System.Windows.Forms.Panel();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabGrid = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.grid = new nsteam.Windows.Commons.DataGrid.DataGridViewExtension();
            this.pColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DynamicControl = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.obj = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OldProperty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabDetail = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.txtValue = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblCreatedBy = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblCreatedDateTime = new System.Windows.Forms.Label();
            this.lblModifiedBy = new System.Windows.Forms.Label();
            this.lblModifiedDateTime = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.mainBackPanel = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabBrowse = new System.Windows.Forms.TabPage();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.tabSource = new System.Windows.Forms.TabPage();
            this.txtSource = new ICSharpCode.TextEditor.TextEditorControl();
            this.panel6 = new System.Windows.Forms.Panel();
            this.btnSaveSource = new System.Windows.Forms.Button();
            this.rdAsJson = new System.Windows.Forms.RadioButton();
            this.rdAsXML = new System.Windows.Forms.RadioButton();
            this.tabQuery = new System.Windows.Forms.TabPage();
            this.textEdQuery = new ICSharpCode.TextEditor.TextEditorControl();
            this.panel7 = new System.Windows.Forms.Panel();
            this.btnGetTree = new System.Windows.Forms.Button();
            this.chObjectInfo = new System.Windows.Forms.CheckBox();
            this.lbQueryTitle = new System.Windows.Forms.Label();
            this.txtCommand = new System.Windows.Forms.TextBox();
            this.btnGetNode = new System.Windows.Forms.Button();
            this.rdQueryAsJson = new System.Windows.Forms.RadioButton();
            this.rdQueryAsXml = new System.Windows.Forms.RadioButton();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openUrlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusSpacer = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusServer = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusPristine = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusDirty = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusError = new System.Windows.Forms.ToolStripStatusLabel();
            this.miniToolStrip = new System.Windows.Forms.MenuStrip();
            this.txtPreview = new ICSharpCode.TextEditor.TextEditorControl();
            this.txtSourceNode = new ICSharpCode.TextEditor.TextEditorControl();
            this.panel8 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.panel9 = new System.Windows.Forms.Panel();
            this.btnSaveSourceNode = new System.Windows.Forms.Button();
            this.panelTopGrid = new System.Windows.Forms.Panel();
            this.panel10 = new System.Windows.Forms.Panel();
            this.splitterGrid = new System.Windows.Forms.Splitter();
            this.splitter3 = new System.Windows.Forms.Splitter();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gridToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.leftPane.SuspendLayout();
            this.panel5.SuspendLayout();
            this.mainPane.SuspendLayout();
            this.mainContentPane.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.tabGrid.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.tabDetail.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.mainBackPanel.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabBrowse.SuspendLayout();
            this.tabSource.SuspendLayout();
            this.panel6.SuspendLayout();
            this.tabQuery.SuspendLayout();
            this.panel7.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.panel8.SuspendLayout();
            this.panel9.SuspendLayout();
            this.panelTopGrid.SuspendLayout();
            this.panel10.SuspendLayout();
            this.SuspendLayout();
            // 
            // leftPane
            // 
            this.leftPane.Controls.Add(this.tree);
            this.leftPane.Controls.Add(this.panel5);
            this.leftPane.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftPane.Location = new System.Drawing.Point(3, 3);
            this.leftPane.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.leftPane.Name = "leftPane";
            this.leftPane.Size = new System.Drawing.Size(317, 530);
            this.leftPane.TabIndex = 1;
            // 
            // tree
            // 
            this.tree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tree.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tree.HideSelection = false;
            this.tree.HotTracking = true;
            this.tree.Location = new System.Drawing.Point(0, 0);
            this.tree.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tree.Name = "tree";
            this.tree.ShowNodeToolTips = true;
            this.tree.Size = new System.Drawing.Size(317, 461);
            this.tree.TabIndex = 0;
            this.tree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tree_AfterSelect);
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.btnAddCollection);
            this.panel5.Controls.Add(this.btnRemoveNode);
            this.panel5.Controls.Add(this.btnAddNode);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel5.Location = new System.Drawing.Point(0, 461);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(317, 69);
            this.panel5.TabIndex = 1;
            // 
            // btnAddCollection
            // 
            this.btnAddCollection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddCollection.Location = new System.Drawing.Point(93, 14);
            this.btnAddCollection.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnAddCollection.Name = "btnAddCollection";
            this.btnAddCollection.Size = new System.Drawing.Size(94, 36);
            this.btnAddCollection.TabIndex = 21;
            this.btnAddCollection.Text = "Add Collection";
            this.btnAddCollection.UseVisualStyleBackColor = true;
            this.btnAddCollection.Click += new System.EventHandler(this.btnAddCollection_Click);
            // 
            // btnRemoveNode
            // 
            this.btnRemoveNode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRemoveNode.Location = new System.Drawing.Point(210, 14);
            this.btnRemoveNode.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnRemoveNode.Name = "btnRemoveNode";
            this.btnRemoveNode.Size = new System.Drawing.Size(102, 36);
            this.btnRemoveNode.TabIndex = 22;
            this.btnRemoveNode.Text = "Remove Node";
            this.btnRemoveNode.UseVisualStyleBackColor = true;
            this.btnRemoveNode.Click += new System.EventHandler(this.btnRemoveNode_Click);
            // 
            // btnAddNode
            // 
            this.btnAddNode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddNode.Location = new System.Drawing.Point(11, 14);
            this.btnAddNode.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnAddNode.Name = "btnAddNode";
            this.btnAddNode.Size = new System.Drawing.Size(78, 36);
            this.btnAddNode.TabIndex = 20;
            this.btnAddNode.Text = "Add Node";
            this.btnAddNode.UseVisualStyleBackColor = true;
            this.btnAddNode.Click += new System.EventHandler(this.btnAddNode_Click);
            // 
            // mainPane
            // 
            this.mainPane.Controls.Add(this.mainContentPane);
            this.mainPane.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPane.Location = new System.Drawing.Point(320, 3);
            this.mainPane.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.mainPane.Name = "mainPane";
            this.mainPane.Size = new System.Drawing.Size(630, 530);
            this.mainPane.TabIndex = 3;
            // 
            // mainContentPane
            // 
            this.mainContentPane.Controls.Add(this.tabMain);
            this.mainContentPane.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainContentPane.Location = new System.Drawing.Point(0, 0);
            this.mainContentPane.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.mainContentPane.Name = "mainContentPane";
            this.mainContentPane.Size = new System.Drawing.Size(630, 530);
            this.mainContentPane.TabIndex = 2;
            // 
            // tabMain
            // 
            this.tabMain.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabMain.CausesValidation = false;
            this.tabMain.Controls.Add(this.tabGrid);
            this.tabMain.Controls.Add(this.tabDetail);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.ItemSize = new System.Drawing.Size(0, 1);
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Margin = new System.Windows.Forms.Padding(0);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(630, 530);
            this.tabMain.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabMain.TabIndex = 0;
            // 
            // tabGrid
            // 
            this.tabGrid.Controls.Add(this.panel3);
            this.tabGrid.Location = new System.Drawing.Point(4, 5);
            this.tabGrid.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tabGrid.Name = "tabGrid";
            this.tabGrid.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tabGrid.Size = new System.Drawing.Size(622, 521);
            this.tabGrid.TabIndex = 0;
            this.tabGrid.Text = "tabGrid";
            this.tabGrid.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.txtPreview);
            this.panel3.Controls.Add(this.splitter3);
            this.panel3.Controls.Add(this.panel10);
            this.panel3.Controls.Add(this.splitterGrid);
            this.panel3.Controls.Add(this.panelTopGrid);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(2, 3);
            this.panel3.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(618, 515);
            this.panel3.TabIndex = 6;
            // 
            // grid
            // 
            this.grid.AllowUserToOrderColumns = true;
            this.grid.AlternateRowColor = System.Drawing.Color.White;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.pColumn,
            this.DynamicControl,
            this.obj,
            this.col,
            this.OldProperty});
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.Location = new System.Drawing.Point(0, 0);
            this.grid.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid.Size = new System.Drawing.Size(618, 117);
            this.grid.TabIndex = 0;
            this.grid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellEndEdit);
            this.grid.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.grid_DataBindingComplete);
            this.grid.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.grid_KeyPress);
            // 
            // pColumn
            // 
            this.pColumn.DataPropertyName = "Property";
            this.pColumn.HeaderText = "Property";
            this.pColumn.Name = "pColumn";
            this.pColumn.Width = 150;
            // 
            // DynamicControl
            // 
            this.DynamicControl.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.DynamicControl.DataPropertyName = "Value";
            this.DynamicControl.HeaderText = "Value";
            this.DynamicControl.Name = "DynamicControl";
            this.DynamicControl.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // obj
            // 
            this.obj.DataPropertyName = "obj";
            this.obj.HeaderText = "obj";
            this.obj.Name = "obj";
            this.obj.ReadOnly = true;
            this.obj.Visible = false;
            // 
            // col
            // 
            this.col.DataPropertyName = "col";
            this.col.HeaderText = "col";
            this.col.Name = "col";
            this.col.ReadOnly = true;
            this.col.Visible = false;
            // 
            // OldProperty
            // 
            this.OldProperty.DataPropertyName = "OldProperty";
            this.OldProperty.HeaderText = "OldProperty";
            this.OldProperty.Name = "OldProperty";
            this.OldProperty.ReadOnly = true;
            this.OldProperty.Visible = false;
            // 
            // tabDetail
            // 
            this.tabDetail.Controls.Add(this.panel2);
            this.tabDetail.Controls.Add(this.panel1);
            this.tabDetail.Location = new System.Drawing.Point(4, 5);
            this.tabDetail.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tabDetail.Name = "tabDetail";
            this.tabDetail.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tabDetail.Size = new System.Drawing.Size(622, 521);
            this.tabDetail.TabIndex = 1;
            this.tabDetail.Text = "tabDetail";
            this.tabDetail.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.txtValue);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(2, 3);
            this.panel2.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(618, 453);
            this.panel2.TabIndex = 4;
            // 
            // txtValue
            // 
            this.txtValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtValue.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtValue.Location = new System.Drawing.Point(0, 0);
            this.txtValue.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtValue.Multiline = true;
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(618, 453);
            this.txtValue.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnSave);
            this.panel1.Controls.Add(this.lblCreatedBy);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.lblCreatedDateTime);
            this.panel1.Controls.Add(this.lblModifiedBy);
            this.panel1.Controls.Add(this.lblModifiedDateTime);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(2, 456);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(618, 62);
            this.panel1.TabIndex = 3;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(502, 14);
            this.btnSave.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(102, 36);
            this.btnSave.TabIndex = 17;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            // 
            // lblCreatedBy
            // 
            this.lblCreatedBy.AutoSize = true;
            this.lblCreatedBy.Location = new System.Drawing.Point(193, 14);
            this.lblCreatedBy.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCreatedBy.Name = "lblCreatedBy";
            this.lblCreatedBy.Size = new System.Drawing.Size(72, 13);
            this.lblCreatedBy.TabIndex = 16;
            this.lblCreatedBy.Text = "[lblCreatedBy]";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(169, 14);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(21, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "by:";
            // 
            // lblCreatedDateTime
            // 
            this.lblCreatedDateTime.AutoSize = true;
            this.lblCreatedDateTime.Location = new System.Drawing.Point(65, 14);
            this.lblCreatedDateTime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCreatedDateTime.Name = "lblCreatedDateTime";
            this.lblCreatedDateTime.Size = new System.Drawing.Size(106, 13);
            this.lblCreatedDateTime.TabIndex = 14;
            this.lblCreatedDateTime.Text = "[lblCreatedDateTime]";
            // 
            // lblModifiedBy
            // 
            this.lblModifiedBy.AutoSize = true;
            this.lblModifiedBy.Location = new System.Drawing.Point(221, 37);
            this.lblModifiedBy.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblModifiedBy.Name = "lblModifiedBy";
            this.lblModifiedBy.Size = new System.Drawing.Size(75, 13);
            this.lblModifiedBy.TabIndex = 13;
            this.lblModifiedBy.Text = "[lblModifiedBy]";
            // 
            // lblModifiedDateTime
            // 
            this.lblModifiedDateTime.AutoSize = true;
            this.lblModifiedDateTime.Location = new System.Drawing.Point(85, 37);
            this.lblModifiedDateTime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblModifiedDateTime.Name = "lblModifiedDateTime";
            this.lblModifiedDateTime.Size = new System.Drawing.Size(109, 13);
            this.lblModifiedDateTime.TabIndex = 12;
            this.lblModifiedDateTime.Text = "[lblModifiedDateTime]";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(197, 37);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(21, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "by:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 37);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Last Modified:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Created:";
            // 
            // mainBackPanel
            // 
            this.mainBackPanel.Controls.Add(this.tabControl1);
            this.mainBackPanel.Controls.Add(this.menuStrip1);
            this.mainBackPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainBackPanel.Location = new System.Drawing.Point(0, 0);
            this.mainBackPanel.Name = "mainBackPanel";
            this.mainBackPanel.Size = new System.Drawing.Size(961, 586);
            this.mainBackPanel.TabIndex = 4;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabBrowse);
            this.tabControl1.Controls.Add(this.tabSource);
            this.tabControl1.Controls.Add(this.tabQuery);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 24);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(961, 562);
            this.tabControl1.TabIndex = 21;
            // 
            // tabBrowse
            // 
            this.tabBrowse.Controls.Add(this.splitter1);
            this.tabBrowse.Controls.Add(this.mainPane);
            this.tabBrowse.Controls.Add(this.leftPane);
            this.tabBrowse.Location = new System.Drawing.Point(4, 22);
            this.tabBrowse.Name = "tabBrowse";
            this.tabBrowse.Padding = new System.Windows.Forms.Padding(3);
            this.tabBrowse.Size = new System.Drawing.Size(953, 536);
            this.tabBrowse.TabIndex = 0;
            this.tabBrowse.Text = "Browsing";
            this.tabBrowse.UseVisualStyleBackColor = true;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(320, 3);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 530);
            this.splitter1.TabIndex = 4;
            this.splitter1.TabStop = false;
            // 
            // tabSource
            // 
            this.tabSource.Controls.Add(this.txtSource);
            this.tabSource.Controls.Add(this.panel6);
            this.tabSource.Location = new System.Drawing.Point(4, 22);
            this.tabSource.Name = "tabSource";
            this.tabSource.Padding = new System.Windows.Forms.Padding(3);
            this.tabSource.Size = new System.Drawing.Size(953, 536);
            this.tabSource.TabIndex = 1;
            this.tabSource.Text = "Source";
            this.tabSource.UseVisualStyleBackColor = true;
            // 
            // txtSource
            // 
            this.txtSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSource.IsReadOnly = false;
            this.txtSource.Location = new System.Drawing.Point(3, 39);
            this.txtSource.Name = "txtSource";
            this.txtSource.ShowVRuler = false;
            this.txtSource.Size = new System.Drawing.Size(947, 494);
            this.txtSource.TabIndex = 0;
            this.txtSource.VRulerRow = 0;
            this.txtSource.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // panel6
            // 
            this.panel6.BackColor = System.Drawing.SystemColors.Info;
            this.panel6.Controls.Add(this.btnSaveSource);
            this.panel6.Controls.Add(this.rdAsJson);
            this.panel6.Controls.Add(this.rdAsXML);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(3, 3);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(947, 36);
            this.panel6.TabIndex = 1;
            // 
            // btnSaveSource
            // 
            this.btnSaveSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveSource.Location = new System.Drawing.Point(867, 6);
            this.btnSaveSource.Name = "btnSaveSource";
            this.btnSaveSource.Size = new System.Drawing.Size(75, 23);
            this.btnSaveSource.TabIndex = 2;
            this.btnSaveSource.Text = "Save";
            this.btnSaveSource.UseVisualStyleBackColor = true;
            this.btnSaveSource.Click += new System.EventHandler(this.btnSaveSource_Click);
            // 
            // rdAsJson
            // 
            this.rdAsJson.AutoSize = true;
            this.rdAsJson.Checked = true;
            this.rdAsJson.Location = new System.Drawing.Point(80, 9);
            this.rdAsJson.Name = "rdAsJson";
            this.rdAsJson.Size = new System.Drawing.Size(53, 17);
            this.rdAsJson.TabIndex = 1;
            this.rdAsJson.TabStop = true;
            this.rdAsJson.Text = "JSON";
            this.rdAsJson.UseVisualStyleBackColor = true;
            this.rdAsJson.CheckedChanged += new System.EventHandler(this.rdAsJson_CheckedChanged);
            // 
            // rdAsXML
            // 
            this.rdAsXML.AutoSize = true;
            this.rdAsXML.Location = new System.Drawing.Point(18, 9);
            this.rdAsXML.Name = "rdAsXML";
            this.rdAsXML.Size = new System.Drawing.Size(47, 17);
            this.rdAsXML.TabIndex = 0;
            this.rdAsXML.Text = "XML";
            this.rdAsXML.UseVisualStyleBackColor = true;
            // 
            // tabQuery
            // 
            this.tabQuery.Controls.Add(this.textEdQuery);
            this.tabQuery.Controls.Add(this.panel7);
            this.tabQuery.Location = new System.Drawing.Point(4, 22);
            this.tabQuery.Name = "tabQuery";
            this.tabQuery.Padding = new System.Windows.Forms.Padding(3);
            this.tabQuery.Size = new System.Drawing.Size(953, 536);
            this.tabQuery.TabIndex = 2;
            this.tabQuery.Text = "Query";
            this.tabQuery.UseVisualStyleBackColor = true;
            // 
            // textEdQuery
            // 
            this.textEdQuery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textEdQuery.IsReadOnly = false;
            this.textEdQuery.Location = new System.Drawing.Point(3, 41);
            this.textEdQuery.Name = "textEdQuery";
            this.textEdQuery.ShowVRuler = false;
            this.textEdQuery.Size = new System.Drawing.Size(947, 492);
            this.textEdQuery.TabIndex = 2;
            this.textEdQuery.VRulerRow = 0;
            // 
            // panel7
            // 
            this.panel7.BackColor = System.Drawing.SystemColors.Info;
            this.panel7.Controls.Add(this.btnGetTree);
            this.panel7.Controls.Add(this.chObjectInfo);
            this.panel7.Controls.Add(this.lbQueryTitle);
            this.panel7.Controls.Add(this.txtCommand);
            this.panel7.Controls.Add(this.btnGetNode);
            this.panel7.Controls.Add(this.rdQueryAsJson);
            this.panel7.Controls.Add(this.rdQueryAsXml);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel7.Location = new System.Drawing.Point(3, 3);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(947, 38);
            this.panel7.TabIndex = 3;
            // 
            // btnGetTree
            // 
            this.btnGetTree.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetTree.Location = new System.Drawing.Point(677, 5);
            this.btnGetTree.Name = "btnGetTree";
            this.btnGetTree.Size = new System.Drawing.Size(74, 23);
            this.btnGetTree.TabIndex = 7;
            this.btnGetTree.Text = "Get Tree";
            this.btnGetTree.UseVisualStyleBackColor = true;
            this.btnGetTree.Click += new System.EventHandler(this.btnGetTree_Click);
            // 
            // chObjectInfo
            // 
            this.chObjectInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chObjectInfo.AutoSize = true;
            this.chObjectInfo.Location = new System.Drawing.Point(758, 10);
            this.chObjectInfo.Name = "chObjectInfo";
            this.chObjectInfo.Size = new System.Drawing.Size(78, 17);
            this.chObjectInfo.TabIndex = 6;
            this.chObjectInfo.Text = "Object Info";
            this.chObjectInfo.UseVisualStyleBackColor = true;
            // 
            // lbQueryTitle
            // 
            this.lbQueryTitle.AutoSize = true;
            this.lbQueryTitle.Location = new System.Drawing.Point(6, 9);
            this.lbQueryTitle.Name = "lbQueryTitle";
            this.lbQueryTitle.Size = new System.Drawing.Size(35, 13);
            this.lbQueryTitle.TabIndex = 4;
            this.lbQueryTitle.Text = "Query";
            // 
            // txtCommand
            // 
            this.txtCommand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCommand.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCommand.Location = new System.Drawing.Point(50, 5);
            this.txtCommand.Name = "txtCommand";
            this.txtCommand.Size = new System.Drawing.Size(541, 23);
            this.txtCommand.TabIndex = 3;
            // 
            // btnGetNode
            // 
            this.btnGetNode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetNode.Location = new System.Drawing.Point(597, 5);
            this.btnGetNode.Name = "btnGetNode";
            this.btnGetNode.Size = new System.Drawing.Size(74, 23);
            this.btnGetNode.TabIndex = 2;
            this.btnGetNode.Text = "Get Node";
            this.btnGetNode.UseVisualStyleBackColor = true;
            this.btnGetNode.Click += new System.EventHandler(this.btnGetNode_Click);
            // 
            // rdQueryAsJson
            // 
            this.rdQueryAsJson.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.rdQueryAsJson.AutoSize = true;
            this.rdQueryAsJson.Checked = true;
            this.rdQueryAsJson.Location = new System.Drawing.Point(890, 9);
            this.rdQueryAsJson.Name = "rdQueryAsJson";
            this.rdQueryAsJson.Size = new System.Drawing.Size(53, 17);
            this.rdQueryAsJson.TabIndex = 1;
            this.rdQueryAsJson.TabStop = true;
            this.rdQueryAsJson.Text = "JSON";
            this.rdQueryAsJson.UseVisualStyleBackColor = true;
            // 
            // rdQueryAsXml
            // 
            this.rdQueryAsXml.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.rdQueryAsXml.AutoSize = true;
            this.rdQueryAsXml.Enabled = false;
            this.rdQueryAsXml.Location = new System.Drawing.Point(841, 9);
            this.rdQueryAsXml.Name = "rdQueryAsXml";
            this.rdQueryAsXml.Size = new System.Drawing.Size(47, 17);
            this.rdQueryAsXml.TabIndex = 0;
            this.rdQueryAsXml.Text = "XML";
            this.rdQueryAsXml.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(961, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openUrlToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openUrlToolStripMenuItem
            // 
            this.openUrlToolStripMenuItem.Name = "openUrlToolStripMenuItem";
            this.openUrlToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.openUrlToolStripMenuItem.Text = "Open Url";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(118, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.statusLabel,
            this.statusSpacer,
            this.statusServer,
            this.statusPristine,
            this.statusDirty,
            this.statusError});
            this.statusBar.Location = new System.Drawing.Point(0, 586);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(961, 22);
            this.statusBar.TabIndex = 5;
            this.statusBar.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(39, 17);
            this.toolStripStatusLabel1.Text = "Node:";
            // 
            // statusLabel
            // 
            this.statusLabel.IsLink = true;
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 17);
            this.statusLabel.Click += new System.EventHandler(this.statusLabel_Click);
            // 
            // statusSpacer
            // 
            this.statusSpacer.Name = "statusSpacer";
            this.statusSpacer.Size = new System.Drawing.Size(907, 17);
            this.statusSpacer.Spring = true;
            // 
            // statusServer
            // 
            this.statusServer.Name = "statusServer";
            this.statusServer.Size = new System.Drawing.Size(0, 17);
            this.statusServer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusPristine
            // 
            this.statusPristine.Image = global::nsteam.ConfigServer.Properties.Resources.accept;
            this.statusPristine.Name = "statusPristine";
            this.statusPristine.Size = new System.Drawing.Size(16, 17);
            this.statusPristine.ToolTipText = "Information saved";
            this.statusPristine.Visible = false;
            // 
            // statusDirty
            // 
            this.statusDirty.Image = global::nsteam.ConfigServer.Properties.Resources.error;
            this.statusDirty.Name = "statusDirty";
            this.statusDirty.Size = new System.Drawing.Size(16, 17);
            this.statusDirty.ToolTipText = "Changes not saved";
            this.statusDirty.Visible = false;
            // 
            // statusError
            // 
            this.statusError.Image = global::nsteam.ConfigServer.Properties.Resources.cancel;
            this.statusError.Name = "statusError";
            this.statusError.Size = new System.Drawing.Size(16, 17);
            this.statusError.Visible = false;
            // 
            // miniToolStrip
            // 
            this.miniToolStrip.AutoSize = false;
            this.miniToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.miniToolStrip.Location = new System.Drawing.Point(95, 2);
            this.miniToolStrip.Name = "miniToolStrip";
            this.miniToolStrip.Size = new System.Drawing.Size(961, 24);
            this.miniToolStrip.TabIndex = 0;
            // 
            // txtPreview
            // 
            this.txtPreview.AutoScroll = true;
            this.txtPreview.Cursor = System.Windows.Forms.Cursors.Default;
            this.txtPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPreview.IsReadOnly = false;
            this.txtPreview.Location = new System.Drawing.Point(0, 316);
            this.txtPreview.Name = "txtPreview";
            this.txtPreview.ShowVRuler = false;
            this.txtPreview.Size = new System.Drawing.Size(618, 199);
            this.txtPreview.TabIndex = 3;
            this.txtPreview.VRulerRow = 0;
            // 
            // txtSourceNode
            // 
            this.txtSourceNode.AutoScroll = true;
            this.txtSourceNode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSourceNode.IsReadOnly = false;
            this.txtSourceNode.Location = new System.Drawing.Point(0, 0);
            this.txtSourceNode.Name = "txtSourceNode";
            this.txtSourceNode.ShowVRuler = false;
            this.txtSourceNode.Size = new System.Drawing.Size(618, 123);
            this.txtSourceNode.TabIndex = 4;
            this.txtSourceNode.VRulerRow = 0;
            // 
            // panel8
            // 
            this.panel8.BackColor = System.Drawing.SystemColors.Info;
            this.panel8.Controls.Add(this.button1);
            this.panel8.Controls.Add(this.button2);
            this.panel8.Controls.Add(this.button3);
            this.panel8.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel8.Location = new System.Drawing.Point(0, 117);
            this.panel8.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(618, 33);
            this.panel8.TabIndex = 6;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(102, 5);
            this.button1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(102, 22);
            this.button1.TabIndex = 19;
            this.button1.Text = "Remove Element";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button2.Location = new System.Drawing.Point(12, 5);
            this.button2.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(86, 22);
            this.button2.TabIndex = 18;
            this.button2.Text = "Add Element";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(506, 5);
            this.button3.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(102, 22);
            this.button3.TabIndex = 17;
            this.button3.Text = "Save";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.btnSaveTree_Click);
            // 
            // panel9
            // 
            this.panel9.BackColor = System.Drawing.SystemColors.Info;
            this.panel9.Controls.Add(this.btnSaveSourceNode);
            this.panel9.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel9.Location = new System.Drawing.Point(0, 123);
            this.panel9.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panel9.Name = "panel9";
            this.panel9.Size = new System.Drawing.Size(618, 33);
            this.panel9.TabIndex = 7;
            // 
            // btnSaveSourceNode
            // 
            this.btnSaveSourceNode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveSourceNode.Location = new System.Drawing.Point(506, 5);
            this.btnSaveSourceNode.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnSaveSourceNode.Name = "btnSaveSourceNode";
            this.btnSaveSourceNode.Size = new System.Drawing.Size(102, 22);
            this.btnSaveSourceNode.TabIndex = 17;
            this.btnSaveSourceNode.Text = "Save";
            this.btnSaveSourceNode.UseVisualStyleBackColor = true;
            this.btnSaveSourceNode.Click += new System.EventHandler(this.btnSaveSourceNode_Click);
            // 
            // panelTopGrid
            // 
            this.panelTopGrid.Controls.Add(this.grid);
            this.panelTopGrid.Controls.Add(this.panel8);
            this.panelTopGrid.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTopGrid.Location = new System.Drawing.Point(0, 0);
            this.panelTopGrid.Name = "panelTopGrid";
            this.panelTopGrid.Size = new System.Drawing.Size(618, 150);
            this.panelTopGrid.TabIndex = 8;
            // 
            // panel10
            // 
            this.panel10.Controls.Add(this.txtSourceNode);
            this.panel10.Controls.Add(this.panel9);
            this.panel10.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel10.Location = new System.Drawing.Point(0, 155);
            this.panel10.Name = "panel10";
            this.panel10.Size = new System.Drawing.Size(618, 156);
            this.panel10.TabIndex = 9;
            // 
            // splitterGrid
            // 
            this.splitterGrid.BackColor = System.Drawing.SystemColors.GrayText;
            this.splitterGrid.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.splitterGrid.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitterGrid.Location = new System.Drawing.Point(0, 150);
            this.splitterGrid.Name = "splitterGrid";
            this.splitterGrid.Size = new System.Drawing.Size(618, 5);
            this.splitterGrid.TabIndex = 10;
            this.splitterGrid.TabStop = false;
            // 
            // splitter3
            // 
            this.splitter3.BackColor = System.Drawing.SystemColors.GrayText;
            this.splitter3.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.splitter3.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter3.Location = new System.Drawing.Point(0, 311);
            this.splitter3.Name = "splitter3";
            this.splitter3.Size = new System.Drawing.Size(618, 5);
            this.splitter3.TabIndex = 11;
            this.splitter3.TabStop = false;
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gridToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // gridToolStripMenuItem
            // 
            this.gridToolStripMenuItem.Name = "gridToolStripMenuItem";
            this.gridToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.gridToolStripMenuItem.Text = "Show Grid";
            this.gridToolStripMenuItem.Click += new System.EventHandler(this.gridToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(961, 608);
            this.Controls.Add(this.mainBackPanel);
            this.Controls.Add(this.statusBar);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "MainForm";
            this.Text = "Config Server Management";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.leftPane.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.mainPane.ResumeLayout(false);
            this.mainContentPane.ResumeLayout(false);
            this.tabMain.ResumeLayout(false);
            this.tabGrid.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.tabDetail.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.mainBackPanel.ResumeLayout(false);
            this.mainBackPanel.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabBrowse.ResumeLayout(false);
            this.tabSource.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.tabQuery.ResumeLayout(false);
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.panel8.ResumeLayout(false);
            this.panel9.ResumeLayout(false);
            this.panelTopGrid.ResumeLayout(false);
            this.panel10.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel leftPane;
        private System.Windows.Forms.Panel mainPane;
        private System.Windows.Forms.Panel mainContentPane;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabGrid;
        private System.Windows.Forms.TabPage tabDetail;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox txtValue;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblCreatedBy;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblCreatedDateTime;
        private System.Windows.Forms.Label lblModifiedBy;
        private System.Windows.Forms.Label lblModifiedDateTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private nsteam.Windows.Commons.Tree.ReflectionTree tree;
        private DataGridViewExtension grid;
        private System.Windows.Forms.Button btnAddNode;
        private System.Windows.Forms.Button btnAddCollection;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Button btnRemoveNode;
        private System.Windows.Forms.Panel mainBackPanel;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabBrowse;
        private System.Windows.Forms.TabPage tabSource;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openUrlToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.MenuStrip miniToolStrip;
        private System.Windows.Forms.Splitter splitter1;
        private ICSharpCode.TextEditor.TextEditorControl txtSource;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.RadioButton rdAsJson;
        private System.Windows.Forms.RadioButton rdAsXML;
        private System.Windows.Forms.Button btnSaveSource;
        private System.Windows.Forms.TabPage tabQuery;
        private ICSharpCode.TextEditor.TextEditorControl textEdQuery;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Label lbQueryTitle;
        private System.Windows.Forms.TextBox txtCommand;
        private System.Windows.Forms.Button btnGetNode;
        private System.Windows.Forms.RadioButton rdQueryAsJson;
        private System.Windows.Forms.RadioButton rdQueryAsXml;
        private System.Windows.Forms.DataGridViewTextBoxColumn pColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DynamicControl;
        private System.Windows.Forms.DataGridViewTextBoxColumn obj;
        private System.Windows.Forms.DataGridViewTextBoxColumn col;
        private System.Windows.Forms.DataGridViewTextBoxColumn OldProperty;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel statusSpacer;
        private System.Windows.Forms.ToolStripStatusLabel statusServer;
        private System.Windows.Forms.ToolStripStatusLabel statusPristine;
        private System.Windows.Forms.ToolStripStatusLabel statusDirty;
        private System.Windows.Forms.ToolStripStatusLabel statusError;
        private System.Windows.Forms.CheckBox chObjectInfo;
        private System.Windows.Forms.Button btnGetTree;
        private ICSharpCode.TextEditor.TextEditorControl txtSourceNode;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Panel panel9;
        private System.Windows.Forms.Button btnSaveSourceNode;
        private System.Windows.Forms.Splitter splitter3;
        private System.Windows.Forms.Panel panel10;
        private System.Windows.Forms.Splitter splitterGrid;
        private System.Windows.Forms.Panel panelTopGrid;
        private ICSharpCode.TextEditor.TextEditorControl txtPreview;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gridToolStripMenuItem;
    }
}