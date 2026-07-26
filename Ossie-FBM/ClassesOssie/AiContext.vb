Imports System.Collections.Generic

Namespace Ossie
    ''' <summary>Additional context supplied for AI tools as structured data.</summary>
    Public Class AiContext
        ''' <summary>Instructions for AI tools using the annotated construct.</summary>
        Public Property Instructions As String

        ''' <summary>Alternative names and terms for the construct.</summary>
        Public Property Synonyms As List(Of String)

        ''' <summary>Sample questions or use cases.</summary>
        Public Property Examples As List(Of String)
    End Class
End Namespace
