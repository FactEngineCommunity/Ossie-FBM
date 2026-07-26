<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
    Inherits Global.System.Windows.Forms.Form

    Private components As Global.System.ComponentModel.IContainer

    <Global.System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    <Global.System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        menuStrip = New System.Windows.Forms.MenuStrip()
        fileMenu = New System.Windows.Forms.ToolStripMenuItem()
        importMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        importOssieYamlMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        importFbmModelMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        saveAsMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        exportOssieYamlMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        exportMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        exportFbmMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        fileMenuSeparator = New System.Windows.Forms.ToolStripSeparator()
        exitMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        contentPanel = New System.Windows.Forms.Panel()
        treePanel = New System.Windows.Forms.Panel()
        treeLayoutPanel = New System.Windows.Forms.TableLayoutPanel()
        searchTextBox = New System.Windows.Forms.TextBox()
        modelTabControl = New System.Windows.Forms.TabControl()
        ossieTabPage = New System.Windows.Forms.TabPage()
        documentTree = New System.Windows.Forms.TreeView()
        yamlTextTabPage = New System.Windows.Forms.TabPage()
        yamlTextEditor = New ScintillaNET.Scintilla()
        fbmTabPage = New System.Windows.Forms.TabPage()
        fbmTree = New System.Windows.Forms.TreeView()
        summaryPanel = New System.Windows.Forms.TableLayoutPanel()
        headingLabel = New System.Windows.Forms.Label()
        fileCaptionLabel = New System.Windows.Forms.Label()
        filePathValue = New System.Windows.Forms.Label()
        nameCaptionLabel = New System.Windows.Forms.Label()
        nameValue = New System.Windows.Forms.Label()
        versionCaptionLabel = New System.Windows.Forms.Label()
        versionValue = New System.Windows.Forms.Label()
        descriptionCaptionLabel = New System.Windows.Forms.Label()
        descriptionValue = New System.Windows.Forms.Label()
        componentCountCaptionLabel = New System.Windows.Forms.Label()
        componentCountValue = New System.Windows.Forms.Label()
        mappingCountCaptionLabel = New System.Windows.Forms.Label()
        mappingCountValue = New System.Windows.Forms.Label()
        statusStrip = New System.Windows.Forms.StatusStrip()
        statusLabel = New System.Windows.Forms.ToolStripStatusLabel()
        menuStrip.SuspendLayout()
        contentPanel.SuspendLayout()
        treePanel.SuspendLayout()
        treeLayoutPanel.SuspendLayout()
        modelTabControl.SuspendLayout()
        ossieTabPage.SuspendLayout()
        yamlTextTabPage.SuspendLayout()
        fbmTabPage.SuspendLayout()
        summaryPanel.SuspendLayout()
        statusStrip.SuspendLayout()
        SuspendLayout()
        ' 
        ' menuStrip
        ' 
        menuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {fileMenu})
        menuStrip.Location = New System.Drawing.Point(0, 0)
        menuStrip.Name = "menuStrip"
        menuStrip.Size = New System.Drawing.Size(940, 24)
        menuStrip.TabIndex = 0
        ' 
        ' fileMenu
        ' 
        fileMenu.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {importMenuItem, saveAsMenuItem, exportMenuItem, fileMenuSeparator, exitMenuItem})
        fileMenu.Name = "fileMenu"
        fileMenu.Size = New System.Drawing.Size(37, 20)
        fileMenu.Text = "&File"
        ' 
        ' importMenuItem
        ' 
        importMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {importOssieYamlMenuItem, importFbmModelMenuItem})
        importMenuItem.Name = "importMenuItem"
        importMenuItem.Size = New System.Drawing.Size(246, 22)
        importMenuItem.Text = "&Import"
        ' 
        ' importOssieYamlMenuItem
        ' 
        importOssieYamlMenuItem.Name = "importOssieYamlMenuItem"
        importOssieYamlMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.O
        importOssieYamlMenuItem.Size = New System.Drawing.Size(238, 22)
        importOssieYamlMenuItem.Text = "&Ossie YAML file..."
        ' 
        ' importFbmModelMenuItem
        ' 
        importFbmModelMenuItem.Name = "importFbmModelMenuItem"
        importFbmModelMenuItem.Size = New System.Drawing.Size(238, 22)
        importFbmModelMenuItem.Text = "&Fact-Based Model (.fbm)..."
        ' 
        ' saveAsMenuItem
        ' 
        saveAsMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {exportOssieYamlMenuItem})
        saveAsMenuItem.Name = "saveAsMenuItem"
        saveAsMenuItem.Size = New System.Drawing.Size(246, 22)
        saveAsMenuItem.Text = "&Save As..."
        ' 
        ' exportOssieYamlMenuItem
        ' 
        exportOssieYamlMenuItem.Enabled = False
        exportOssieYamlMenuItem.Name = "exportOssieYamlMenuItem"
        exportOssieYamlMenuItem.Size = New System.Drawing.Size(180, 22)
        exportOssieYamlMenuItem.Text = "&Ossie .yaml..."
        ' 
        ' exportMenuItem
        ' 
        exportMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {exportFbmMenuItem})
        exportMenuItem.Name = "exportMenuItem"
        exportMenuItem.Size = New System.Drawing.Size(246, 22)
        exportMenuItem.Text = "&Export"
        ' 
        ' exportFbmMenuItem
        ' 
        exportFbmMenuItem.Enabled = False
        exportFbmMenuItem.Name = "exportFbmMenuItem"
        exportFbmMenuItem.Size = New System.Drawing.Size(239, 22)
        exportFbmMenuItem.Text = "...as .fbm Fact-Based Model..."
        ' 
        ' fileMenuSeparator
        ' 
        fileMenuSeparator.Name = "fileMenuSeparator"
        fileMenuSeparator.Size = New System.Drawing.Size(243, 6)
        ' 
        ' exitMenuItem
        ' 
        exitMenuItem.Name = "exitMenuItem"
        exitMenuItem.Size = New System.Drawing.Size(246, 22)
        exitMenuItem.Text = "E&xit"
        ' 
        ' contentPanel
        ' 
        contentPanel.Controls.Add(treePanel)
        contentPanel.Controls.Add(summaryPanel)
        contentPanel.Dock = System.Windows.Forms.DockStyle.Fill
        contentPanel.Location = New System.Drawing.Point(0, 24)
        contentPanel.Name = "contentPanel"
        contentPanel.Size = New System.Drawing.Size(940, 634)
        contentPanel.TabIndex = 1
        ' 
        ' treePanel
        ' 
        treePanel.Controls.Add(treeLayoutPanel)
        treePanel.Dock = System.Windows.Forms.DockStyle.Fill
        treePanel.Location = New System.Drawing.Point(0, 164)
        treePanel.Name = "treePanel"
        treePanel.Size = New System.Drawing.Size(940, 470)
        treePanel.TabIndex = 1
        ' 
        ' treeLayoutPanel
        ' 
        treeLayoutPanel.ColumnCount = 1
        treeLayoutPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        treeLayoutPanel.Controls.Add(searchTextBox, 0, 0)
        treeLayoutPanel.Controls.Add(modelTabControl, 0, 1)
        treeLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill
        treeLayoutPanel.Location = New System.Drawing.Point(0, 0)
        treeLayoutPanel.Name = "treeLayoutPanel"
        treeLayoutPanel.Padding = New System.Windows.Forms.Padding(12, 0, 12, 12)
        treeLayoutPanel.RowCount = 2
        treeLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle())
        treeLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        treeLayoutPanel.Size = New System.Drawing.Size(940, 470)
        treeLayoutPanel.TabIndex = 0
        ' 
        ' searchTextBox
        ' 
        searchTextBox.Dock = System.Windows.Forms.DockStyle.Top
        searchTextBox.Location = New System.Drawing.Point(15, 3)
        searchTextBox.Margin = New System.Windows.Forms.Padding(3, 3, 3, 7)
        searchTextBox.Name = "searchTextBox"
        searchTextBox.PlaceholderText = "Search the selected tree and press Enter for the next match..."
        searchTextBox.Size = New System.Drawing.Size(910, 23)
        searchTextBox.TabIndex = 0
        ' 
        ' modelTabControl
        ' 
        modelTabControl.Controls.Add(ossieTabPage)
        modelTabControl.Controls.Add(yamlTextTabPage)
        modelTabControl.Controls.Add(fbmTabPage)
        modelTabControl.Dock = System.Windows.Forms.DockStyle.Fill
        modelTabControl.Location = New System.Drawing.Point(15, 36)
        modelTabControl.Name = "modelTabControl"
        modelTabControl.SelectedIndex = 0
        modelTabControl.Size = New System.Drawing.Size(910, 419)
        modelTabControl.TabIndex = 1
        ' 
        ' ossieTabPage
        ' 
        ossieTabPage.Controls.Add(documentTree)
        ossieTabPage.Location = New System.Drawing.Point(4, 24)
        ossieTabPage.Name = "ossieTabPage"
        ossieTabPage.Padding = New System.Windows.Forms.Padding(6)
        ossieTabPage.Size = New System.Drawing.Size(902, 391)
        ossieTabPage.TabIndex = 0
        ossieTabPage.Text = "Ossie"
        ossieTabPage.UseVisualStyleBackColor = True
        ' 
        ' documentTree
        ' 
        documentTree.Dock = System.Windows.Forms.DockStyle.Fill
        documentTree.HideSelection = False
        documentTree.Location = New System.Drawing.Point(6, 6)
        documentTree.Name = "documentTree"
        documentTree.ShowNodeToolTips = True
        documentTree.Size = New System.Drawing.Size(890, 379)
        documentTree.TabIndex = 0
        ' 
        ' yamlTextTabPage
        ' 
        yamlTextTabPage.Controls.Add(yamlTextEditor)
        yamlTextTabPage.Location = New System.Drawing.Point(4, 24)
        yamlTextTabPage.Name = "yamlTextTabPage"
        yamlTextTabPage.Padding = New System.Windows.Forms.Padding(6)
        yamlTextTabPage.Size = New System.Drawing.Size(902, 391)
        yamlTextTabPage.TabIndex = 1
        yamlTextTabPage.Text = ".YAML Text"
        yamlTextTabPage.UseVisualStyleBackColor = True
        ' 
        ' yamlTextEditor
        ' 
        yamlTextEditor.Dock = System.Windows.Forms.DockStyle.Fill
        yamlTextEditor.Location = New System.Drawing.Point(6, 6)
        yamlTextEditor.Name = "yamlTextEditor"
        yamlTextEditor.Size = New System.Drawing.Size(890, 379)
        yamlTextEditor.TabIndex = 0
        ' 
        ' fbmTabPage
        ' 
        fbmTabPage.Controls.Add(fbmTree)
        fbmTabPage.Location = New System.Drawing.Point(4, 24)
        fbmTabPage.Name = "fbmTabPage"
        fbmTabPage.Padding = New System.Windows.Forms.Padding(6)
        fbmTabPage.Size = New System.Drawing.Size(902, 391)
        fbmTabPage.TabIndex = 2
        fbmTabPage.Text = "Fact-Based Model (ORM)"
        fbmTabPage.UseVisualStyleBackColor = True
        ' 
        ' fbmTree
        ' 
        fbmTree.Dock = System.Windows.Forms.DockStyle.Fill
        fbmTree.HideSelection = False
        fbmTree.Location = New System.Drawing.Point(6, 6)
        fbmTree.Name = "fbmTree"
        fbmTree.ShowNodeToolTips = True
        fbmTree.Size = New System.Drawing.Size(890, 379)
        fbmTree.TabIndex = 0
        ' 
        ' summaryPanel
        ' 
        summaryPanel.AutoSize = True
        summaryPanel.ColumnCount = 2
        summaryPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 145F))
        summaryPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        summaryPanel.Controls.Add(headingLabel, 0, 0)
        summaryPanel.Controls.Add(fileCaptionLabel, 0, 1)
        summaryPanel.Controls.Add(filePathValue, 1, 1)
        summaryPanel.Controls.Add(nameCaptionLabel, 0, 2)
        summaryPanel.Controls.Add(nameValue, 1, 2)
        summaryPanel.Controls.Add(versionCaptionLabel, 0, 3)
        summaryPanel.Controls.Add(versionValue, 1, 3)
        summaryPanel.Controls.Add(descriptionCaptionLabel, 0, 4)
        summaryPanel.Controls.Add(descriptionValue, 1, 4)
        summaryPanel.Controls.Add(componentCountCaptionLabel, 0, 5)
        summaryPanel.Controls.Add(componentCountValue, 1, 5)
        summaryPanel.Controls.Add(mappingCountCaptionLabel, 0, 6)
        summaryPanel.Controls.Add(mappingCountValue, 1, 6)
        summaryPanel.Dock = System.Windows.Forms.DockStyle.Top
        summaryPanel.Location = New System.Drawing.Point(0, 0)
        summaryPanel.Name = "summaryPanel"
        summaryPanel.Padding = New System.Windows.Forms.Padding(12)
        summaryPanel.RowCount = 7
        summaryPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F))
        summaryPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F))
        summaryPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F))
        summaryPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F))
        summaryPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F))
        summaryPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F))
        summaryPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F))
        summaryPanel.Size = New System.Drawing.Size(940, 164)
        summaryPanel.TabIndex = 0
        ' 
        ' headingLabel
        ' 
        headingLabel.AutoSize = True
        summaryPanel.SetColumnSpan(headingLabel, 2)
        headingLabel.Font = New System.Drawing.Font("Segoe UI", 9F, Drawing.FontStyle.Bold)
        headingLabel.Location = New System.Drawing.Point(15, 18)
        headingLabel.Margin = New System.Windows.Forms.Padding(3, 6, 3, 10)
        headingLabel.Name = "headingLabel"
        headingLabel.Size = New System.Drawing.Size(152, 4)
        headingLabel.TabIndex = 0
        headingLabel.Text = "Imported Ossie document"
        ' 
        ' fileCaptionLabel
        ' 
        fileCaptionLabel.AutoSize = True
        fileCaptionLabel.Location = New System.Drawing.Point(15, 35)
        fileCaptionLabel.Margin = New System.Windows.Forms.Padding(3, 3, 3, 5)
        fileCaptionLabel.Name = "fileCaptionLabel"
        fileCaptionLabel.Size = New System.Drawing.Size(28, 12)
        fileCaptionLabel.TabIndex = 1
        fileCaptionLabel.Text = "File:"
        ' 
        ' filePathValue
        ' 
        filePathValue.AutoEllipsis = True
        filePathValue.AutoSize = True
        filePathValue.Location = New System.Drawing.Point(160, 35)
        filePathValue.Margin = New System.Windows.Forms.Padding(3, 3, 3, 5)
        filePathValue.Name = "filePathValue"
        filePathValue.Size = New System.Drawing.Size(94, 12)
        filePathValue.TabIndex = 2
        filePathValue.Text = "No file imported"
        ' 
        ' nameCaptionLabel
        ' 
        nameCaptionLabel.AutoSize = True
        nameCaptionLabel.Location = New System.Drawing.Point(15, 55)
        nameCaptionLabel.Margin = New System.Windows.Forms.Padding(3, 3, 3, 5)
        nameCaptionLabel.Name = "nameCaptionLabel"
        nameCaptionLabel.Size = New System.Drawing.Size(42, 12)
        nameCaptionLabel.TabIndex = 3
        nameCaptionLabel.Text = "Name:"
        ' 
        ' nameValue
        ' 
        nameValue.AutoEllipsis = True
        nameValue.AutoSize = True
        nameValue.Location = New System.Drawing.Point(160, 55)
        nameValue.Margin = New System.Windows.Forms.Padding(3, 3, 3, 5)
        nameValue.Name = "nameValue"
        nameValue.Size = New System.Drawing.Size(12, 12)
        nameValue.TabIndex = 4
        nameValue.Text = "-"
        ' 
        ' versionCaptionLabel
        ' 
        versionCaptionLabel.AutoSize = True
        versionCaptionLabel.Location = New System.Drawing.Point(15, 75)
        versionCaptionLabel.Margin = New System.Windows.Forms.Padding(3, 3, 3, 5)
        versionCaptionLabel.Name = "versionCaptionLabel"
        versionCaptionLabel.Size = New System.Drawing.Size(48, 12)
        versionCaptionLabel.TabIndex = 5
        versionCaptionLabel.Text = "Version:"
        ' 
        ' versionValue
        ' 
        versionValue.AutoEllipsis = True
        versionValue.AutoSize = True
        versionValue.Location = New System.Drawing.Point(160, 75)
        versionValue.Margin = New System.Windows.Forms.Padding(3, 3, 3, 5)
        versionValue.Name = "versionValue"
        versionValue.Size = New System.Drawing.Size(12, 12)
        versionValue.TabIndex = 6
        versionValue.Text = "-"
        ' 
        ' descriptionCaptionLabel
        ' 
        descriptionCaptionLabel.AutoSize = True
        descriptionCaptionLabel.Location = New System.Drawing.Point(15, 95)
        descriptionCaptionLabel.Margin = New System.Windows.Forms.Padding(3, 3, 3, 5)
        descriptionCaptionLabel.Name = "descriptionCaptionLabel"
        descriptionCaptionLabel.Size = New System.Drawing.Size(70, 12)
        descriptionCaptionLabel.TabIndex = 7
        descriptionCaptionLabel.Text = "Description:"
        ' 
        ' descriptionValue
        ' 
        descriptionValue.AutoEllipsis = True
        descriptionValue.AutoSize = True
        descriptionValue.Location = New System.Drawing.Point(160, 95)
        descriptionValue.Margin = New System.Windows.Forms.Padding(3, 3, 3, 5)
        descriptionValue.Name = "descriptionValue"
        descriptionValue.Size = New System.Drawing.Size(231, 12)
        descriptionValue.TabIndex = 8
        descriptionValue.Text = "Use File > Import Ossie YAML file to begin."
        ' 
        ' componentCountCaptionLabel
        ' 
        componentCountCaptionLabel.AutoSize = True
        componentCountCaptionLabel.Location = New System.Drawing.Point(15, 115)
        componentCountCaptionLabel.Margin = New System.Windows.Forms.Padding(3, 3, 3, 5)
        componentCountCaptionLabel.Name = "componentCountCaptionLabel"
        componentCountCaptionLabel.Size = New System.Drawing.Size(130, 12)
        componentCountCaptionLabel.TabIndex = 9
        componentCountCaptionLabel.Text = "Ontology components:"
        ' 
        ' componentCountValue
        ' 
        componentCountValue.AutoEllipsis = True
        componentCountValue.AutoSize = True
        componentCountValue.Location = New System.Drawing.Point(160, 115)
        componentCountValue.Margin = New System.Windows.Forms.Padding(3, 3, 3, 5)
        componentCountValue.Name = "componentCountValue"
        componentCountValue.Size = New System.Drawing.Size(13, 12)
        componentCountValue.TabIndex = 10
        componentCountValue.Text = "0"
        ' 
        ' mappingCountCaptionLabel
        ' 
        mappingCountCaptionLabel.AutoSize = True
        mappingCountCaptionLabel.Location = New System.Drawing.Point(15, 135)
        mappingCountCaptionLabel.Margin = New System.Windows.Forms.Padding(3, 3, 3, 5)
        mappingCountCaptionLabel.Name = "mappingCountCaptionLabel"
        mappingCountCaptionLabel.Size = New System.Drawing.Size(116, 12)
        mappingCountCaptionLabel.TabIndex = 11
        mappingCountCaptionLabel.Text = "Ontology mappings:"
        ' 
        ' mappingCountValue
        ' 
        mappingCountValue.AutoEllipsis = True
        mappingCountValue.AutoSize = True
        mappingCountValue.Location = New System.Drawing.Point(160, 135)
        mappingCountValue.Margin = New System.Windows.Forms.Padding(3, 3, 3, 5)
        mappingCountValue.Name = "mappingCountValue"
        mappingCountValue.Size = New System.Drawing.Size(13, 12)
        mappingCountValue.TabIndex = 12
        mappingCountValue.Text = "0"
        ' 
        ' statusStrip
        ' 
        statusStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {statusLabel})
        statusStrip.Location = New System.Drawing.Point(0, 658)
        statusStrip.Name = "statusStrip"
        statusStrip.Size = New System.Drawing.Size(940, 22)
        statusStrip.TabIndex = 2
        ' 
        ' statusLabel
        ' 
        statusLabel.Name = "statusLabel"
        statusLabel.Size = New System.Drawing.Size(39, 17)
        statusLabel.Text = "Ready"
        ' 
        ' MainForm
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        ClientSize = New System.Drawing.Size(940, 680)
        Controls.Add(contentPanel)
        Controls.Add(statusStrip)
        Controls.Add(menuStrip)
        MainMenuStrip = menuStrip
        MinimumSize = New System.Drawing.Size(760, 520)
        Name = "MainForm"
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Text = "FBM Ossie YAML Viewer"
        menuStrip.ResumeLayout(False)
        menuStrip.PerformLayout()
        contentPanel.ResumeLayout(False)
        contentPanel.PerformLayout()
        treePanel.ResumeLayout(False)
        treeLayoutPanel.ResumeLayout(False)
        treeLayoutPanel.PerformLayout()
        modelTabControl.ResumeLayout(False)
        ossieTabPage.ResumeLayout(False)
        yamlTextTabPage.ResumeLayout(False)
        fbmTabPage.ResumeLayout(False)
        summaryPanel.ResumeLayout(False)
        summaryPanel.PerformLayout()
        statusStrip.ResumeLayout(False)
        statusStrip.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents menuStrip As Global.System.Windows.Forms.MenuStrip
    Friend WithEvents fileMenu As Global.System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents importMenuItem As Global.System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents importOssieYamlMenuItem As Global.System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents importFbmModelMenuItem As Global.System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents saveAsMenuItem As Global.System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents exportMenuItem As Global.System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents exportOssieYamlMenuItem As Global.System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents exportFbmMenuItem As Global.System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents fileMenuSeparator As Global.System.Windows.Forms.ToolStripSeparator
    Friend WithEvents exitMenuItem As Global.System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents contentPanel As Global.System.Windows.Forms.Panel
    Friend WithEvents treePanel As Global.System.Windows.Forms.Panel
    Friend WithEvents treeLayoutPanel As Global.System.Windows.Forms.TableLayoutPanel
    Friend WithEvents searchTextBox As Global.System.Windows.Forms.TextBox
    Friend WithEvents modelTabControl As Global.System.Windows.Forms.TabControl
    Friend WithEvents ossieTabPage As Global.System.Windows.Forms.TabPage
    Friend WithEvents documentTree As Global.System.Windows.Forms.TreeView
    Friend WithEvents yamlTextTabPage As Global.System.Windows.Forms.TabPage
    Friend WithEvents yamlTextEditor As Global.ScintillaNET.Scintilla
    Friend WithEvents fbmTabPage As Global.System.Windows.Forms.TabPage
    Friend WithEvents fbmTree As Global.System.Windows.Forms.TreeView
    Friend WithEvents summaryPanel As Global.System.Windows.Forms.TableLayoutPanel
    Friend WithEvents headingLabel As Global.System.Windows.Forms.Label
    Friend WithEvents fileCaptionLabel As Global.System.Windows.Forms.Label
    Friend WithEvents filePathValue As Global.System.Windows.Forms.Label
    Friend WithEvents nameCaptionLabel As Global.System.Windows.Forms.Label
    Friend WithEvents nameValue As Global.System.Windows.Forms.Label
    Friend WithEvents versionCaptionLabel As Global.System.Windows.Forms.Label
    Friend WithEvents versionValue As Global.System.Windows.Forms.Label
    Friend WithEvents descriptionCaptionLabel As Global.System.Windows.Forms.Label
    Friend WithEvents descriptionValue As Global.System.Windows.Forms.Label
    Friend WithEvents componentCountCaptionLabel As Global.System.Windows.Forms.Label
    Friend WithEvents componentCountValue As Global.System.Windows.Forms.Label
    Friend WithEvents mappingCountCaptionLabel As Global.System.Windows.Forms.Label
    Friend WithEvents mappingCountValue As Global.System.Windows.Forms.Label
    Friend WithEvents statusStrip As Global.System.Windows.Forms.StatusStrip
    Friend WithEvents statusLabel As Global.System.Windows.Forms.ToolStripStatusLabel
End Class
