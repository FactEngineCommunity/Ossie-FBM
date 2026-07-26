Imports System.Collections.Generic
Imports YamlDotNet.Serialization

Namespace Ossie
    ''' <summary>Defines a relationship between ontology concepts.</summary>
    Public Class OntologyRelationship
        ''' <summary>Name of the relationship within its containing concept.</summary>
        Public Property Name As String

        ''' <summary>Human-readable relationship description.</summary>
        Public Property Description As String

        ''' <summary>Additional roles after the implicit first role.</summary>
        Public Property Roles As List(Of Role)

        ''' <summary>Functional multiplicity constraint for the relationship.</summary>
        Public Property Multiplicity As Multiplicity?

        ''' <summary>Expressions that derive relationship links.</summary>
        <YamlMember(Alias:="derived_by")>
        Public Property DerivedBy As List(Of String)

        ''' <summary>Expressions that constrain relationship links.</summary>
        Public Property Requires As List(Of String)

        ''' <summary>Natural-language patterns that verbalize the relationship.</summary>
        Public Property Verbalizes As List(Of String)
    End Class
End Namespace
