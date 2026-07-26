Imports System.Collections.Generic
Imports YamlDotNet.Serialization

Namespace Ossie
    ''' <summary>Quantitative measure defined over business data.</summary>
    Public Class Metric
        ''' <summary>Unique metric identifier.</summary>
        Public Property Name As String

        ''' <summary>Multi-dialect expression that computes the metric.</summary>
        Public Property Expression As Expression

        ''' <summary>Human-readable metric description.</summary>
        Public Property Description As String

        ''' <summary>AI context supplied as either text or an <see cref="AiContext"/> object.</summary>
        <YamlMember(Alias:="ai_context")>
        Public Property AiContext As Object

        ''' <summary>Vendor-specific extension data.</summary>
        <YamlMember(Alias:="custom_extensions")>
        Public Property CustomExtensions As List(Of CustomExtension)
    End Class
End Namespace
