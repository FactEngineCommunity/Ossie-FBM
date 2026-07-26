Imports System.ComponentModel
Imports System.Text.Json.Serialization
Imports System.Xml.Serialization

Namespace FBMMetaModel

    <Serializable()>
    Public Class ConceptInstance

        <XmlIgnore()>
        <JsonIgnore()>
        <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
        <DebuggerBrowsable(DebuggerBrowsableState.Never)>
        Public _Symbol As String = ""
        <XmlAttribute()>
        <Browsable(False)>
        Public Property Symbol() As String
            Get
                Return Me._Symbol
            End Get
            Set(ByVal value As String)
                Me._Symbol = value
            End Set
        End Property

        '--------------------------------------------------------------------------------------------------------------------------
        'This class is predominantly used to store instance (usage) of a Symbol within a diagram (e.g. FBMDiagram, UseCaseDiagram).
        '  e.g. If an EntityType is used within an ORM Diagram, this class is used to reflect within which diagram and at which coordinate
        '  the EntityType was displayed within the diagram.
        '--------------------------------------------------------------------------------------------------------------------------
        <XmlIgnore()>
        Public ModelId As String = ""

        <XmlAttribute()>
        Public ConceptType As pcenumConceptType

        <XmlIgnore()>
        Public PageId As String = ""

        <XmlAttribute()>
        Public RoleId As String = "NotUsed" 'Only used for ConceptInstances where ConceptType is 'Value' otherwise is set to 'NotUsed'.

        <XmlAttribute()>
        Public X As Integer

        <XmlAttribute()>
        Public Y As Integer

        <XmlAttribute()>
        Public Width As Integer

        <XmlAttribute()>
        Public Height As Integer

        <XmlAttribute()>
        Public Orientation As Integer

        <XmlAttribute()>
        Public Visible As Boolean = False

        <XmlAttribute()>
        Public InstanceNumber As Integer = 1

        Public Sub New()
            '-------------------
            'Parameterless New
            '-------------------
        End Sub

        Public Sub New(ByRef arModel As FBMMetaModel.FBMModel,
                   ByRef arPage As FBMMetaModel.Page,
                   ByVal asSymbol As String,
                   Optional aiInstanceNumber As Integer = 1)

            Me.ModelId = arModel.ModelId
            Me.PageId = arPage.Id
            Me.Symbol = asSymbol
            Me.InstanceNumber = aiInstanceNumber

        End Sub

        Public Sub New(ByRef arModel As FBMMetaModel.FBMModel,
                   ByRef arPage As FBMMetaModel.Page,
                   ByVal asSymbol As String,
                   ByVal aiConceptType As pcenumConceptType,
                   ByVal aiInstanceNumber As Integer,
                   Optional ByVal aiX As Integer = 0,
                   Optional ByVal aiY As Integer = 0)

            Me.ModelId = arModel.ModelId
            Me.PageId = arPage.Id
            Me.Symbol = asSymbol
            Me.ConceptType = aiConceptType
            Me.InstanceNumber = aiInstanceNumber
            Me.X = aiX
            Me.Y = aiY

        End Sub

        Function EqualsBySymbolType(other As FBMMetaModel.ConceptInstance) As Boolean
            ' Your custom comparison logic here

            Return Me.Symbol = other.Symbol And Me.ConceptType.ToString = other.ConceptType.ToString

        End Function

        Public Function EqualsBySymbolRoleId(ByVal other As FBMMetaModel.ConceptInstance) As Boolean

            If Me.Symbol = other.Symbol And Me.RoleId = other.RoleId Then
                Return True
            Else
                Return False
            End If

        End Function

    End Class

End Namespace