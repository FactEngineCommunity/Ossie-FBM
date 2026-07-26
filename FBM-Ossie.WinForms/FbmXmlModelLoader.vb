Imports System
Imports System.IO
Imports System.Xml.Linq
Imports System.Xml.Serialization
Imports FBM = FactEngineForServices.FBM
Imports XMLModel = FactEngineForServices.XMLModel

''' <summary>
''' Imports a FactEngine XML .fbm file using the same current-format sequence
''' used by Boston.
''' </summary>
Public NotInheritable Class FbmXmlModelLoader

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Deserializes a version 1.7 .fbm file, maps it to an FBM Model, and
    ''' materializes embedded Core structures such as the RDS.
    ''' </summary>
    ''' <param name="asFileName">The XML .fbm file to import.</param>
    ''' <returns>The fully imported FactEngine Model.</returns>
    Public Shared Function Load(
        ByVal asFileName As String) As FBM.Model

        If String.IsNullOrWhiteSpace(asFileName) Then
            Throw New ArgumentException(
                "A Fact-Based Model file path is required.",
                NameOf(asFileName))
        End If
        If Not File.Exists(asFileName) Then
            Throw New FileNotFoundException(
                "The Fact-Based Model file could not be found.",
                asFileName)
        End If

        Dim lrXMLDocument As XDocument =
            XDocument.Load(asFileName)
        If lrXMLDocument.Root Is Nothing Then
            Throw New InvalidDataException(
                "The selected file does not contain an XML document root.")
        End If

        Dim lsXMLNamespace As String =
            lrXMLDocument.Root.GetDefaultNamespace().NamespaceName
        If Not String.IsNullOrWhiteSpace(lsXMLNamespace) AndAlso
            lsXMLNamespace.StartsWith(
                "https://www.fbmwg.org/fbm",
                StringComparison.OrdinalIgnoreCase) Then
            Throw New NotSupportedException(
                "FBM Working Group version 0.1 files are not supported by this importer yet.")
        End If

        Dim lsXSDVersionNumber As String =
            CStr(
                lrXMLDocument.Root.Attribute(
                    "XSDVersionNr"))
        If Not String.Equals(
            lsXSDVersionNumber,
            "1.7",
            StringComparison.Ordinal) Then
            Throw New NotSupportedException(
                $"Unsupported FactEngine XML schema version '{lsXSDVersionNumber}'. This importer currently supports version 1.7.")
        End If

        Dim lrSerializer As New XmlSerializer(
            GetType(XMLModel.Model))
        Dim lrXMLModel As XMLModel.Model

        Using lrStreamReader As New StreamReader(asFileName)
            lrXMLModel = DirectCast(
                lrSerializer.Deserialize(lrStreamReader),
                XMLModel.Model)
        End Using

        If lrXMLModel?.ORMModel Is Nothing Then
            Throw New InvalidDataException(
                "The selected file does not contain a FactEngine ORM Model.")
        End If

        ' Keep the Boston defaults here. In particular, do not override
        ' abToConceptInstancesOnly while importing the XML model.
        Dim lrFBMModel As FBM.Model =
            lrXMLModel.MapToFBMModel()
        If lrFBMModel Is Nothing Then
            Throw New InvalidDataException(
                "FactEngineForServices did not return an imported FBM Model.")
        End If

        LoadAllCMMLStructures(lrFBMModel)
        Return lrFBMModel
    End Function

    ''' <summary>
    ''' Materializes the RDS, STM, and other Core-backed structures carried as
    ''' facts inside an imported Model.
    ''' </summary>
    ''' <param name="arFBMModel">The imported FactEngine Model.</param>
    Public Shared Sub LoadAllCMMLStructures(
        ByRef arFBMModel As FBM.Model)

        If arFBMModel Is Nothing Then
            Throw New ArgumentNullException(NameOf(arFBMModel))
        End If
        If String.Equals(
            arFBMModel.ModelId,
            "Core",
            StringComparison.OrdinalIgnoreCase) Then
            Return
        End If

        If Not arFBMModel.HasCoreModel Then
            ' Boston can clone its application-level Core pages here. This
            ' standalone importer has no Boston project/UI context, so retain
            ' the imported ORM Model and leave its RDS unmaterialized.
            arFBMModel.RDSCreated = False
            Return
        End If

        arFBMModel.performCoreManagement(False)
        arFBMModel.PopulateAllCoreStructuresFromCoreMDAElements(
            Nothing)
        arFBMModel.RDSCreated = True
    End Sub
End Class
