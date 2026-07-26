Imports System.Collections.Generic
Imports YamlDotNet.Serialization

Namespace Ossie
    ''' <summary>Row-level attribute used for grouping, filtering, or calculations.</summary>
    Public Class Field
        ''' <summary>Unique field identifier within the dataset.</summary>
        Public Property Name As String

        ''' <summary>Multi-dialect expression that computes the field.</summary>
        Public Property Expression As Expression

        ''' <summary>Optional dimension metadata.</summary>
        Public Property Dimension As Dimension

        ''' <summary>Label used for categorization.</summary>
        Public Property Label As String

        ''' <summary>Human-readable field description.</summary>
        Public Property Description As String

        ''' <summary>AI context supplied as either text or an <see cref="AiContext"/> object.</summary>
        <YamlMember(Alias:="ai_context")>
        Public Property AiContext As Object

        ''' <summary>Vendor-specific extension data.</summary>
        <YamlMember(Alias:="custom_extensions")>
        Public Property CustomExtensions As List(Of CustomExtension)
    End Class
End Namespace
