Imports System.Collections.Generic
Imports YamlDotNet.Serialization

Namespace Ossie
    ''' <summary>Logical dataset representing a business entity or table-like source.</summary>
    Public Class Dataset
        ''' <summary>Unique dataset identifier.</summary>
        Public Property Name As String

        ''' <summary>Physical table, view, or query that supplies the dataset.</summary>
        Public Property Source As String

        ''' <summary>Single or composite primary-key field names.</summary>
        <YamlMember(Alias:="primary_key")>
        Public Property PrimaryKey As List(Of String)

        ''' <summary>Alternate single or composite unique-key definitions.</summary>
        <YamlMember(Alias:="unique_keys")>
        Public Property UniqueKeys As List(Of List(Of String))

        ''' <summary>Human-readable dataset description.</summary>
        Public Property Description As String

        ''' <summary>AI context supplied as either text or an <see cref="AiContext"/> object.</summary>
        <YamlMember(Alias:="ai_context")>
        Public Property AiContext As Object

        ''' <summary>Fields exposed by the dataset.</summary>
        Public Property Fields As List(Of Field)

        ''' <summary>Vendor-specific extension data.</summary>
        <YamlMember(Alias:="custom_extensions")>
        Public Property CustomExtensions As List(Of CustomExtension)
    End Class
End Namespace
