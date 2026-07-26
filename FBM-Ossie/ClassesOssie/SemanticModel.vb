Imports System.Collections.Generic
Imports YamlDotNet.Serialization

Namespace Ossie
    ''' <summary>Top-level container for a complete logical semantic model.</summary>
    Public Class SemanticModel
        ''' <summary>Unique identifier for the semantic model.</summary>
        Public Property Name As String

        ''' <summary>Human-readable model description.</summary>
        Public Property Description As String

        ''' <summary>AI context supplied as either text or an <see cref="AiContext"/> object.</summary>
        <YamlMember(Alias:="ai_context")>
        Public Property AiContext As Object

        ''' <summary>Logical datasets defined by the model.</summary>
        Public Property Datasets As List(Of Dataset)

        ''' <summary>Foreign-key relationships between datasets.</summary>
        Public Property Relationships As List(Of DatasetRelationship)

        ''' <summary>Quantitative measures spanning the datasets.</summary>
        Public Property Metrics As List(Of Metric)

        ''' <summary>Vendor-specific extension data.</summary>
        <YamlMember(Alias:="custom_extensions")>
        Public Property CustomExtensions As List(Of CustomExtension)
    End Class
End Namespace
