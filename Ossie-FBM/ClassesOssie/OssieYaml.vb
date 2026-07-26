Imports System
Imports System.Collections.Generic
Imports System.IO
Imports YamlDotNet.Serialization
Imports YamlDotNet.Serialization.NamingConventions

Namespace Ossie
    ''' <summary>Creates YAML serializers configured for Apache Ossie's snake_case document format.</summary>
    Public NotInheritable Class OssieYaml
        Private Sub New()
        End Sub

        ''' <summary>Creates a serializer that emits snake_case YAML member names.</summary>
        Public Shared Function CreateSerializer() As ISerializer
            Return New SerializerBuilder().
                WithNamingConvention(
                    UnderscoredNamingConvention.Instance).
                ConfigureDefaultValuesHandling(
                    DefaultValuesHandling.OmitNull Or
                    DefaultValuesHandling.OmitEmptyCollections).
                Build()
        End Function

        ''' <summary>Creates a deserializer that reads snake_case YAML member names.</summary>
        Public Shared Function CreateDeserializer() As IDeserializer
            Return New DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build()
        End Function

        ''' <summary>Deserializes an Apache Ossie ontology document from YAML text.</summary>
        ''' <param name="yaml">The complete YAML document.</param>
        ''' <returns>The deserialized Ossie ontology document.</returns>
        Public Shared Function DeserializeOntology(yaml As String) As OntologyDocument
            If yaml Is Nothing Then
                Throw New ArgumentNullException(NameOf(yaml))
            End If

            Using reader As New StringReader(yaml)
                Return DeserializeOntology(reader)
            End Using
        End Function

        ''' <summary>Deserializes an Apache Ossie ontology document from a text reader.</summary>
        ''' <param name="reader">A reader positioned at the start of an Ossie YAML document.</param>
        ''' <returns>The deserialized Ossie ontology document.</returns>
        Public Shared Function DeserializeOntology(reader As TextReader) As OntologyDocument
            If reader Is Nothing Then
                Throw New ArgumentNullException(NameOf(reader))
            End If

            Return CreateDeserializer().Deserialize(Of OntologyDocument)(reader)
        End Function

        ''' <summary>Deserializes an Apache Ossie semantic-model document from YAML text.</summary>
        Public Shared Function DeserializeSemanticModel(yaml As String) As SemanticModelDocument
            If yaml Is Nothing Then
                Throw New ArgumentNullException(NameOf(yaml))
            End If

            Using reader As New StringReader(yaml)
                Return DeserializeSemanticModel(reader)
            End Using
        End Function

        ''' <summary>Deserializes an Apache Ossie semantic-model document from a text reader.</summary>
        Public Shared Function DeserializeSemanticModel(reader As TextReader) As SemanticModelDocument
            If reader Is Nothing Then
                Throw New ArgumentNullException(NameOf(reader))
            End If

            Return CreateDeserializer().Deserialize(Of SemanticModelDocument)(reader)
        End Function

        ''' <summary>
        ''' Detects and deserializes either an Ossie ontology document or a standalone
        ''' semantic-model document.
        ''' </summary>
        Public Shared Function DeserializeDocument(yaml As String) As OssieDocument
            If yaml Is Nothing Then
                Throw New ArgumentNullException(NameOf(yaml))
            End If

            Dim root = CreateDeserializer().
                Deserialize(Of Dictionary(Of String, Object))(yaml)

            If root Is Nothing Then
                Throw New InvalidDataException("The YAML file did not contain an Ossie document.")
            End If

            If HasRootMember(root, "ontology") OrElse
                HasRootMember(root, "ontology_mappings") Then
                Return DeserializeOntology(yaml)
            End If

            If HasRootMember(root, "semantic_model") Then
                Return DeserializeSemanticModel(yaml)
            End If

            Throw New InvalidDataException(
                "The YAML document contains neither an ontology nor a semantic_model section.")
        End Function

        ''' <summary>Deserializes any supported Ossie document from a text reader.</summary>
        Public Shared Function DeserializeDocument(reader As TextReader) As OssieDocument
            If reader Is Nothing Then
                Throw New ArgumentNullException(NameOf(reader))
            End If

            Return DeserializeDocument(reader.ReadToEnd())
        End Function

        ''' <summary>Imports an Apache Ossie ontology document from a YAML file.</summary>
        ''' <param name="filePath">Path to the .yaml or .yml file.</param>
        ''' <returns>The deserialized Ossie ontology document.</returns>
        Public Shared Function ImportOntology(filePath As String) As OntologyDocument
            If String.IsNullOrWhiteSpace(filePath) Then
                Throw New ArgumentException("An Ossie YAML file path is required.", NameOf(filePath))
            End If

            Using reader As New StreamReader(filePath)
                Return DeserializeOntology(reader)
            End Using
        End Function

        ''' <summary>Imports an Apache Ossie standalone semantic-model document.</summary>
        Public Shared Function ImportSemanticModel(filePath As String) As SemanticModelDocument
            ValidateFilePath(filePath)

            Using reader As New StreamReader(filePath)
                Return DeserializeSemanticModel(reader)
            End Using
        End Function

        ''' <summary>
        ''' Imports either an Apache Ossie ontology document or a standalone
        ''' semantic-model document, based on its root YAML sections.
        ''' </summary>
        Public Shared Function ImportDocument(filePath As String) As OssieDocument
            ValidateFilePath(filePath)

            Using reader As New StreamReader(filePath)
                Return DeserializeDocument(reader)
            End Using
        End Function

        Private Shared Sub ValidateFilePath(filePath As String)
            If String.IsNullOrWhiteSpace(filePath) Then
                Throw New ArgumentException("An Ossie YAML file path is required.", NameOf(filePath))
            End If
        End Sub

        Private Shared Function HasRootMember(
            root As Dictionary(Of String, Object),
            memberName As String) As Boolean

            For Each key In root.Keys
                If String.Equals(key, memberName, StringComparison.OrdinalIgnoreCase) Then
                    Return True
                End If
            Next

            Return False
        End Function
    End Class
End Namespace
