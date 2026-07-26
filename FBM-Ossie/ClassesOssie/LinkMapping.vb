Imports System.Collections.Generic
Imports YamlDotNet.Serialization

Namespace Ossie
    ''' <summary>Represents one node in a hierarchy of relationship-link mappings.</summary>
    Public Class LinkMapping
        ''' <summary>Optional relationship populated by this mapping node.</summary>
        Public Property Relationship As String

        ''' <summary>Mapping for the object in this tuple position.</summary>
        <YamlMember(Alias:="object_mapping")>
        Public Property ObjectMapping As ObjectMapping

        ''' <summary>Mappings for subsequent positions in the link tuple.</summary>
        Public Property Children As List(Of LinkMapping)
    End Class
End Namespace
