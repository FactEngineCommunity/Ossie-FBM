Imports System.Collections.Generic
Imports YamlDotNet.Serialization

Namespace Ossie
    ''' <summary>Maps logical expressions to objects of an ontology concept.</summary>
    Public Class ObjectMapping
        ''' <summary>Optional target concept name when it is not implied by context.</summary>
        Public Property Concept As String

        ''' <summary>Mappings through identifying relationships for an entity object.</summary>
        <YamlMember(Alias:="referent_mappings")>
        Public Property ReferentMappings As List(Of ReferentMapping)

        ''' <summary>ANSI SQL expression that supplies the mapped value.</summary>
        Public Property Expression As String
    End Class
End Namespace
