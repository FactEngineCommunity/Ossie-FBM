Imports System.Collections.Generic
Imports YamlDotNet.Serialization

Namespace Ossie
    ''' <summary>Foreign-key relationship between two datasets.</summary>
    Public Class DatasetRelationship
        ''' <summary>Unique relationship identifier.</summary>
        Public Property Name As String

        ''' <summary>Dataset on the many side.</summary>
        <YamlMember(Alias:="from")>
        Public Property FromDataset As String

        ''' <summary>Dataset on the one side.</summary>
        <YamlMember(Alias:="to")>
        Public Property ToDataset As String

        ''' <summary>Foreign-key fields in the source dataset.</summary>
        <YamlMember(Alias:="from_columns")>
        Public Property FromColumns As List(Of String)

        ''' <summary>Primary or unique key fields in the target dataset.</summary>
        <YamlMember(Alias:="to_columns")>
        Public Property ToColumns As List(Of String)

        ''' <summary>AI context supplied as either text or an <see cref="AiContext"/> object.</summary>
        <YamlMember(Alias:="ai_context")>
        Public Property AiContext As Object

        ''' <summary>Vendor-specific extension data.</summary>
        <YamlMember(Alias:="custom_extensions")>
        Public Property CustomExtensions As List(Of CustomExtension)
    End Class
End Namespace
