Imports System.Xml.Serialization

Namespace FBMMetaModel

    <Serializable()> _
    Public Class RoleConstraintArgument

        Private _Id As String
        <XmlAttribute()> _
        Public Property Id() As String
            Get
                Return Me._Id
            End Get
            Set(ByVal value As String)
                Me._Id = value
            End Set
        End Property

        <DebuggerBrowsable(DebuggerBrowsableState.Never)> _
        Private _SequenceNr As Integer = 1
        <XmlAttribute()> _
        Public Property SequenceNr As Integer
            Get
                Return Me._SequenceNr
            End Get
            Set(value As Integer)
                Me._SequenceNr = value
            End Set
        End Property

        <DebuggerBrowsable(DebuggerBrowsableState.Never)> _
        Private _Role As New List(Of FBMMetaModel.RoleReference)
        Public Property Role As List(Of FBMMetaModel.RoleReference)
            Get
                Return Me._Role
            End Get
            Set(value As List(Of FBMMetaModel.RoleReference))
                Me._Role = value
            End Set
        End Property

        <DebuggerBrowsable(DebuggerBrowsableState.Never)> _
        Private _JoinPath As New FBMMetaModel.JoinPath
        Public Property JoinPath As FBMMetaModel.JoinPath
            Get
                Return Me._JoinPath
            End Get
            Set(value As FBMMetaModel.JoinPath)
                Me._JoinPath = value
            End Set
        End Property

    End Class

End Namespace
