Imports System.Collections.Generic
Imports YamlDotNet.Serialization

Namespace Ossie
    ''' <summary>Defines a business concept in an ontology.</summary>
    Public Class Concept
        ''' <summary>Unique identifier for the concept.</summary>
        Public Property Name As String

        ''' <summary>Whether this is an entity type or value type.</summary>
        Public Property Type As ConceptType

        ''' <summary>Human-readable concept description.</summary>
        Public Property Description As String

        ''' <summary>Names of this concept's supertypes.</summary>
        Public Property Extends As List(Of String)

        ''' <summary>Expressions that derive this concept's population.</summary>
        <YamlMember(Alias:="derived_by")>
        Public Property DerivedBy As List(Of String)

        ''' <summary>Relationship names forming this concept's preferred identifier.</summary>
        <YamlMember(Alias:="identify_by")>
        Public Property IdentifyBy As List(Of String)

        ''' <summary>Expressions that constrain this concept's population.</summary>
        Public Property Requires As List(Of String)
    End Class
End Namespace
