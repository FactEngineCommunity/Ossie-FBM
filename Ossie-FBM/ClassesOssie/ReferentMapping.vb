Imports System.Collections.Generic
Imports YamlDotNet.Serialization

Namespace Ossie
    ''' <summary>Maps a logical value through an identifying relationship.</summary>
    Public Class ReferentMapping
        ''' <summary>Name of the identifying relationship used for the reference.</summary>
        Public Property Relationship As String

        ''' <summary>ANSI SQL expression that supplies the referenced value.</summary>
        Public Property Expression As String

        ''' <summary>Nested mappings used when the referenced object is an entity.</summary>
        <YamlMember(Alias:="referent_mappings")>
        Public Property ReferentMappings As List(Of ReferentMapping)
    End Class
End Namespace
