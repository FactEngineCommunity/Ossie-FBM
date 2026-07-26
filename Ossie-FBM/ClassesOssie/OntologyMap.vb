Imports System.Collections.Generic
Imports YamlDotNet.Serialization

Namespace Ossie
    ''' <summary>Maps a logical semantic model into an ontology.</summary>
    Public Class OntologyMap
        ''' <summary>Optional name of this ontology map.</summary>
        Public Property Name As String

        ''' <summary>Human-readable map description.</summary>
        Public Property Description As String

        ''' <summary>Logical semantic model supplying the mapped data.</summary>
        <YamlMember(Alias:="semantic_model")>
        Public Property SemanticModel As SemanticModel

        ''' <summary>Mappings for concepts and their relationships.</summary>
        <YamlMember(Alias:="concept_mappings")>
        Public Property ConceptMappings As List(Of ConceptMapping)
    End Class
End Namespace
