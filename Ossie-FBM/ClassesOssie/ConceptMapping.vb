Imports System.Collections.Generic
Imports YamlDotNet.Serialization

Namespace Ossie
    ''' <summary>Maps logical constructs to one ontology concept and its relationships.</summary>
    Public Class ConceptMapping
        ''' <summary>Name of the ontology concept being populated.</summary>
        Public Property Concept As String

        ''' <summary>Mappings that populate objects of the concept.</summary>
        <YamlMember(Alias:="object_mappings")>
        Public Property ObjectMappings As List(Of ObjectMapping)

        ''' <summary>Mappings that populate relationships grouped by the concept.</summary>
        <YamlMember(Alias:="link_mappings")>
        Public Property LinkMappings As List(Of LinkMapping)
    End Class
End Namespace
