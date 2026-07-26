Imports System.Collections.Generic

Namespace Ossie
    ''' <summary>Groups one concept with its primary relationships.</summary>
    Public Class OntologyComponent
        ''' <summary>Human-readable description of this component.</summary>
        Public Property Description As String

        ''' <summary>Concept defined by this component.</summary>
        Public Property Concept As Concept

        ''' <summary>Relationships primarily associated with the concept.</summary>
        Public Property Relationships As List(Of OntologyRelationship)
    End Class
End Namespace
