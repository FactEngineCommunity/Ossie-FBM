Imports System.Xml.Serialization
Imports System.Reflection
Imports System.Threading.Tasks
Imports System.Runtime.CompilerServices

Namespace FBMMetaModel

    ''' <summary>
    ''' v1.3 Adds the GUID field to the ValueType, FactType and RoleConstraint classes.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()>
    Public Class Model

        <XmlAttribute()>
        Public XSDVersionNr As Double = 1.7
        Public FBMModel As New FBMMetaModel.FBMModel
        Public FBMDiagram As New List(Of FBMMetaModel.Page)


        Private Function FactTypeIsReferenceModeFactType(ByVal asXMLFactTypeId As String) As Boolean

            Try
                Dim larFactType = From EntityType In Me.FBMModel.EntityTypes
                                  From RoleConstraint In Me.FBMModel.RoleConstraints
                                  From FactType In Me.FBMModel.FactTypes
                                  From Role In FactType.RoleGroup
                                  Where FactType.Id = asXMLFactTypeId
                                  Where EntityType.ReferenceSchemeRoleConstraintId IsNot Nothing
                                  Where EntityType.ReferenceSchemeRoleConstraintId = RoleConstraint.Id
                                  Where RoleConstraint.IsPreferredUniqueness
                                  Where RoleConstraint.RoleConstraintRoles(0).RoleId = Role.Id
                                  Where RoleConstraint.RoleConstraintRoles.Count = 1
                                  Where FactType.RoleGroup.Count = 2
                                  Select FactType

                Return larFactType.Count > 0

            Catch ex As Exception
                Dim lsMessage As String
                Dim mb As MethodBase = MethodInfo.GetCurrentMethod()

                lsMessage = "Error: " & mb.ReflectedType.Name & "." & mb.Name
                lsMessage &= vbCrLf & vbCrLf & ex.Message
                Throw New Exception(lsMessage)

                Return False
            End Try

        End Function

        Private Function ValueTypeIsReferenceModeValueType(ByVal asXMLValueTypeId As String) As Boolean

            Try
                Dim larValueType = From EntityType In Me.FBMModel.EntityTypes
                                   Where EntityType.ReferenceModeValueTypeId IsNot Nothing
                                   Where EntityType.ReferenceModeValueTypeId = asXMLValueTypeId
                                   Select EntityType.ReferenceModeValueTypeId

                Return larValueType.Count > 0

            Catch ex As Exception
                Dim lsMessage As String
                Dim mb As MethodBase = MethodInfo.GetCurrentMethod()

                lsMessage = "Error: " & mb.ReflectedType.Name & "." & mb.Name
                lsMessage &= vbCrLf & vbCrLf & ex.Message
                Throw New Exception(lsMessage)

                Return False
            End Try

        End Function


    End Class

End Namespace
