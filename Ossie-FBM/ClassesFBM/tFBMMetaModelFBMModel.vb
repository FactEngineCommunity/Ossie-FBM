Imports System.Reflection
Imports System.Xml.Serialization

Namespace FBMMetaModel
    <Serializable()>
    Public Class FBMModel

        <XmlAttribute()>
        Public ModelId As String = ""

        <XmlAttribute()>
        Public Name As String = ""

        Public ValueTypes As New List(Of FBMMetaModel.ValueType)
        Public EntityTypes As New List(Of FBMMetaModel.EntityType)
        Public FactTypes As New List(Of FBMMetaModel.FactType)
        Public RoleConstraints As New List(Of FBMMetaModel.RoleConstraint)
        Public ModelNotes As New List(Of FBMMetaModel.ModelNote)
        Public Synonyms As New List(Of FBMMetaModel.Synonym)

        ''' <summary>
        ''' Parameterless Constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            '-------------------
            'Parameterless New
            '-------------------
        End Sub

        Public Function getModelElementById(ByVal asModelElementId As String) As Object

            Try

                Dim lrValueType = Me.ValueTypes.Find(Function(x) x.Id = asModelElementId)
                Dim lrEntityType = Me.EntityTypes.Find(Function(x) x.Id = asModelElementId)
                Dim lrFactType = Me.FactTypes.Find(Function(x) x.Id = asModelElementId)
                Dim lrRoleConstraint = Me.RoleConstraints.Find(Function(x) x.Id = asModelElementId)
                Dim lrModelNote = Me.ModelNotes.Find(Function(x) x.Id = asModelElementId)

                If lrValueType IsNot Nothing Then
                    Return lrValueType
                ElseIf lrEntityType IsNot Nothing Then
                    Return lrEntityType
                ElseIf lrFactType IsNot Nothing Then
                    Return lrFactType
                ElseIf lrRoleConstraint IsNot Nothing Then
                    Return lrRoleConstraint
                ElseIf lrModelNote IsNot Nothing Then
                    Return lrModelNote
                Else
                    Return Nothing
                End If

            Catch ex As Exception
                Dim lsMessage As String
                Dim mb As MethodBase = MethodInfo.GetCurrentMethod()

                lsMessage = "Error: " & mb.ReflectedType.Name & "." & mb.Name
                lsMessage &= vbCrLf & vbCrLf & ex.Message
                Throw New Exception(lsMessage)

                Return Nothing
            End Try

        End Function

    End Class
End Namespace