Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms
Imports System.Xml.Serialization
Imports FBM_Ossie
Imports FBM_Ossie.Ossie
Imports ScintillaNET
Imports FBM = FactEngineForServices.FBM
Imports XMLModel = FactEngineForServices.XMLModel

Public Class MainForm

    Private currentDocument As OssieDocument
    Private currentFbmModel As FBM.Model
    Private currentSourceFilePath As String
    Private ReadOnly searchMatches As New List(Of TreeNode)()
    Private searchMatchIndex As Integer = -1
    Private lastSearchText As String = String.Empty

    Public Sub New()
        InitializeComponent()
        SetupYamlEditor()
        AddHandler importOssieYamlMenuItem.Click,
            AddressOf ImportOssieYamlMenuItem_Click
        AddHandler importFbmModelMenuItem.Click,
            AddressOf ImportFbmModelMenuItem_Click
        AddHandler exportOssieYamlMenuItem.Click,
            AddressOf ExportOssieYamlMenuItem_Click
        AddHandler exportFbmMenuItem.Click, AddressOf ExportFbmMenuItem_Click
        AddHandler exitMenuItem.Click, AddressOf ExitMenuItem_Click
        AddHandler searchTextBox.KeyDown, AddressOf SearchTextBox_KeyDown
        AddHandler searchTextBox.TextChanged, AddressOf SearchTextBox_TextChanged
        AddHandler modelTabControl.SelectedIndexChanged, AddressOf ModelTabControl_SelectedIndexChanged
        ShowEmptyState()
    End Sub

    Private Sub ExitMenuItem_Click(sender As Object, e As EventArgs)
        Close()
    End Sub

    Private Sub SearchTextBox_TextChanged(sender As Object, e As EventArgs)
        ResetSearch()
    End Sub

    Private Sub ModelTabControl_SelectedIndexChanged(sender As Object, e As EventArgs)
        ResetSearch()
        Dim treeSelected =
            modelTabControl.SelectedTab Is ossieTabPage OrElse
            modelTabControl.SelectedTab Is fbmTabPage
        searchTextBox.Enabled = treeSelected
        searchTextBox.PlaceholderText = If(
            treeSelected,
            "Search the selected tree and press Enter for the next match...",
            "Tree search is unavailable in the YAML text view")
    End Sub

    Private Sub SearchTextBox_KeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode <> Keys.Enter Then
            Return
        End If

        e.Handled = True
        e.SuppressKeyPress = True
        FindNextTreeNode()
    End Sub

    Private Sub FindNextTreeNode()
        Dim searchText = searchTextBox.Text.Trim()
        If searchText.Length = 0 Then
            statusLabel.Text = "Enter text to search the tree"
            Return
        End If

        If searchMatches.Count = 0 OrElse
            Not String.Equals(searchText, lastSearchText, StringComparison.OrdinalIgnoreCase) Then
            searchMatches.Clear()
            CollectMatchingNodes(ActiveTree.Nodes, searchText, searchMatches)
            searchMatchIndex = -1
            lastSearchText = searchText
        End If

        If searchMatches.Count = 0 Then
            statusLabel.Text = $"No tree nodes contain ""{searchText}"""
            Return
        End If

        searchMatchIndex = (searchMatchIndex + 1) Mod searchMatches.Count
        Dim matchingNode = searchMatches(searchMatchIndex)
        ExpandParentNodes(matchingNode)
        ActiveTree.SelectedNode = matchingNode
        matchingNode.EnsureVisible()
        statusLabel.Text =
            $"Match {searchMatchIndex + 1} of {searchMatches.Count}: {matchingNode.Text}"
    End Sub

    Private Shared Sub CollectMatchingNodes(
        nodes As TreeNodeCollection,
        searchText As String,
        matches As List(Of TreeNode))

        For Each node As TreeNode In nodes
            If node.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 Then
                matches.Add(node)
            End If
            CollectMatchingNodes(node.Nodes, searchText, matches)
        Next
    End Sub

    Private Shared Sub ExpandParentNodes(node As TreeNode)
        Dim parentNode = node.Parent
        While parentNode IsNot Nothing
            parentNode.Expand()
            parentNode = parentNode.Parent
        End While
    End Sub

    Private Sub ResetSearch()
        searchMatches.Clear()
        searchMatchIndex = -1
        lastSearchText = String.Empty
    End Sub

    Private ReadOnly Property ActiveTree As TreeView
        Get
            If modelTabControl.SelectedTab Is fbmTabPage Then
                Return fbmTree
            End If

            Return documentTree
        End Get
    End Property

    Private Sub ImportOssieYamlMenuItem_Click(
        sender As Object,
        e As EventArgs)

        Using dialog As New OpenFileDialog()
            dialog.Title = "Import Ossie YAML file"
            dialog.Filter = "YAML files (*.yaml;*.yml)|*.yaml;*.yml|All files (*.*)|*.*"
            dialog.CheckFileExists = True
            dialog.Multiselect = False

            If dialog.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If

            ImportDocument(dialog.FileName)
        End Using
    End Sub

    Private Sub ImportFbmModelMenuItem_Click(
        sender As Object,
        e As EventArgs)

        Using lrOpenFileDialog As New OpenFileDialog()
            lrOpenFileDialog.Title = "Import Fact-Based Model"
            lrOpenFileDialog.Filter =
                "Fact-Based Model files (*.fbm)|*.fbm|All files (*.*)|*.*"
            lrOpenFileDialog.CheckFileExists = True
            lrOpenFileDialog.Multiselect = False

            If lrOpenFileDialog.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If

            ImportFbmDocument(lrOpenFileDialog.FileName)
        End Using
    End Sub

    Private Sub ImportDocument(filePath As String)
        Try
            Cursor = Cursors.WaitCursor
            statusLabel.Text = "Importing..."

            Dim yamlText = File.ReadAllText(filePath)
            Dim importedDocument = FBMOssie.ImportDocument(filePath)
            If importedDocument Is Nothing Then
                Throw New InvalidDataException("The YAML file did not contain an Ossie document.")
            End If

            currentDocument = importedDocument
            currentFbmModel = OssieToFbmMapper.Map(importedDocument)
            If TypeOf importedDocument Is OntologyDocument Then
                DisplayOntologyDocument(
                    filePath,
                    DirectCast(importedDocument, OntologyDocument))
            ElseIf TypeOf importedDocument Is SemanticModelDocument Then
                DisplaySemanticModelDocument(
                    filePath,
                    DirectCast(importedDocument, SemanticModelDocument))
            Else
                Throw New InvalidDataException(
                    $"Unsupported Ossie document type: {importedDocument.GetType().Name}")
            End If
            DisplayFbmModel(currentFbmModel)
            DisplayYamlText(yamlText)
            currentSourceFilePath = filePath
            exportOssieYamlMenuItem.Enabled = True
            exportFbmMenuItem.Enabled = True
            modelTabControl.SelectedTab = ossieTabPage
            statusLabel.Text = $"Imported {Path.GetFileName(filePath)}"
        Catch ex As Exception
            statusLabel.Text = "Import failed"
            MessageBox.Show(
                Me,
                $"The Ossie YAML file could not be imported.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "Import Ossie YAML file",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        Finally
            Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub ImportFbmDocument(ByVal asFilePath As String)
        Try
            Cursor = Cursors.WaitCursor
            statusLabel.Text = "Loading Fact-Based Model..."

            Dim lrLoadedFBMModel As FBM.Model =
                LoadFbmModel(asFilePath)
            statusLabel.Text = "Mapping Fact-Based Model to Ossie..."

            Dim lrMappedDocument As OntologyDocument =
                FBMToOssieMapper.Map(lrLoadedFBMModel)
            Dim lsYamlText As String =
                OssieYaml.CreateSerializer().Serialize(lrMappedDocument)

            currentDocument = lrMappedDocument
            currentFbmModel = lrLoadedFBMModel
            currentSourceFilePath = asFilePath

            DisplayOntologyDocument(
                asFilePath,
                lrMappedDocument)
            DisplayFbmModel(lrLoadedFBMModel)
            DisplayYamlText(lsYamlText)

            exportOssieYamlMenuItem.Enabled = True
            exportFbmMenuItem.Enabled = True
            modelTabControl.SelectedTab = ossieTabPage
            statusLabel.Text =
                $"Imported and mapped {Path.GetFileName(asFilePath)}"
        Catch lrException As Exception
            statusLabel.Text = "Fact-Based Model import failed"
            MessageBox.Show(
                Me,
                $"The Fact-Based Model could not be imported and mapped.{Environment.NewLine}{Environment.NewLine}{lrException.Message}",
                "Import Fact-Based Model",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        Finally
            Cursor = Cursors.Default
        End Try
    End Sub

    Private Shared Function LoadFbmModel(
        ByVal asFilePath As String) As FBM.Model

        Return FbmXmlModelLoader.Load(asFilePath)
    End Function

    Private Sub ExportOssieYamlMenuItem_Click(
        sender As Object,
        e As EventArgs)

        If currentDocument Is Nothing Then
            MessageBox.Show(
                Me,
                "Import an Ossie YAML file or Fact-Based Model before exporting Ossie YAML.",
                "Export Ossie YAML",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)
            Return
        End If

        Using lrSaveFileDialog As New SaveFileDialog()
            lrSaveFileDialog.Title = "Export as Ossie YAML"
            lrSaveFileDialog.Filter =
                "YAML files (*.yaml)|*.yaml|YAML files (*.yml)|*.yml"
            lrSaveFileDialog.DefaultExt = "yaml"
            lrSaveFileDialog.AddExtension = True
            lrSaveFileDialog.OverwritePrompt = True
            lrSaveFileDialog.FileName = GetDefaultYamlFileName()

            If Not String.IsNullOrWhiteSpace(currentSourceFilePath) Then
                Dim lsSourceFolder As String =
                    Path.GetDirectoryName(currentSourceFilePath)
                If Directory.Exists(lsSourceFolder) Then
                    lrSaveFileDialog.InitialDirectory = lsSourceFolder
                End If
            End If

            If lrSaveFileDialog.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If

            ExportOssieYaml(lrSaveFileDialog.FileName)
        End Using
    End Sub

    Private Sub ExportOssieYaml(ByVal asFilePath As String)
        Try
            Cursor = Cursors.WaitCursor
            statusLabel.Text = "Exporting Ossie YAML..."

            Dim lsYamlText As String =
                OssieYaml.CreateSerializer().Serialize(currentDocument)
            File.WriteAllText(
                asFilePath,
                lsYamlText,
                New UTF8Encoding(False))

            statusLabel.Text =
                $"Exported {Path.GetFileName(asFilePath)}"
        Catch lrException As Exception
            statusLabel.Text = "Ossie YAML export failed"
            MessageBox.Show(
                Me,
                $"The Ossie YAML file could not be exported.{Environment.NewLine}{Environment.NewLine}{lrException.Message}",
                "Export Ossie YAML",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        Finally
            Cursor = Cursors.Default
        End Try
    End Sub

    Private Function GetDefaultYamlFileName() As String
        If Not String.IsNullOrWhiteSpace(currentSourceFilePath) Then
            Return Path.GetFileNameWithoutExtension(
                currentSourceFilePath) & ".yaml"
        End If

        Dim lrOntologyDocument As OntologyDocument =
            TryCast(currentDocument, OntologyDocument)
        Dim lsFileName As String =
            If(
                lrOntologyDocument Is Nothing,
                "Ossie-model",
                ValueOrDash(lrOntologyDocument.Name))
        For Each lcInvalidCharacter As Char In Path.GetInvalidFileNameChars()
            lsFileName = lsFileName.Replace(
                lcInvalidCharacter,
                "_"c)
        Next

        Return lsFileName & ".yaml"
    End Function

    Private Sub ExportFbmMenuItem_Click(sender As Object, e As EventArgs)
        If currentFbmModel Is Nothing Then
            MessageBox.Show(
                Me,
                "Import an Ossie YAML file before exporting a Fact-Based Model.",
                "Export Fact-Based Model",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)
            Return
        End If

        Using dialog As New SaveFileDialog()
            dialog.Title = "Export as .fbm Fact-Based Model"
            dialog.Filter = "Fact-Based Model file (*.fbm)|*.fbm"
            dialog.DefaultExt = "fbm"
            dialog.AddExtension = True
            dialog.OverwritePrompt = True
            dialog.FileName = GetDefaultFbmFileName()

            If Not String.IsNullOrWhiteSpace(currentSourceFilePath) Then
                Dim sourceFolder = Path.GetDirectoryName(currentSourceFilePath)
                If Directory.Exists(sourceFolder) Then
                    dialog.InitialDirectory = sourceFolder
                End If
            End If

            If dialog.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If

            ExportFbmModel(dialog.FileName)
        End Using
    End Sub

    Private Sub ExportFbmModel(filePath As String)
        Try
            Cursor = Cursors.WaitCursor
            statusLabel.Text = "Exporting Fact-Based Model..."

            Dim exportModel As New XMLModel.Model()
            exportModel.ORMModel.ModelId = currentFbmModel.ModelId
            exportModel.ORMModel.Name = currentFbmModel.Name

            If Not exportModel.MapFromFBMModel(currentFbmModel, False) Then
                Throw New InvalidOperationException(
                    "The Fact-Based Model contains errors and could not be mapped for export.")
            End If

            Dim serializer As New XmlSerializer(GetType(XMLModel.Model))
            Using writer As New StreamWriter(
                filePath,
                False,
                New UTF8Encoding(False))

                serializer.Serialize(writer, exportModel)
            End Using

            statusLabel.Text = $"Exported {Path.GetFileName(filePath)}"
            MessageBox.Show(
                Me,
                $"The Fact-Based Model was exported successfully.{Environment.NewLine}{Environment.NewLine}{filePath}",
                "Export Fact-Based Model",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)
        Catch ex As Exception
            statusLabel.Text = "Export failed"
            MessageBox.Show(
                Me,
                $"The Fact-Based Model could not be exported.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "Export Fact-Based Model",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        Finally
            Cursor = Cursors.Default
        End Try
    End Sub

    Private Function GetDefaultFbmFileName() As String
        If Not String.IsNullOrWhiteSpace(currentSourceFilePath) Then
            Return Path.GetFileNameWithoutExtension(currentSourceFilePath) & ".fbm"
        End If

        Dim fileName = ValueOrDash(currentFbmModel?.Name)
        For Each invalidCharacter In Path.GetInvalidFileNameChars()
            fileName = fileName.Replace(invalidCharacter, "_"c)
        Next

        Return fileName & ".fbm"
    End Function

    Private Sub SetupYamlEditor()
        Const SCE_YAML_DEFAULT As Integer = 0
        Const SCE_YAML_COMMENT As Integer = 1
        Const SCE_YAML_IDENTIFIER As Integer = 2
        Const SCE_YAML_KEYWORD As Integer = 3
        Const SCE_YAML_NUMBER As Integer = 4
        Const SCE_YAML_REFERENCE As Integer = 5
        Const SCE_YAML_DOCUMENT As Integer = 6
        Const SCE_YAML_TEXT As Integer = 7
        Const SCE_YAML_ERROR As Integer = 8
        Const SCE_YAML_OPERATOR As Integer = 9

        With yamlTextEditor
            .LexerName = "yaml"
            .SetProperty("fold", "1")
            .SetProperty("fold.compact", "0")

            .StyleResetDefault()
            .Styles(Style.Default).Font = "Consolas"
            .Styles(Style.Default).Size = 10
            .Styles(Style.Default).BackColor = Color.White
            .Styles(Style.Default).ForeColor = Color.Black
            .StyleClearAll()

            .Styles(SCE_YAML_DEFAULT).ForeColor = Color.Black
            .Styles(SCE_YAML_COMMENT).ForeColor = Color.FromArgb(106, 153, 85)
            .Styles(SCE_YAML_IDENTIFIER).ForeColor = Color.FromArgb(86, 156, 214)
            .Styles(SCE_YAML_IDENTIFIER).Bold = True
            .Styles(SCE_YAML_KEYWORD).ForeColor = Color.FromArgb(86, 156, 214)
            .Styles(SCE_YAML_KEYWORD).Bold = True
            .Styles(SCE_YAML_NUMBER).ForeColor = Color.FromArgb(181, 206, 168)
            .Styles(SCE_YAML_REFERENCE).ForeColor = Color.FromArgb(160, 120, 40)
            .Styles(SCE_YAML_DOCUMENT).ForeColor = Color.FromArgb(100, 100, 100)
            .Styles(SCE_YAML_DOCUMENT).Bold = True
            .Styles(SCE_YAML_TEXT).ForeColor = Color.FromArgb(163, 80, 45)
            .Styles(SCE_YAML_ERROR).ForeColor = Color.Red
            .Styles(SCE_YAML_ERROR).Bold = True
            .Styles(SCE_YAML_OPERATOR).ForeColor = Color.FromArgb(205, 75, 45)
            .Styles(SCE_YAML_OPERATOR).Bold = True

            .Margins(0).Type = MarginType.Number
            .Margins(0).Width = 44

            .Margins(2).Type = MarginType.Symbol
            .Margins(2).Width = 16
            .Margins(2).Sensitive = True
            .Margins(2).Mask = Marker.MaskFolders

            .Markers(Marker.Folder).Symbol = MarkerSymbol.Arrow
            .Markers(Marker.FolderOpen).Symbol = MarkerSymbol.ArrowDown
            .Markers(Marker.FolderEnd).Symbol = MarkerSymbol.Arrow
            .Markers(Marker.FolderMidTail).Symbol = MarkerSymbol.TCorner
            .Markers(Marker.FolderOpenMid).Symbol = MarkerSymbol.ArrowDown
            .Markers(Marker.FolderSub).Symbol = MarkerSymbol.VLine
            .Markers(Marker.FolderTail).Symbol = MarkerSymbol.LCorner

            For Each markerItem As Marker In .Markers
                markerItem.SetForeColor(Color.White)
                markerItem.SetBackColor(Color.FromArgb(86, 156, 214))
            Next

            .CaretLineBackColor = Color.FromArgb(80, 180, 210, 255)
            .TabWidth = 2
            .UseTabs = False
            .IndentWidth = 2
            .ReadOnly = True
        End With

        AddHandler yamlTextEditor.MarginClick, AddressOf YamlTextEditor_MarginClick
    End Sub

    Private Sub DisplayYamlText(yamlText As String)
        yamlTextEditor.ReadOnly = False
        yamlTextEditor.Text = If(yamlText, String.Empty)
        yamlTextEditor.EmptyUndoBuffer()
        yamlTextEditor.SetSavePoint()
        yamlTextEditor.ReadOnly = True
        yamlTextEditor.GotoPosition(0)
    End Sub

    Private Sub YamlTextEditor_MarginClick(
        sender As Object,
        e As MarginClickEventArgs)

        If e.Margin = 2 Then
            Dim lineIndex = yamlTextEditor.LineFromPosition(e.Position)
            yamlTextEditor.Lines(lineIndex).ToggleFold()
        End If
    End Sub

    Private Sub DisplayOntologyDocument(filePath As String, document As OntologyDocument)
        headingLabel.Text = "Ossie ontology"
        nameCaptionLabel.Text = "Name:"
        componentCountCaptionLabel.Text = "Ontology components:"
        mappingCountCaptionLabel.Text = "Ontology mappings:"
        filePathValue.Text = filePath
        nameValue.Text = ValueOrDash(document.Name)
        versionValue.Text = ValueOrDash(document.Version)
        descriptionValue.Text = ValueOrDash(document.Description)
        componentCountValue.Text = CountOf(document.Ontology).ToString()
        mappingCountValue.Text = CountOf(document.OntologyMappings).ToString()

        documentTree.BeginUpdate()
        documentTree.Nodes.Clear()
        ResetSearch()

        Dim semanticModelNodes = AddSemanticModelNodes(
            documentTree.Nodes,
            document.OntologyMappings)

        Dim conceptsNode = documentTree.Nodes.Add($"Ontology ({CountOf(document.Ontology)})")
        If document.Ontology IsNot Nothing Then
            For Each component In document.Ontology
                Dim conceptName = If(component?.Concept?.Name, "(unnamed concept)")
                Dim conceptType = If(component?.Concept Is Nothing, String.Empty, $" ({component.Concept.Type})")
                Dim conceptNode = conceptsNode.Nodes.Add(conceptName & conceptType)

                If component?.Relationships IsNot Nothing Then
                    For Each relationship In component.Relationships
                        AddRelationshipNode(conceptNode, document, conceptName, relationship)
                    Next
                End If
            Next
        End If

        Dim mappingsNode = documentTree.Nodes.Add($"Ontology mappings ({CountOf(document.OntologyMappings)})")
        If document.OntologyMappings IsNot Nothing Then
            For Each mapping In document.OntologyMappings
                Dim mappingNode = mappingsNode.Nodes.Add(If(mapping?.Name, "(unnamed mapping)"))
                If mapping?.SemanticModel IsNot Nothing Then
                    mappingNode.Nodes.Add($"Semantic model: {ValueOrDash(mapping.SemanticModel.Name)}")
                End If
                mappingNode.Nodes.Add($"Concept mappings: {CountOf(mapping?.ConceptMappings)}")
            Next
        End If

        For Each semanticModelNode In semanticModelNodes
            semanticModelNode.Expand()
        Next
        conceptsNode.Expand()
        mappingsNode.Expand()
        documentTree.EndUpdate()
    End Sub

    Private Sub DisplaySemanticModelDocument(
        filePath As String,
        document As SemanticModelDocument)

        Dim modelCount = CountOf(document.SemanticModel)
        Dim firstModel As SemanticModel = Nothing
        If modelCount = 1 Then
            firstModel = document.SemanticModel(0)
        End If

        headingLabel.Text = "Ossie semantic model"
        nameCaptionLabel.Text = "Name:"
        componentCountCaptionLabel.Text = "Semantic models:"
        mappingCountCaptionLabel.Text = "Datasets:"
        filePathValue.Text = filePath
        nameValue.Text = If(
            firstModel Is Nothing,
            If(modelCount = 0, ValueOrDash(Nothing), $"{modelCount} semantic models"),
            ValueOrDash(firstModel.Name))
        versionValue.Text = ValueOrDash(document.Version)
        descriptionValue.Text = If(
            firstModel Is Nothing,
            ValueOrDash(Nothing),
            ValueOrDash(firstModel.Description))
        componentCountValue.Text = modelCount.ToString()
        mappingCountValue.Text = CountDatasets(document.SemanticModel).ToString()

        documentTree.BeginUpdate()
        documentTree.Nodes.Clear()
        ResetSearch()

        Dim semanticModelNodes = AddSemanticModelNodes(
            documentTree.Nodes,
            document.SemanticModel)

        For Each semanticModelNode In semanticModelNodes
            semanticModelNode.Expand()
        Next

        documentTree.EndUpdate()
    End Sub

    Private Shared Function AddSemanticModelNodes(
        rootNodes As TreeNodeCollection,
        ontologyMaps As ICollection(Of OntologyMap)) As List(Of TreeNode)

        Dim addedNodes As New List(Of TreeNode)()
        If ontologyMaps Is Nothing Then
            Return addedNodes
        End If

        For Each ontologyMap In ontologyMaps
            Dim semanticModel = ontologyMap?.SemanticModel
            If semanticModel Is Nothing Then
                Continue For
            End If

            Dim modelNode = AddSemanticModelNode(rootNodes, semanticModel)
            addedNodes.Add(modelNode)

            modelNode.Nodes.Add(
                $"Ontology mapping: {ValueOrDash(ontologyMap.Name)}")
        Next

        Return addedNodes
    End Function

    Private Shared Function AddSemanticModelNodes(
        rootNodes As TreeNodeCollection,
        semanticModels As ICollection(Of SemanticModel)) As List(Of TreeNode)

        Dim addedNodes As New List(Of TreeNode)()
        If semanticModels Is Nothing Then
            Return addedNodes
        End If

        For Each semanticModel In semanticModels
            If semanticModel Is Nothing Then
                Continue For
            End If

            addedNodes.Add(AddSemanticModelNode(rootNodes, semanticModel))
        Next

        Return addedNodes
    End Function

    Private Shared Function AddSemanticModelNode(
        rootNodes As TreeNodeCollection,
        semanticModel As SemanticModel) As TreeNode

        Dim modelNode = rootNodes.Add(
            $"Semantic model: {ValueOrDash(semanticModel.Name)}")

        If Not String.IsNullOrWhiteSpace(semanticModel.Description) Then
            modelNode.Nodes.Add($"Description: {semanticModel.Description}")
        End If

        Dim datasetsNode = modelNode.Nodes.Add(
            $"Datasets ({CountOf(semanticModel.Datasets)})")
        AddDatasetNodes(datasetsNode, semanticModel.Datasets)
        datasetsNode.Expand()

        If semanticModel.Relationships IsNot Nothing AndAlso
            semanticModel.Relationships.Count > 0 Then
            Dim relationshipsNode = modelNode.Nodes.Add(
                $"Dataset relationships ({semanticModel.Relationships.Count})")
            AddDatasetRelationshipNodes(
                relationshipsNode,
                semanticModel.Relationships)
        End If

        If semanticModel.Metrics IsNot Nothing AndAlso
            semanticModel.Metrics.Count > 0 Then
            Dim metricsNode = modelNode.Nodes.Add(
                $"Metrics ({semanticModel.Metrics.Count})")
            AddMetricNodes(metricsNode, semanticModel.Metrics)
        End If

        Return modelNode
    End Function

    Private Shared Sub AddDatasetNodes(
        datasetsNode As TreeNode,
        datasets As ICollection(Of Dataset))

        If datasets Is Nothing Then
            Return
        End If

        For Each dataset In datasets
            If dataset Is Nothing Then
                Continue For
            End If

            Dim datasetNode = datasetsNode.Nodes.Add(
                $"Dataset: {ValueOrDash(dataset.Name)}")
            datasetNode.Nodes.Add($"Source: {ValueOrDash(dataset.Source)}")

            If Not String.IsNullOrWhiteSpace(dataset.Description) Then
                datasetNode.Nodes.Add($"Description: {dataset.Description}")
            End If

            If dataset.PrimaryKey IsNot Nothing AndAlso
                dataset.PrimaryKey.Count > 0 Then
                datasetNode.Nodes.Add(
                    $"Primary key: {String.Join(", ", dataset.PrimaryKey)}")
            End If

            Dim fieldsNode = datasetNode.Nodes.Add(
                $"Fields ({CountOf(dataset.Fields)})")
            If dataset.Fields Is Nothing Then
                Continue For
            End If

            For Each field In dataset.Fields
                If field Is Nothing Then
                    Continue For
                End If

                Dim fieldNode = fieldsNode.Nodes.Add(
                    $"Field: {ValueOrDash(field.Name)}")
                AddExpressionNodes(fieldNode, field.Expression)

                If Not String.IsNullOrWhiteSpace(field.Description) Then
                    fieldNode.Nodes.Add($"Description: {field.Description}")
                End If
            Next
        Next
    End Sub

    Private Shared Sub AddDatasetRelationshipNodes(
        relationshipsNode As TreeNode,
        relationships As ICollection(Of DatasetRelationship))

        For Each relationship In relationships
            If relationship Is Nothing Then
                Continue For
            End If

            Dim relationshipNode = relationshipsNode.Nodes.Add(
                $"Relationship: {ValueOrDash(relationship.Name)}")
            relationshipNode.Nodes.Add(
                $"From: {ValueOrDash(relationship.FromDataset)}")
            relationshipNode.Nodes.Add(
                $"To: {ValueOrDash(relationship.ToDataset)}")

            If relationship.FromColumns IsNot Nothing Then
                relationshipNode.Nodes.Add(
                    $"From columns: {String.Join(", ", relationship.FromColumns)}")
            End If
            If relationship.ToColumns IsNot Nothing Then
                relationshipNode.Nodes.Add(
                    $"To columns: {String.Join(", ", relationship.ToColumns)}")
            End If
        Next
    End Sub

    Private Shared Sub AddMetricNodes(
        metricsNode As TreeNode,
        metrics As ICollection(Of Metric))

        For Each metric In metrics
            If metric Is Nothing Then
                Continue For
            End If

            Dim metricNode = metricsNode.Nodes.Add(
                $"Metric: {ValueOrDash(metric.Name)}")
            AddExpressionNodes(metricNode, metric.Expression)

            If Not String.IsNullOrWhiteSpace(metric.Description) Then
                metricNode.Nodes.Add($"Description: {metric.Description}")
            End If
        Next
    End Sub

    Private Shared Sub AddExpressionNodes(
        parentNode As TreeNode,
        expression As Expression)

        If expression?.Dialects Is Nothing Then
            Return
        End If

        Dim expressionNode = parentNode.Nodes.Add("Expression")
        For Each dialectExpression In expression.Dialects
            If dialectExpression Is Nothing Then
                Continue For
            End If

            expressionNode.Nodes.Add(
                $"{dialectExpression.Dialect}: {ValueOrDash(dialectExpression.Expression)}")
        Next
    End Sub

    Private Sub DisplayFbmModel(model As FBM.Model)
        fbmTree.BeginUpdate()
        fbmTree.Nodes.Clear()
        ResetSearch()

        If model Is Nothing Then
            fbmTree.EndUpdate()
            Return
        End If

        Dim modelNode = fbmTree.Nodes.Add(
            $"Fact-Based Model: {ValueOrDash(model.Name)}")
        modelNode.Nodes.Add($"Model ID: {ValueOrDash(model.ModelId)}")

        Dim larValueTypes As List(Of FBM.ValueType) =
            model.ValueType.
                Where(
                    Function(arValueType As FBM.ValueType)
                        Return arValueType IsNot Nothing AndAlso
                            Not arValueType.IsMDAModelElement
                    End Function).
                ToList()
        Dim valueTypesNode = modelNode.Nodes.Add(
            $"Value Types ({larValueTypes.Count})")
        For Each valueType In larValueTypes.
            OrderBy(Function(item) item.Name, StringComparer.OrdinalIgnoreCase)
            Dim valueTypeNode = valueTypesNode.Nodes.Add(
                $"Value Type: {ValueOrDash(valueType.Name)}")
            AddFbmModelObjectDetails(valueTypeNode, valueType)
            valueTypeNode.Nodes.Add($"Data type: {valueType.DataType}")
        Next

        Dim larEntityTypes As List(Of FBM.EntityType) =
            model.EntityType.
                Where(
                    Function(arEntityType As FBM.EntityType)
                        Return arEntityType IsNot Nothing AndAlso
                            Not arEntityType.IsMDAModelElement
                    End Function).
                ToList()
        Dim entityTypesNode = modelNode.Nodes.Add(
            $"Entity Types ({larEntityTypes.Count})")
        For Each entityType In larEntityTypes.
            OrderBy(Function(item) item.Name, StringComparer.OrdinalIgnoreCase)
            Dim entityTypeNode = entityTypesNode.Nodes.Add(
                $"Entity Type: {ValueOrDash(entityType.Name)}")
            AddFbmModelObjectDetails(entityTypeNode, entityType)
        Next

        Dim larFactTypes As List(Of FBM.FactType) =
            model.FactType.
                Where(
                    Function(arFactType As FBM.FactType)
                        Return arFactType IsNot Nothing AndAlso
                            Not arFactType.IsMDAModelElement
                    End Function).
                ToList()
        Dim factTypesNode = modelNode.Nodes.Add(
            $"Fact Types ({larFactTypes.Count})")
        For Each factType In larFactTypes.
            OrderBy(Function(item) item.Name, StringComparer.OrdinalIgnoreCase)
            AddFbmFactTypeNode(factTypesNode, factType)
        Next

        modelNode.Expand()
        valueTypesNode.Expand()
        entityTypesNode.Expand()
        factTypesNode.Expand()
        fbmTree.EndUpdate()
    End Sub

    Private Shared Sub AddFbmFactTypeNode(
        factTypesNode As TreeNode,
        factType As FBM.FactType)

        If factType Is Nothing Then
            Return
        End If

        Dim factTypeNode = factTypesNode.Nodes.Add(
            $"Fact Type: {ValueOrDash(factType.Name)}")
        AddFbmModelObjectDetails(factTypeNode, factType)

        Dim rolesNode = factTypeNode.Nodes.Add(
            $"Roles ({factType.RoleGroup.Count})")
        For Each role In factType.RoleGroup.
            OrderBy(Function(item) item.SequenceNr)
            Dim objectTypeName = ValueOrDash(role.JoinedORMObject?.Name)
            Dim roleName = If(
                String.IsNullOrWhiteSpace(role.Name),
                String.Empty,
                $" [{role.Name}]")
            rolesNode.Nodes.Add(
                $"{role.SequenceNr}: {objectTypeName}{roleName}")
        Next

        Dim readingsNode = factTypeNode.Nodes.Add(
            $"Readings ({factType.FactTypeReading.Count})")
        For Each reading In factType.FactTypeReading
            readingsNode.Nodes.Add(FormatFbmReading(reading))
        Next

        If factType.InternalUniquenessConstraint IsNot Nothing AndAlso
            factType.InternalUniquenessConstraint.Count > 0 Then
            factTypeNode.Nodes.Add(
                $"Internal uniqueness constraints: {factType.InternalUniquenessConstraint.Count}")
        End If
    End Sub

    Private Shared Sub AddFbmModelObjectDetails(
        parentNode As TreeNode,
        modelObject As FBM.ModelObject)

        parentNode.Nodes.Add($"ID: {ValueOrDash(modelObject.Id)}")

        If modelObject.Alias IsNot Nothing AndAlso
            modelObject.Alias.Count > 0 Then
            Dim aliasesNode = parentNode.Nodes.Add(
                $"Aliases ({modelObject.Alias.Count})")
            For Each modelAlias In modelObject.Alias
                aliasesNode.Nodes.Add(
                    $"{modelAlias.AliasType}: {ValueOrDash(modelAlias.Alias)}")
            Next
        End If

        If Not String.IsNullOrWhiteSpace(modelObject.ShortDescription) Then
            parentNode.Nodes.Add(
                $"Description: {modelObject.ShortDescription}")
        End If

        If IsFbmModelObjectDerived(modelObject) Then
            Dim derivationNode = parentNode.Nodes.Add("Derivation Rule")
            Dim derivationText = GetFbmDerivationText(modelObject)
            If String.IsNullOrWhiteSpace(derivationText) Then
                Dim missingRuleNode = derivationNode.Nodes.Add("Not specified")
                missingRuleNode.ForeColor = SystemColors.GrayText
                Return
            End If

            Dim ruleLines = derivationText.Split(
                {vbCrLf, vbLf, vbCr},
                StringSplitOptions.RemoveEmptyEntries)
            For Each ruleLine In ruleLines
                derivationNode.Nodes.Add(ruleLine.Trim())
            Next
        End If
    End Sub

    Private Shared Function IsFbmModelObjectDerived(
        modelObject As FBM.ModelObject) As Boolean

        If TypeOf modelObject Is FBM.EntityType Then
            Return DirectCast(modelObject, FBM.EntityType).IsDerived
        End If
        If TypeOf modelObject Is FBM.FactType Then
            Return DirectCast(modelObject, FBM.FactType).IsDerived
        End If

        Return modelObject.IsDerived
    End Function

    Private Shared Function GetFbmDerivationText(
        modelObject As FBM.ModelObject) As String

        If TypeOf modelObject Is FBM.EntityType Then
            Return DirectCast(modelObject, FBM.EntityType).DerivationText
        End If
        If TypeOf modelObject Is FBM.FactType Then
            Return DirectCast(modelObject, FBM.FactType).DerivationText
        End If

        Return modelObject.DerivationText
    End Function

    Private Shared Function FormatFbmReading(
        reading As FBM.FactTypeReading) As String

        If reading Is Nothing Then
            Return "(empty reading)"
        End If

        Dim text As New StringBuilder(If(reading.FrontText, String.Empty))
        For Each predicatePart In reading.PredicatePart.
            OrderBy(Function(item) item.SequenceNr)
            text.Append(If(predicatePart.PreBoundText, String.Empty))
            text.Append("{"c)
            text.Append(ValueOrDash(predicatePart.Role?.JoinedORMObject?.Name))
            text.Append("}"c)
            text.Append(If(predicatePart.PostBoundText, String.Empty))
            text.Append(If(predicatePart.PredicatePartText, String.Empty))
        Next
        text.Append(If(reading.FollowingText, String.Empty))

        Return text.ToString()
    End Function

    Private Sub AddRelationshipNode(
        conceptNode As TreeNode,
        document As OntologyDocument,
        conceptName As String,
        relationship As OntologyRelationship)

        If relationship Is Nothing Then
            conceptNode.Nodes.Add("Relationship: (unnamed)")
            Return
        End If

        Dim relationshipName = If(relationship.Name, "(unnamed)")
        Dim relationshipNode = conceptNode.Nodes.Add($"Relationship: {relationshipName}")
        AddRoleNodes(relationshipNode, conceptName, relationship)

        relationshipNode.Nodes.Add(
            $"Multiplicity: {If(relationship.Multiplicity.HasValue, relationship.Multiplicity.Value.ToString(), "Not specified")}")

        If relationship.Requires IsNot Nothing AndAlso
            relationship.Requires.Count > 0 Then
            Dim requiresNode = relationshipNode.Nodes.Add(
                $"Requires ({relationship.Requires.Count})")
            For Each expression In relationship.Requires
                requiresNode.Nodes.Add(expression)
            Next
        End If

        Dim verbalizesCount = CountOf(relationship.Verbalizes)
        Dim verbalizesNode = relationshipNode.Nodes.Add(
            $"Verbalizes ({verbalizesCount})")
        If verbalizesCount = 0 Then
            Dim missingReadingNode = verbalizesNode.Nodes.Add("Not specified")
            missingReadingNode.ForeColor = SystemColors.GrayText
        Else
            For Each factTypeReading In relationship.Verbalizes
                verbalizesNode.Nodes.Add(factTypeReading)
            Next
        End If

        If relationship.DerivedBy IsNot Nothing Then
            Dim derivedNode = relationshipNode.Nodes.Add("Derived by")
            For Each expression In relationship.DerivedBy
                derivedNode.Nodes.Add(expression)
            Next
        End If

        Dim mappingsNode = relationshipNode.Nodes.Add("Mappings")
        Dim mappingCount = AddMappings(
            mappingsNode,
            document.OntologyMappings,
            conceptName,
            relationship)

        If mappingCount = 0 Then
            Dim unmappedNode = mappingsNode.Nodes.Add("Not explicitly mapped")
            unmappedNode.ForeColor = SystemColors.GrayText
        Else
            mappingsNode.Text = $"Mappings ({mappingCount})"
        End If
    End Sub

    Private Shared Sub AddRoleNodes(
        relationshipNode As TreeNode,
        conceptName As String,
        relationship As OntologyRelationship)

        Dim rolesNode = relationshipNode.Nodes.Add("Roles")
        rolesNode.Nodes.Add($"1: {conceptName}")

        If relationship.Roles Is Nothing Then
            Return
        End If

        For index = 0 To relationship.Roles.Count - 1
            Dim role = relationship.Roles(index)
            Dim roleConcept = If(role?.Concept, "(unnamed concept)")
            Dim roleName = If(
                String.IsNullOrWhiteSpace(role?.Name),
                String.Empty,
                $" [{role.Name}]")
            rolesNode.Nodes.Add($"{index + 2}: {roleConcept}{roleName}")
        Next
    End Sub

    Private Shared Function AddMappings(
        mappingsNode As TreeNode,
        ontologyMaps As ICollection(Of OntologyMap),
        conceptName As String,
        relationship As OntologyRelationship) As Integer

        Dim mappingsAdded = 0
        If ontologyMaps Is Nothing Then
            Return mappingsAdded
        End If

        For Each ontologyMap In ontologyMaps
            If ontologyMap?.ConceptMappings Is Nothing Then
                Continue For
            End If

            Dim mapName = ValueOrDash(ontologyMap.Name)
            For Each conceptMapping In ontologyMap.ConceptMappings
                If conceptMapping Is Nothing OrElse
                    Not String.Equals(conceptMapping.Concept, conceptName, StringComparison.OrdinalIgnoreCase) Then
                    Continue For
                End If

                If conceptMapping.ObjectMappings IsNot Nothing Then
                    For Each objectMapping In conceptMapping.ObjectMappings
                        If objectMapping?.ReferentMappings Is Nothing Then
                            Continue For
                        End If

                        For Each referentMapping In objectMapping.ReferentMappings
                            If Not RelationshipNamesMatch(referentMapping?.Relationship, relationship.Name) Then
                                Continue For
                            End If

                            Dim mappingNode = mappingsNode.Nodes.Add($"Referent mapping — {mapName}")
                            Dim targetConcept = GetRelationshipTargetConcept(relationship)
                            mappingNode.Nodes.Add($"Target: {targetConcept}")
                            AddReferentMappingNode(mappingNode, referentMapping)
                            mappingsAdded += 1
                        Next
                    Next
                End If

                mappingsAdded += AddLinkMappings(
                    mappingsNode,
                    conceptMapping.LinkMappings,
                    relationship,
                    conceptName,
                    mapName,
                    New List(Of ObjectMapping)())
            Next
        Next

        Return mappingsAdded
    End Function

    Private Shared Function AddLinkMappings(
        mappingsNode As TreeNode,
        linkMappings As ICollection(Of LinkMapping),
        targetRelationship As OntologyRelationship,
        containingConcept As String,
        mapName As String,
        parentPath As List(Of ObjectMapping)) As Integer

        Dim mappingsAdded = 0
        If linkMappings Is Nothing Then
            Return mappingsAdded
        End If

        For Each linkMapping In linkMappings
            If linkMapping Is Nothing Then
                Continue For
            End If

            Dim currentPath As New List(Of ObjectMapping)(parentPath)
            currentPath.Add(linkMapping.ObjectMapping)

            If RelationshipNamesMatch(linkMapping.Relationship, targetRelationship.Name) Then
                Dim mappingNode = mappingsNode.Nodes.Add($"Link mapping — {mapName}")

                For index = 0 To currentPath.Count - 1
                    Dim inferredConcept = GetRoleConcept(
                        containingConcept,
                        targetRelationship,
                        index)
                    AddObjectMappingNode(
                        mappingNode,
                        index + 1,
                        inferredConcept,
                        currentPath(index))
                Next

                mappingsAdded += 1
            End If

            mappingsAdded += AddLinkMappings(
                mappingsNode,
                linkMapping.Children,
                targetRelationship,
                containingConcept,
                mapName,
                currentPath)
        Next

        Return mappingsAdded
    End Function

    Private Shared Sub AddObjectMappingNode(
        mappingNode As TreeNode,
        roleNumber As Integer,
        inferredConcept As String,
        objectMapping As ObjectMapping)

        Dim mappedConcept = If(
            String.IsNullOrWhiteSpace(objectMapping?.Concept),
            inferredConcept,
            objectMapping.Concept)
        Dim roleNode = mappingNode.Nodes.Add($"Role {roleNumber}: {mappedConcept}")

        If objectMapping Is Nothing Then
            roleNode.Nodes.Add("(no object mapping)")
        ElseIf Not String.IsNullOrWhiteSpace(objectMapping.Expression) Then
            roleNode.Nodes.Add($"Expression: {objectMapping.Expression}")
        ElseIf objectMapping.ReferentMappings IsNot Nothing Then
            Dim identifiersNode = roleNode.Nodes.Add("Identified by")
            For Each referentMapping In objectMapping.ReferentMappings
                AddReferentMappingNode(identifiersNode, referentMapping)
            Next
        Else
            roleNode.Nodes.Add("(empty object mapping)")
        End If
    End Sub

    Private Shared Sub AddReferentMappingNode(
        parentNode As TreeNode,
        referentMapping As ReferentMapping)

        If referentMapping Is Nothing Then
            parentNode.Nodes.Add("(empty referent mapping)")
            Return
        End If

        Dim relationshipName = If(referentMapping.Relationship, "(unnamed relationship)")
        If Not String.IsNullOrWhiteSpace(referentMapping.Expression) Then
            parentNode.Nodes.Add($"{relationshipName} ← {referentMapping.Expression}")
            Return
        End If

        Dim referentNode = parentNode.Nodes.Add(relationshipName)
        If referentMapping.ReferentMappings Is Nothing Then
            referentNode.Nodes.Add("(empty referent mapping)")
            Return
        End If

        For Each nestedMapping In referentMapping.ReferentMappings
            AddReferentMappingNode(referentNode, nestedMapping)
        Next
    End Sub

    Private Shared Function GetRelationshipTargetConcept(
        relationship As OntologyRelationship) As String

        If relationship?.Roles Is Nothing OrElse relationship.Roles.Count = 0 Then
            Return "(unary relationship)"
        End If

        Return If(relationship.Roles(0)?.Concept, "(unnamed concept)")
    End Function

    Private Shared Function GetRoleConcept(
        containingConcept As String,
        relationship As OntologyRelationship,
        zeroBasedRoleIndex As Integer) As String

        If zeroBasedRoleIndex = 0 Then
            Return containingConcept
        End If

        Dim additionalRoleIndex = zeroBasedRoleIndex - 1
        If relationship?.Roles Is Nothing OrElse
            additionalRoleIndex >= relationship.Roles.Count Then
            Return "(unknown concept)"
        End If

        Return If(relationship.Roles(additionalRoleIndex)?.Concept, "(unnamed concept)")
    End Function

    Private Shared Function RelationshipNamesMatch(
        mappedName As String,
        relationshipName As String) As Boolean

        If String.IsNullOrWhiteSpace(mappedName) OrElse
            String.IsNullOrWhiteSpace(relationshipName) Then
            Return False
        End If

        Dim separatorIndex = mappedName.LastIndexOf("."c)
        Dim unqualifiedName = If(
            separatorIndex < 0,
            mappedName,
            mappedName.Substring(separatorIndex + 1))

        Return String.Equals(
            unqualifiedName,
            relationshipName,
            StringComparison.OrdinalIgnoreCase)
    End Function

    Private Sub ShowEmptyState()
        headingLabel.Text = "Ossie document viewer"
        nameCaptionLabel.Text = "Name:"
        componentCountCaptionLabel.Text = "Ontology components:"
        mappingCountCaptionLabel.Text = "Ontology mappings:"
        filePathValue.Text = "No file imported"
        nameValue.Text = "—"
        versionValue.Text = "—"
        descriptionValue.Text = "Use File → Import Ossie YAML file to begin."
        componentCountValue.Text = "0"
        mappingCountValue.Text = "0"
        documentTree.Nodes.Clear()
        fbmTree.Nodes.Clear()
        DisplayYamlText(String.Empty)
        exportFbmMenuItem.Enabled = False
    End Sub

    Private Shared Function CountDatasets(
        semanticModels As ICollection(Of SemanticModel)) As Integer

        Dim count = 0
        If semanticModels Is Nothing Then
            Return count
        End If

        For Each semanticModel In semanticModels
            count += CountOf(semanticModel?.Datasets)
        Next

        Return count
    End Function

    Private Shared Function CountOf(Of T)(items As ICollection(Of T)) As Integer
        Return If(items?.Count, 0)
    End Function

    Private Shared Function ValueOrDash(value As String) As String
        Return If(String.IsNullOrWhiteSpace(value), "—", value)
    End Function

End Class
