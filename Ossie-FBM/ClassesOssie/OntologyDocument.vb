Imports System.Collections.Generic
Imports YamlDotNet.Serialization
Imports YamlDotNet.Serialization.NamingConventions

Namespace Ossie
    ''' <summary>Root document for an Apache Ossie ontology definition.</summary>
    Public Class OntologyDocument
        Inherits OssieDocument

        ''' <summary>Unique identifier for this ontology.</summary>
        Public Property Name As String

        ''' <summary>Human-readable description of the ontology.</summary>
        Public Property Description As String

        ''' <summary>AI context supplied as either text or an <see cref="AiContext"/> object.</summary>
        <YamlMember(Alias:="ai_context")>
        Public Property AiContext As Object

        ''' <summary>Expressions that constrain the ontology population.</summary>
        Public Property Requires As List(Of String)

        ''' <summary>Concepts and their primary relationships.</summary>
        Public Property Ontology As List(Of OntologyComponent)

        ''' <summary>Mappings from logical semantic models into this ontology.</summary>
        <YamlMember(Alias:="ontology_mappings")>
        Public Property OntologyMappings As List(Of OntologyMap)
    End Class
End Namespace
