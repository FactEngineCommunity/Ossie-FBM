Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports FBM_Ossie.Ossie
Imports FBM = FactEngineForServices.FBM
Imports RDS = FactEngineForServices.RDS

''' <summary>
''' Maps an already-loaded FactEngine Model, including its Relational Data
''' Structure, to an Ossie ontology document.
''' </summary>
Public NotInheritable Class FBMToOssieMapper

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Maps an already-loaded FactEngine Model to an Ossie ontology and
    ''' semantic model.
    ''' </summary>
    ''' <param name="arFBMModel">
    ''' The loaded FactEngine Model. File loading is deliberately outside this
    ''' mapper.
    ''' </param>
    ''' <returns>
    ''' An Ossie ontology document containing the ontology and, when present,
    ''' an ontology mapping whose semantic model is derived from the Model RDS.
    ''' </returns>
    Public Shared Function Map(ByRef arFBMModel As FBM.Model) As OntologyDocument
        If arFBMModel Is Nothing Then
            Throw New ArgumentNullException(NameOf(arFBMModel))
        End If

        Dim lrDocument As New OntologyDocument With {
            .Version = "1.0",
            .Name = TextOrDefault(arFBMModel.Name, "FactEngine model"),
            .Description = CombineDescriptions(
                arFBMModel.ShortDescription,
                arFBMModel.LongDescription),
            .Ontology = New List(Of OntologyComponent)(),
            .OntologyMappings = New List(Of OntologyMap)()
        }
        Dim ldrComponentByModelObjectId As New Dictionary(
            Of String,
            OntologyComponent)(StringComparer.OrdinalIgnoreCase)

        AddValueTypeConcepts(
            arFBMModel,
            lrDocument,
            ldrComponentByModelObjectId)
        AddEntityTypeConcepts(
            arFBMModel,
            lrDocument,
            ldrComponentByModelObjectId)
        AddFactTypeRelationships(
            arFBMModel,
            ldrComponentByModelObjectId)
        ApplyPreferredIdentifiers(
            arFBMModel,
            ldrComponentByModelObjectId)
        AddSemanticModel(
            arFBMModel,
            lrDocument,
            ldrComponentByModelObjectId)

        Return lrDocument
    End Function

    Private Shared Sub AddValueTypeConcepts(
        ByVal arFBMModel As FBM.Model,
        ByVal arDocument As OntologyDocument,
        ByVal adrComponentByModelObjectId As Dictionary(
            Of String,
            OntologyComponent))

        If arFBMModel.ValueType Is Nothing Then
            Return
        End If

        For Each lrValueType As FBM.ValueType In arFBMModel.ValueType.
            Where(Function(arValueType As FBM.ValueType)
                      Return arValueType IsNot Nothing AndAlso
                          Not arValueType.IsMDAModelElement
                  End Function).
            OrderBy(
                Function(arValueType As FBM.ValueType)
                    Return GetOssieName(arValueType)
                End Function,
                StringComparer.OrdinalIgnoreCase)

            Dim larExtends As New List(Of String)()
            AddModelObjectSupertypes(lrValueType, larExtends)

            Dim lsPrimitiveTypeName As String =
                GetPrimitiveTypeName(lrValueType)
            If Not String.IsNullOrWhiteSpace(lsPrimitiveTypeName) AndAlso
                Not larExtends.Contains(
                    lsPrimitiveTypeName,
                    StringComparer.OrdinalIgnoreCase) Then
                larExtends.Add(lsPrimitiveTypeName)
            End If

            AddConceptComponent(
                arDocument,
                adrComponentByModelObjectId,
                lrValueType,
                ConceptType.ValueType,
                larExtends)
        Next
    End Sub

    Private Shared Sub AddEntityTypeConcepts(
        ByVal arFBMModel As FBM.Model,
        ByVal arDocument As OntologyDocument,
        ByVal adrComponentByModelObjectId As Dictionary(
            Of String,
            OntologyComponent))

        If arFBMModel.EntityType Is Nothing Then
            Return
        End If

        For Each lrEntityType As FBM.EntityType In arFBMModel.EntityType.
            Where(Function(arEntityType As FBM.EntityType)
                      Return arEntityType IsNot Nothing AndAlso
                          Not arEntityType.IsMDAModelElement
                  End Function).
            OrderBy(
                Function(arEntityType As FBM.EntityType)
                    Return GetOssieName(arEntityType)
                End Function,
                StringComparer.OrdinalIgnoreCase)

            Dim larExtends As New List(Of String)()
            AddModelObjectSupertypes(lrEntityType, larExtends)

            AddConceptComponent(
                arDocument,
                adrComponentByModelObjectId,
                lrEntityType,
                ConceptType.EntityType,
                larExtends)
        Next
    End Sub

    Private Shared Sub AddConceptComponent(
        ByVal arDocument As OntologyDocument,
        ByVal adrComponentByModelObjectId As Dictionary(
            Of String,
            OntologyComponent),
        ByVal arModelObject As FBM.ModelObject,
        ByVal aiConceptType As ConceptType,
        ByVal aarExtends As List(Of String))

        Dim lrComponent As New OntologyComponent With {
            .Description = CombineDescriptions(
                arModelObject.ShortDescription,
                arModelObject.LongDescription),
            .Concept = New Concept With {
                .Name = GetOssieName(arModelObject),
                .Type = aiConceptType,
                .Description = CombineDescriptions(
                    arModelObject.ShortDescription,
                    arModelObject.LongDescription),
                .Extends = NullIfEmpty(aarExtends),
                .DerivedBy = SplitDerivation(arModelObject)
            },
            .Relationships = New List(Of OntologyRelationship)()
        }

        arDocument.Ontology.Add(lrComponent)
        If Not String.IsNullOrWhiteSpace(arModelObject.Id) Then
            adrComponentByModelObjectId(arModelObject.Id) = lrComponent
        End If
    End Sub

    Private Shared Sub AddModelObjectSupertypes(
        ByVal arModelObject As FBM.ModelObject,
        ByVal aarExtends As List(Of String))

        If arModelObject.parentModelObjectList Is Nothing Then
            Return
        End If

        For Each lrParentModelObject As FBM.ModelObject In
            arModelObject.parentModelObjectList

            If lrParentModelObject Is Nothing OrElse
                Not IsBusinessObjectType(lrParentModelObject) Then
                Continue For
            End If

            Dim lsParentName As String = GetOssieName(lrParentModelObject)
            If Not String.IsNullOrWhiteSpace(lsParentName) AndAlso
                Not aarExtends.Contains(
                    lsParentName,
                    StringComparer.OrdinalIgnoreCase) Then
                aarExtends.Add(lsParentName)
            End If
        Next
    End Sub

    Private Shared Sub AddFactTypeRelationships(
        ByVal arFBMModel As FBM.Model,
        ByVal adrComponentByModelObjectId As Dictionary(
            Of String,
            OntologyComponent))

        If arFBMModel.FactType Is Nothing Then
            Return
        End If

        For Each lrFactType As FBM.FactType In arFBMModel.FactType.
            Where(Function(arFactType As FBM.FactType)
                      Return IsBusinessFactType(arFactType)
                  End Function).
            OrderBy(
                Function(arFactType As FBM.FactType)
                    Return GetRelationshipName(arFactType)
                End Function,
                StringComparer.OrdinalIgnoreCase)

            Dim larOrderedRoles As List(Of FBM.Role) =
                GetOrderedRoles(lrFactType)
            If larOrderedRoles.Count = 0 Then
                Continue For
            End If

            Dim lrOwningModelObject As FBM.ModelObject =
                larOrderedRoles(0).JoinedORMObject
            Dim lrOwningComponent As OntologyComponent = Nothing
            If lrOwningModelObject Is Nothing OrElse
                String.IsNullOrWhiteSpace(lrOwningModelObject.Id) OrElse
                Not adrComponentByModelObjectId.TryGetValue(
                    lrOwningModelObject.Id,
                    lrOwningComponent) Then
                Continue For
            End If

            Dim lrRelationship As New OntologyRelationship With {
                .Name = GetRelationshipName(lrFactType),
                .Description = CombineDescriptions(
                    lrFactType.ShortDescription,
                    lrFactType.LongDescription),
                .Roles = CreateOssieRoles(larOrderedRoles),
                .Multiplicity = GetMultiplicity(larOrderedRoles),
                .DerivedBy = SplitDerivation(lrFactType),
                .Verbalizes = CreateVerbalizations(lrFactType)
            }

            lrOwningComponent.Relationships.Add(lrRelationship)
        Next
    End Sub

    Private Shared Function CreateOssieRoles(
        ByVal aarOrderedRoles As List(Of FBM.Role)) As List(Of Role)

        Dim larRole As New List(Of Role)()
        For liRoleIndex As Integer = 1 To aarOrderedRoles.Count - 1
            Dim lrFBMRole As FBM.Role = aarOrderedRoles(liRoleIndex)
            If lrFBMRole?.JoinedORMObject Is Nothing Then
                Continue For
            End If

            larRole.Add(
                New Role With {
                    .Concept = GetOssieName(lrFBMRole.JoinedORMObject),
                    .Name = NullIfWhiteSpace(lrFBMRole.Name)
                })
        Next

        Return larRole
    End Function

    Private Shared Function GetMultiplicity(
        ByVal aarOrderedRoles As List(Of FBM.Role)) As Multiplicity?

        If aarOrderedRoles.Count <> 2 Then
            Return Nothing
        End If

        Dim lbFirstRoleIsUnique As Boolean =
            HasInternalUniquenessConstraint(aarOrderedRoles(0))
        If Not lbFirstRoleIsUnique Then
            Return Nothing
        End If

        If HasInternalUniquenessConstraint(aarOrderedRoles(1)) Then
            Return Ossie.Multiplicity.OneToOne
        End If

        Return Ossie.Multiplicity.ManyToOne
    End Function

    Private Shared Function HasInternalUniquenessConstraint(
        ByVal arRole As FBM.Role) As Boolean

        If arRole Is Nothing OrElse
            arRole.InternalUniquenessConstraint Is Nothing Then
            Return False
        End If

        Return arRole.InternalUniquenessConstraint.Any(
            Function(arRoleConstraint As FBM.RoleConstraint)
                Return arRoleConstraint IsNot Nothing AndAlso
                    arRoleConstraint.RoleConstraintType.ToString().
                        Equals(
                            "InternalUniquenessConstraint",
                            StringComparison.OrdinalIgnoreCase)
            End Function)
    End Function

    Private Shared Function CreateVerbalizations(
        ByVal arFactType As FBM.FactType) As List(Of String)

        If arFactType.FactTypeReading Is Nothing Then
            Return Nothing
        End If

        Dim larVerbalization As List(Of String) =
            arFactType.FactTypeReading.
                Where(
                    Function(arReading As FBM.FactTypeReading)
                        Return arReading IsNot Nothing
                    End Function).
                OrderByDescending(
                    Function(arReading As FBM.FactTypeReading)
                        Return arReading.IsPreferred
                    End Function).
                Select(
                    Function(arReading As FBM.FactTypeReading)
                        Return FormatReading(arReading)
                    End Function).
                Where(
                    Function(asReading As String)
                        Return Not String.IsNullOrWhiteSpace(asReading)
                    End Function).
                Distinct(StringComparer.Ordinal).
                ToList()

        Return NullIfEmpty(larVerbalization)
    End Function

    Private Shared Function FormatReading(
        ByVal arReading As FBM.FactTypeReading) As String

        Dim lrText As New StringBuilder()
        AppendReadingText(
            lrText,
            arReading.FrontText)

        If arReading.PredicatePart IsNot Nothing Then
            For Each lrPredicatePart As FBM.PredicatePart In
                arReading.PredicatePart.
                    Where(
                        Function(arPart As FBM.PredicatePart)
                            Return arPart IsNot Nothing
                        End Function).
                    OrderBy(
                        Function(arPart As FBM.PredicatePart)
                            Return arPart.SequenceNr
                        End Function)

                AppendReadingText(
                    lrText,
                    lrPredicatePart.PreBoundText)
                AppendReadingText(
                    lrText,
                    "{" &
                    If(
                        lrPredicatePart.Role?.JoinedORMObject Is Nothing,
                        String.Empty,
                        GetOssieName(
                            lrPredicatePart.Role.JoinedORMObject)) &
                    "}")
                AppendReadingText(
                    lrText,
                    lrPredicatePart.PostBoundText)
                AppendReadingText(
                    lrText,
                    lrPredicatePart.PredicatePartText)
            Next
        End If

        AppendReadingText(
            lrText,
            arReading.FollowingText)
        Return lrText.ToString().Trim()
    End Function

    ''' <summary>
    ''' Appends one reading component with a word boundary where FEFS stores
    ''' adjacent components without their display whitespace. A trailing
    ''' hyphen remains bound to the following role placeholder because hyphens
    ''' carry predicate significance in Ossie.
    ''' </summary>
    Private Shared Sub AppendReadingText(
        ByVal arText As StringBuilder,
        ByVal asTextPart As String)

        If arText Is Nothing OrElse
            String.IsNullOrEmpty(asTextPart) Then
            Return
        End If

        Dim lsTextPart As String = asTextPart
        If arText.Length > 0 AndAlso
            Not Char.IsWhiteSpace(arText(arText.Length - 1)) AndAlso
            Not Char.IsWhiteSpace(lsTextPart(0)) AndAlso
            arText(arText.Length - 1) <> "-"c AndAlso
            Not StartsWithClosingPunctuation(lsTextPart) Then
            arText.Append(" "c)
        End If

        arText.Append(lsTextPart)
    End Sub

    Private Shared Function StartsWithClosingPunctuation(
        ByVal asText As String) As Boolean

        If String.IsNullOrEmpty(asText) Then
            Return False
        End If

        Return ",.;:!?)]}".IndexOf(asText(0)) >= 0
    End Function

    Private Shared Sub ApplyPreferredIdentifiers(
        ByVal arFBMModel As FBM.Model,
        ByVal adrComponentByModelObjectId As Dictionary(
            Of String,
            OntologyComponent))

        If arFBMModel.EntityType Is Nothing Then
            Return
        End If

        For Each lrEntityType As FBM.EntityType In arFBMModel.EntityType
            If lrEntityType Is Nothing OrElse
                lrEntityType.IsMDAModelElement OrElse
                String.IsNullOrWhiteSpace(lrEntityType.Id) Then
                Continue For
            End If

            Dim lrComponent As OntologyComponent = Nothing
            If Not adrComponentByModelObjectId.TryGetValue(
                lrEntityType.Id,
                lrComponent) Then
                Continue For
            End If

            If lrEntityType.IsObjectifyingEntityType Then
                Dim lrObjectifiedFactType As FBM.FactType =
                    GetObjectifiedFactType(lrEntityType, arFBMModel)
                Dim larLinkFactType As List(Of FBM.FactType) =
                    GetOrderedLinkFactTypes(lrObjectifiedFactType)

                If larLinkFactType.Count > 0 Then
                    lrComponent.Concept.IdentifyBy =
                        larLinkFactType.
                            Select(
                                Function(arLinkFactType As FBM.FactType)
                                    Return GetRelationshipName(
                                        arLinkFactType)
                                End Function).
                            Distinct(StringComparer.OrdinalIgnoreCase).
                            ToList()
                    Continue For
                End If
            End If

            Dim lrPreferredIdentifier As FBM.RoleConstraint =
                GetPreferredIdentifier(lrEntityType, arFBMModel)
            If lrPreferredIdentifier Is Nothing OrElse
                lrPreferredIdentifier.RoleConstraintRole Is Nothing Then
                Continue For
            End If

            Dim larRelationshipName As List(Of String) =
                lrPreferredIdentifier.RoleConstraintRole.
                    Where(
                        Function(arConstraintRole As FBM.RoleConstraintRole)
                            Return arConstraintRole?.Role?.FactType IsNot Nothing
                        End Function).
                    OrderBy(
                        Function(arConstraintRole As FBM.RoleConstraintRole)
                            Return arConstraintRole.SequenceNr
                        End Function).
                    Select(
                        Function(arConstraintRole As FBM.RoleConstraintRole)
                            Return GetRelationshipName(
                                arConstraintRole.Role.FactType)
                        End Function).
                    Distinct(StringComparer.OrdinalIgnoreCase).
                    ToList()

            lrComponent.Concept.IdentifyBy =
                NullIfEmpty(larRelationshipName)
        Next
    End Sub

    Private Shared Function GetObjectifiedFactType(
        ByVal arObjectifyingEntityType As FBM.EntityType,
        ByVal arFBMModel As FBM.Model) As FBM.FactType

        If arObjectifyingEntityType Is Nothing Then
            Return Nothing
        End If

        If arObjectifyingEntityType.ObjectifiedFactType IsNot Nothing Then
            Return arObjectifyingEntityType.ObjectifiedFactType
        End If

        If arFBMModel?.FactType Is Nothing Then
            Return Nothing
        End If

        Return arFBMModel.FactType.FirstOrDefault(
            Function(arFactType As FBM.FactType)
                Return arFactType IsNot Nothing AndAlso
                    arFactType.ObjectifyingEntityType Is
                        arObjectifyingEntityType
            End Function)
    End Function

    Private Shared Function GetOrderedLinkFactTypes(
        ByVal arObjectifiedFactType As FBM.FactType) _
        As List(Of FBM.FactType)

        If arObjectifiedFactType Is Nothing Then
            Return New List(Of FBM.FactType)()
        End If

        Dim larLinkFactType As List(Of FBM.FactType) =
            arObjectifiedFactType.getLinkFactTypes()
        If larLinkFactType Is Nothing Then
            Return New List(Of FBM.FactType)()
        End If

        Return larLinkFactType.
            Where(
                Function(arLinkFactType As FBM.FactType)
                    Return arLinkFactType IsNot Nothing AndAlso
                        Not arLinkFactType.IsMDAModelElement AndAlso
                        arLinkFactType.LinkFactTypeRole IsNot Nothing
                End Function).
            OrderBy(
                Function(arLinkFactType As FBM.FactType)
                    Return arLinkFactType.LinkFactTypeRole.SequenceNr
                End Function).
            ToList()
    End Function

    Private Shared Function GetPreferredIdentifier(
        ByVal arEntityType As FBM.EntityType,
        ByVal arFBMModel As FBM.Model) As FBM.RoleConstraint

        If arEntityType.ReferenceModeRoleConstraint IsNot Nothing Then
            Return arEntityType.ReferenceModeRoleConstraint
        End If

        If arFBMModel.RoleConstraint Is Nothing Then
            Return Nothing
        End If

        Return arFBMModel.RoleConstraint.FirstOrDefault(
            Function(arRoleConstraint As FBM.RoleConstraint)
                If arRoleConstraint Is Nothing OrElse
                    Not arRoleConstraint.IsPreferredIdentifier OrElse
                    arRoleConstraint.RoleConstraintRole Is Nothing Then
                    Return False
                End If

                Return arRoleConstraint.RoleConstraintRole.Any(
                    Function(arConstraintRole As FBM.RoleConstraintRole)
                        Dim lrFactType As FBM.FactType =
                            arConstraintRole?.Role?.FactType
                        If lrFactType Is Nothing Then
                            Return False
                        End If

                        Return GetOrderedRoles(lrFactType).Any(
                            Function(arRole As FBM.Role)
                                Return arRole.JoinedORMObject Is arEntityType
                            End Function)
                    End Function)
            End Function)
    End Function

    Private Shared Sub AddSemanticModel(
        ByVal arFBMModel As FBM.Model,
        ByVal arDocument As OntologyDocument,
        ByVal adrComponentByModelObjectId As Dictionary(
            Of String,
            OntologyComponent))

        If arFBMModel.RDS Is Nothing OrElse
            arFBMModel.RDS.Table Is Nothing OrElse
            arFBMModel.RDS.Table.Count = 0 Then
            Return
        End If

        Dim larTable As List(Of RDS.Table) =
            arFBMModel.RDS.Table.
                Where(
                    Function(arTable As RDS.Table)
                        Return arTable IsNot Nothing AndAlso
                            Not arTable.IsSystemTable AndAlso
                            Not arTable.isAbsorbed
                    End Function).
                OrderBy(
                    Function(arTable As RDS.Table)
                        Return GetTableName(arTable)
                    End Function,
                    StringComparer.OrdinalIgnoreCase).
                ToList()

        If larTable.Count = 0 Then
            Return
        End If

        Dim lrSemanticModel As New SemanticModel With {
            .Name = TextOrDefault(
                arFBMModel.Name,
                "FactEngine model") & " semantic model",
            .Description = CombineDescriptions(
                arFBMModel.ShortDescription,
                arFBMModel.LongDescription),
            .Datasets = New List(Of Dataset)(),
            .Relationships = New List(Of DatasetRelationship)()
        }
        Dim ldrDatasetNameByTable As New Dictionary(
            Of RDS.Table,
            String)()

        For Each lrTable As RDS.Table In larTable
            Dim lrDataset As Dataset =
                CreateDataset(arFBMModel, lrTable)
            lrSemanticModel.Datasets.Add(lrDataset)
            ldrDatasetNameByTable(lrTable) = lrDataset.Name
        Next

        AddDatasetRelationships(
            arFBMModel.RDS,
            lrSemanticModel,
            ldrDatasetNameByTable)

        Dim lrOntologyMap As New OntologyMap With {
            .Name = ToIdentifier(
                TextOrDefault(
                    arFBMModel.Name,
                    "FactEngine model")) & "_mapping",
            .Description =
                "Mapping generated from the FactEngine Relational Data Structure.",
            .SemanticModel = lrSemanticModel,
            .ConceptMappings = CreateConceptMappings(
                larTable,
                ldrDatasetNameByTable,
                adrComponentByModelObjectId)
        }

        arDocument.OntologyMappings.Add(lrOntologyMap)
    End Sub

    Private Shared Function CreateDataset(
        ByVal arFBMModel As FBM.Model,
        ByVal arTable As RDS.Table) As Dataset

        Dim lrDataset As New Dataset With {
            .Name = GetTableName(arTable),
            .Source = GetTableSource(arFBMModel, arTable),
            .Description = GetTableDescription(arTable),
            .Fields = New List(Of Field)(),
            .UniqueKeys = New List(Of List(Of String))()
        }

        If arTable.Column IsNot Nothing Then
            For Each lrColumn As RDS.Column In arTable.Column.
                Where(
                    Function(arColumn As RDS.Column)
                        Return arColumn IsNot Nothing
                    End Function).
                OrderBy(
                    Function(arColumn As RDS.Column)
                        Return arColumn.OrdinalPosition
                    End Function)

                lrDataset.Fields.Add(CreateField(lrColumn))
            Next
        End If

        If arTable.Index IsNot Nothing Then
            For Each lrIndex As RDS.Index In arTable.Index.
                Where(
                    Function(arIndex As RDS.Index)
                        Return arIndex IsNot Nothing AndAlso
                            arIndex.Column IsNot Nothing AndAlso
                            arIndex.Column.Count > 0
                    End Function)

                Dim larColumnName As List(Of String) =
                    lrIndex.Column.
                        Where(
                            Function(arColumn As RDS.Column)
                                Return arColumn IsNot Nothing
                            End Function).
                        Select(
                            Function(arColumn As RDS.Column)
                                Return GetColumnName(arColumn)
                            End Function).
                        ToList()

                If lrIndex.IsPrimaryKey Then
                    lrDataset.PrimaryKey = larColumnName
                ElseIf lrIndex.Unique OrElse Not lrIndex.NonUnique Then
                    lrDataset.UniqueKeys.Add(larColumnName)
                End If
            Next
        End If

        lrDataset.UniqueKeys = NullIfEmpty(lrDataset.UniqueKeys)
        Return lrDataset
    End Function

    Private Shared Function CreateField(
        ByVal arColumn As RDS.Column) As Field

        Return New Field With {
            .Name = GetColumnName(arColumn),
            .Expression = New Expression With {
                .Dialects = New List(Of DialectExpression) From {
                    New DialectExpression With {
                        .Dialect = Dialect.ANSI_SQL,
                        .Expression = GetColumnName(arColumn)
                    }
                }
            },
            .Label = NullIfWhiteSpace(arColumn.Name),
            .Description = GetColumnDescription(arColumn)
        }
    End Function

    Private Shared Sub AddDatasetRelationships(
        ByVal arRDSModel As RDS.Model,
        ByVal arSemanticModel As SemanticModel,
        ByVal adrDatasetNameByTable As Dictionary(
            Of RDS.Table,
            String))

        If arRDSModel.Relation Is Nothing Then
            Return
        End If

        Dim ldrRelationshipNameCount As New Dictionary(
            Of String,
            Integer)(StringComparer.OrdinalIgnoreCase)

        For Each lrRelation As RDS.Relation In arRDSModel.Relation
            If lrRelation Is Nothing OrElse
                lrRelation.OriginTable Is Nothing OrElse
                lrRelation.DestinationTable Is Nothing OrElse
                Not adrDatasetNameByTable.ContainsKey(
                    lrRelation.OriginTable) OrElse
                Not adrDatasetNameByTable.ContainsKey(
                    lrRelation.DestinationTable) Then
                Continue For
            End If

            Dim lsBaseName As String = FirstNonBlank(
                lrRelation.Label,
                If(
                    lrRelation.ResponsibleFactType Is Nothing,
                    Nothing,
                    GetRelationshipName(
                        lrRelation.ResponsibleFactType)),
                adrDatasetNameByTable(lrRelation.OriginTable) &
                    "_to_" &
                    adrDatasetNameByTable(lrRelation.DestinationTable))
            Dim lsRelationshipName As String =
                CreateUniqueName(
                    lsBaseName,
                    ldrRelationshipNameCount)

            arSemanticModel.Relationships.Add(
                New DatasetRelationship With {
                    .Name = lsRelationshipName,
                    .FromDataset =
                        adrDatasetNameByTable(lrRelation.OriginTable),
                    .ToDataset =
                        adrDatasetNameByTable(lrRelation.DestinationTable),
                    .FromColumns = GetColumnNames(
                        lrRelation.OriginColumns),
                    .ToColumns = GetColumnNames(
                        lrRelation.DestinationColumns)
                })
        Next

        arSemanticModel.Relationships =
            NullIfEmpty(arSemanticModel.Relationships)
    End Sub

    Private Shared Function CreateConceptMappings(
        ByVal aarTable As List(Of RDS.Table),
        ByVal adrDatasetNameByTable As Dictionary(
            Of RDS.Table,
            String),
        ByVal adrComponentByModelObjectId As Dictionary(
            Of String,
            OntologyComponent)) As List(Of ConceptMapping)

        Dim larConceptMapping As New List(Of ConceptMapping)()

        For Each lrTable As RDS.Table In aarTable
            Dim lrModelObject As FBM.ModelObject =
                lrTable.FBMModelElement
            If lrModelObject Is Nothing OrElse
                String.IsNullOrWhiteSpace(lrModelObject.Id) OrElse
                Not adrComponentByModelObjectId.ContainsKey(
                    lrModelObject.Id) Then
                Continue For
            End If

            Dim lsDatasetName As String =
                adrDatasetNameByTable(lrTable)
            Dim lrConceptMapping As New ConceptMapping With {
                .Concept = GetOssieName(lrModelObject),
                .ObjectMappings = CreateObjectMappings(
                    lrTable,
                    lrModelObject,
                    lsDatasetName),
                .LinkMappings = CreateLinkMappings(
                    lrTable,
                    lrModelObject,
                    lsDatasetName)
            }

            If lrConceptMapping.ObjectMappings IsNot Nothing OrElse
                lrConceptMapping.LinkMappings IsNot Nothing Then
                larConceptMapping.Add(lrConceptMapping)
            End If
        Next

        Return NullIfEmpty(larConceptMapping)
    End Function

    Private Shared Function CreateObjectMappings(
        ByVal arTable As RDS.Table,
        ByVal arTableModelObject As FBM.ModelObject,
        ByVal asDatasetName As String) As List(Of ObjectMapping)

        Dim larPrimaryKeyColumn As List(Of RDS.Column) =
            GetPrimaryKeyColumns(arTable)
        If larPrimaryKeyColumn.Count = 0 Then
            Return Nothing
        End If

        Dim larReferentMapping As New List(Of ReferentMapping)()
        For Each lrColumn As RDS.Column In larPrimaryKeyColumn
            Dim lrReferentFactType As FBM.FactType =
                GetReferentFactType(
                    arTableModelObject,
                    lrColumn)
            If lrReferentFactType Is Nothing Then
                Continue For
            End If

            larReferentMapping.Add(
                New ReferentMapping With {
                    .Relationship =
                        GetRelationshipName(lrReferentFactType),
                    .Expression =
                        GetQualifiedColumnExpression(
                            asDatasetName,
                            lrColumn)
                })
        Next

        If larReferentMapping.Count = 0 AndAlso
            larPrimaryKeyColumn.Count = 1 Then
            Return New List(Of ObjectMapping) From {
                New ObjectMapping With {
                    .Expression =
                        GetQualifiedColumnExpression(
                            asDatasetName,
                            larPrimaryKeyColumn(0))
                }
            }
        End If

        If larReferentMapping.Count = 0 Then
            Return Nothing
        End If

        Return New List(Of ObjectMapping) From {
            New ObjectMapping With {
                .ReferentMappings = larReferentMapping
            }
        }
    End Function

    Private Shared Function GetReferentFactType(
        ByVal arTableModelObject As FBM.ModelObject,
        ByVal arColumn As RDS.Column) As FBM.FactType

        If arColumn Is Nothing Then
            Return Nothing
        End If

        Dim lrObjectifiedFactType As FBM.FactType =
            TryCast(arTableModelObject, FBM.FactType)
        If lrObjectifiedFactType Is Nothing Then
            Return arColumn.FactType
        End If

        Dim lrColumnRole As FBM.Role = arColumn.Role
        If lrColumnRole Is Nothing Then
            lrColumnRole = arColumn.ActiveRole
        End If

        If lrColumnRole Is Nothing Then
            Return arColumn.FactType
        End If

        Dim lrLinkFactType As FBM.FactType =
            GetOrderedLinkFactTypes(lrObjectifiedFactType).
                FirstOrDefault(
                    Function(arLinkFactType As FBM.FactType)
                        Return RolesAreEqual(
                            arLinkFactType.LinkFactTypeRole,
                            lrColumnRole)
                    End Function)

        Return If(lrLinkFactType, arColumn.FactType)
    End Function

    Private Shared Function RolesAreEqual(
        ByVal arFirstRole As FBM.Role,
        ByVal arSecondRole As FBM.Role) As Boolean

        If arFirstRole Is arSecondRole Then
            Return True
        End If

        Return arFirstRole IsNot Nothing AndAlso
            arSecondRole IsNot Nothing AndAlso
            Not String.IsNullOrWhiteSpace(arFirstRole.Id) AndAlso
            arFirstRole.Id.Equals(
                arSecondRole.Id,
                StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function CreateLinkMappings(
        ByVal arTable As RDS.Table,
        ByVal arTableModelObject As FBM.ModelObject,
        ByVal asDatasetName As String) As List(Of LinkMapping)

        If arTable.Column Is Nothing Then
            Return Nothing
        End If

        Dim larPrimaryKeyColumn As List(Of RDS.Column) =
            GetPrimaryKeyColumns(arTable)
        Dim larSourceObjectMappings As List(Of ObjectMapping) =
            CreateObjectMappings(
                arTable,
                arTableModelObject,
                asDatasetName)
        Dim lrSourceObjectMapping As ObjectMapping =
            larSourceObjectMappings?.FirstOrDefault()
        Dim larChildLinkMapping As New List(Of LinkMapping)()

        For Each lrColumn As RDS.Column In arTable.Column.
            Where(
                Function(arColumn As RDS.Column)
                    Return arColumn IsNot Nothing AndAlso
                        Not larPrimaryKeyColumn.Contains(arColumn) AndAlso
                        arColumn.FactType IsNot Nothing
                End Function).
            OrderBy(
                Function(arColumn As RDS.Column)
                    Return arColumn.OrdinalPosition
                End Function)

            Dim lrTargetModelObject As FBM.ModelObject =
                GetRelationshipTarget(
                    lrColumn.FactType,
                    arTableModelObject)
            If lrTargetModelObject Is Nothing Then
                Continue For
            End If

            larChildLinkMapping.Add(
                New LinkMapping With {
                    .Relationship =
                        GetRelationshipName(lrColumn.FactType),
                    .ObjectMapping = New ObjectMapping With {
                        .Concept = GetOssieName(lrTargetModelObject),
                        .Expression =
                            GetQualifiedColumnExpression(
                                asDatasetName,
                                lrColumn)
                    }
                })
        Next

        If larChildLinkMapping.Count = 0 Then
            Return Nothing
        End If

        If lrSourceObjectMapping Is Nothing Then
            Return larChildLinkMapping
        End If

        Return New List(Of LinkMapping) From {
            New LinkMapping With {
                .ObjectMapping = lrSourceObjectMapping,
                .Children = larChildLinkMapping
            }
        }
    End Function

    Private Shared Function GetRelationshipTarget(
        ByVal arFactType As FBM.FactType,
        ByVal arSourceModelObject As FBM.ModelObject) As FBM.ModelObject

        Dim larRole As List(Of FBM.Role) =
            GetOrderedRoles(arFactType)
        If larRole.Count < 2 Then
            Return Nothing
        End If

        Dim liSourceRoleIndex As Integer =
            larRole.FindIndex(
                Function(arRole As FBM.Role)
                    Return arRole.JoinedORMObject Is arSourceModelObject
                End Function)
        If liSourceRoleIndex < 0 Then
            Return Nothing
        End If

        Return larRole.
            Where(
                Function(arRole As FBM.Role, aiRoleIndex As Integer)
                    Return aiRoleIndex <> liSourceRoleIndex AndAlso
                        arRole.JoinedORMObject IsNot Nothing
                End Function).
            Select(
                Function(arRole As FBM.Role)
                    Return arRole.JoinedORMObject
                End Function).
            FirstOrDefault()
    End Function

    Private Shared Function GetPrimaryKeyColumns(
        ByVal arTable As RDS.Table) As List(Of RDS.Column)

        If arTable.Index Is Nothing Then
            Return New List(Of RDS.Column)()
        End If

        Dim lrPrimaryKeyIndex As RDS.Index =
            arTable.Index.FirstOrDefault(
                Function(arIndex As RDS.Index)
                    Return arIndex IsNot Nothing AndAlso
                        arIndex.IsPrimaryKey AndAlso
                        arIndex.Column IsNot Nothing
                End Function)
        If lrPrimaryKeyIndex Is Nothing Then
            Return New List(Of RDS.Column)()
        End If

        Return lrPrimaryKeyIndex.Column.
            Where(
                Function(arColumn As RDS.Column)
                    Return arColumn IsNot Nothing
                End Function).
            ToList()
    End Function

    Private Shared Function GetTableDescription(
        ByVal arTable As RDS.Table) As String

        If arTable.FBMModelElement IsNot Nothing Then
            Dim lsDescription As String = CombineDescriptions(
                arTable.FBMModelElement.ShortDescription,
                arTable.FBMModelElement.LongDescription)
            If Not String.IsNullOrWhiteSpace(lsDescription) Then
                Return lsDescription
            End If
        End If

        Return FirstNonBlank(arTable.Remarks, arTable.DerivationRule)
    End Function

    Private Shared Function GetColumnDescription(
        ByVal arColumn As RDS.Column) As String

        If arColumn.FactType IsNot Nothing Then
            Dim lsDescription As String = CombineDescriptions(
                arColumn.FactType.ShortDescription,
                arColumn.FactType.LongDescription)
            If Not String.IsNullOrWhiteSpace(lsDescription) Then
                Return lsDescription
            End If
        End If

        Return NullIfWhiteSpace(arColumn.Remarks)
    End Function

    Private Shared Function GetTableSource(
        ByVal arFBMModel As FBM.Model,
        ByVal arTable As RDS.Table) As String

        Dim larSourcePart As New List(Of String)()
        AddNonBlank(larSourcePart, arFBMModel.Database)
        AddNonBlank(larSourcePart, arFBMModel.Schema)
        AddNonBlank(
            larSourcePart,
            FirstNonBlank(
                arTable.DatabaseName,
                arTable.DBName,
                arTable.Name))

        Return String.Join(".", larSourcePart)
    End Function

    Private Shared Function GetTableName(
        ByVal arTable As RDS.Table) As String

        Return FirstNonBlank(
            arTable.Name,
            arTable.DBName,
            arTable.DatabaseName,
            "Dataset")
    End Function

    Private Shared Function GetColumnName(
        ByVal arColumn As RDS.Column) As String

        Return FirstNonBlank(
            arColumn.Name,
            arColumn.DBName,
            arColumn.DatabaseName,
            "Field")
    End Function

    Private Shared Function GetColumnNames(
        ByVal aarColumn As List(Of RDS.Column)) As List(Of String)

        If aarColumn Is Nothing Then
            Return Nothing
        End If

        Return NullIfEmpty(
            aarColumn.
                Where(
                    Function(arColumn As RDS.Column)
                        Return arColumn IsNot Nothing
                    End Function).
                Select(
                    Function(arColumn As RDS.Column)
                        Return GetColumnName(arColumn)
                    End Function).
                ToList())
    End Function

    Private Shared Function GetQualifiedColumnExpression(
        ByVal asDatasetName As String,
        ByVal arColumn As RDS.Column) As String

        Return asDatasetName & "." & GetColumnName(arColumn)
    End Function

    Private Shared Function GetOssieName(
        ByVal arModelObject As FBM.ModelObject) As String

        If arModelObject Is Nothing Then
            Return String.Empty
        End If

        If arModelObject.Alias IsNot Nothing Then
            Dim lrOssieAlias As FBM.Alias =
                arModelObject.Alias.FirstOrDefault(
                    Function(arAlias As FBM.Alias)
                        Return arAlias IsNot Nothing AndAlso
                            arAlias.AliasType.ToString().
                                Equals(
                                    "Ossie",
                                    StringComparison.OrdinalIgnoreCase) AndAlso
                            Not String.IsNullOrWhiteSpace(
                                arAlias.Alias)
                    End Function)
            If lrOssieAlias IsNot Nothing Then
                Return lrOssieAlias.Alias.Trim()
            End If
        End If

        Return TextOrDefault(arModelObject.Name, arModelObject.Id)
    End Function

    Private Shared Function GetRelationshipName(
        ByVal arFactType As FBM.FactType) As String

        Return GetOssieName(arFactType)
    End Function

    Private Shared Function GetOrderedRoles(
        ByVal arFactType As FBM.FactType) As List(Of FBM.Role)

        If arFactType?.RoleGroup Is Nothing Then
            Return New List(Of FBM.Role)()
        End If

        Return arFactType.RoleGroup.
            Where(
                Function(arRole As FBM.Role)
                    Return arRole IsNot Nothing
                End Function).
            OrderBy(
                Function(arRole As FBM.Role)
                    Return arRole.SequenceNr
                End Function).
            ToList()
    End Function

    Private Shared Function IsBusinessFactType(
        ByVal arFactType As FBM.FactType) As Boolean

        Return arFactType IsNot Nothing AndAlso
            Not arFactType.IsMDAModelElement AndAlso
            Not arFactType.IsCoreFactType AndAlso
            Not arFactType.IsSubtypeRelationshipFactType
    End Function

    Private Shared Function IsBusinessObjectType(
        ByVal arModelObject As FBM.ModelObject) As Boolean

        If TypeOf arModelObject Is FBM.EntityType Then
            Return Not DirectCast(
                arModelObject,
                FBM.EntityType).IsMDAModelElement
        End If

        If TypeOf arModelObject Is FBM.ValueType Then
            Return Not DirectCast(
                arModelObject,
                FBM.ValueType).IsMDAModelElement
        End If

        Return False
    End Function

    Private Shared Function GetPrimitiveTypeName(
        ByVal arValueType As FBM.ValueType) As String

        Select Case arValueType.DataType.ToString()
            Case "TrueOrFalse", "Boolean"
                Return "Boolean"
            Case "SignedSmallInteger", "UnsignedSmallInteger",
                 "SignedInteger", "UnsignedInteger"
                Return "Integer"
            Case "SignedLargeInteger", "UnsignedLargeInteger"
                Return "Long"
            Case "Decimal", "Money"
                Return "Decimal"
            Case "FloatingPoint", "DoublePrecisionFloatingPoint"
                Return "Double"
            Case "Date"
                Return "Date"
            Case "Time"
                Return "Time"
            Case "DateAndTime", "DateTime"
                Return "DateTime"
            Case "VariableLengthText", "FixedLengthText",
                 "LargeLengthText", "Character", "String"
                Return "String"
            Case "AutoCounter"
                Return "Integer"
            Case Else
                Return Nothing
        End Select
    End Function

    Private Shared Function SplitDerivation(
        ByVal arModelObject As FBM.ModelObject) As List(Of String)

        If arModelObject Is Nothing OrElse
            Not arModelObject.IsDerived OrElse
            String.IsNullOrWhiteSpace(arModelObject.DerivationText) Then
            Return Nothing
        End If

        Return NullIfEmpty(
            arModelObject.DerivationText.
                Split(
                    {vbCrLf, vbLf, vbCr},
                    StringSplitOptions.RemoveEmptyEntries).
                Select(
                    Function(asLine As String)
                        Return asLine.Trim()
                    End Function).
                Where(
                    Function(asLine As String)
                        Return asLine.Length > 0
                    End Function).
                ToList())
    End Function

    Private Shared Function CombineDescriptions(
        ByVal asShortDescription As String,
        ByVal asLongDescription As String) As String

        Dim lsShortDescription As String =
            NullIfWhiteSpace(asShortDescription)
        Dim lsLongDescription As String =
            NullIfWhiteSpace(asLongDescription)

        If String.IsNullOrWhiteSpace(lsShortDescription) Then
            Return lsLongDescription
        End If
        If String.IsNullOrWhiteSpace(lsLongDescription) OrElse
            lsShortDescription.Equals(
                lsLongDescription,
                StringComparison.Ordinal) Then
            Return lsShortDescription
        End If

        Return lsShortDescription &
            Environment.NewLine &
            Environment.NewLine &
            lsLongDescription
    End Function

    Private Shared Function FirstNonBlank(
        ParamArray aarValue As String()) As String

        If aarValue Is Nothing Then
            Return Nothing
        End If

        For Each lsValue As String In aarValue
            If Not String.IsNullOrWhiteSpace(lsValue) Then
                Return lsValue.Trim()
            End If
        Next

        Return Nothing
    End Function

    Private Shared Function TextOrDefault(
        ByVal asValue As String,
        ByVal asDefaultValue As String) As String

        Return If(
            String.IsNullOrWhiteSpace(asValue),
            asDefaultValue,
            asValue.Trim())
    End Function

    Private Shared Function NullIfWhiteSpace(
        ByVal asValue As String) As String

        Return If(
            String.IsNullOrWhiteSpace(asValue),
            Nothing,
            asValue.Trim())
    End Function

    Private Shared Function NullIfEmpty(Of T)(
        ByVal aarValue As List(Of T)) As List(Of T)

        Return If(
            aarValue Is Nothing OrElse aarValue.Count = 0,
            Nothing,
            aarValue)
    End Function

    Private Shared Sub AddNonBlank(
        ByVal aarValue As List(Of String),
        ByVal asValue As String)

        If Not String.IsNullOrWhiteSpace(asValue) Then
            aarValue.Add(asValue.Trim())
        End If
    End Sub

    Private Shared Function ToIdentifier(
        ByVal asValue As String) As String

        If String.IsNullOrWhiteSpace(asValue) Then
            Return "factengine"
        End If

        Dim lrResult As New StringBuilder()
        Dim lbLastCharacterWasSeparator As Boolean = False
        For Each lcCharacter As Char In asValue.Trim()
            If Char.IsLetterOrDigit(lcCharacter) Then
                lrResult.Append(Char.ToLowerInvariant(lcCharacter))
                lbLastCharacterWasSeparator = False
            ElseIf Not lbLastCharacterWasSeparator Then
                lrResult.Append("_"c)
                lbLastCharacterWasSeparator = True
            End If
        Next

        Return lrResult.ToString().Trim("_"c)
    End Function

    Private Shared Function CreateUniqueName(
        ByVal asBaseName As String,
        ByVal adrNameCount As Dictionary(
            Of String,
            Integer)) As String

        Dim lsBaseName As String =
            TextOrDefault(asBaseName, "relationship")
        Dim liNameCount As Integer = 0
        If Not adrNameCount.TryGetValue(
            lsBaseName,
            liNameCount) Then
            adrNameCount(lsBaseName) = 1
            Return lsBaseName
        End If

        liNameCount += 1
        adrNameCount(lsBaseName) = liNameCount
        Return lsBaseName & "_" & liNameCount.ToString()
    End Function
End Class
