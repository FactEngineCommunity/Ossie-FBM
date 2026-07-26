Imports System.Xml.Serialization

Namespace FBMMetaModel

    <Serializable()>
    Public Class JoinPath

        ''' <summary>
        ''' The set of Roles traversed in order to form the JoinPath.
        ''' </summary>
        ''' <remarks></remarks>
        <DebuggerBrowsable(DebuggerBrowsableState.Never)>
        Private _RolePath As New List(Of FBMMetaModel.RoleReference)
        Public Property RolePath As List(Of FBMMetaModel.RoleReference)
            Get
                Return Me._RolePath
            End Get
            Set(value As List(Of FBMMetaModel.RoleReference))
                Me._RolePath = value
            End Set
        End Property

    End Class

End Namespace
