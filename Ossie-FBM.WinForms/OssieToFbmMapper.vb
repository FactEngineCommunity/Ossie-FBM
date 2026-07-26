Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions
Imports FBM_Ossie.Ossie
Imports FBM = FactEngineForServices.FBM

''' <summary>
''' Builds an in-memory FactEngine fact-based model from a deserialized Ossie document.
''' </summary>
Public NotInheritable Class OssieToFbmMapper

    Private Shared ReadOnly PlaceholderPattern As New Regex(
        "\{([^{}]+)\}",
        RegexOptions.Compiled Or RegexOptions.CultureInvariant)
    Private Shared ReadOnly PrimitiveValueTypeNames As New HashSet(Of String)(
        {
            "Boolean",
            "Byte",
            "Date",
            "DateTime",
            "Decimal",
            "Double",
            "Float",
            "Integer",
            "Long",
            "Number",
            "Short",
            "String",
            "Time"
        },
        StringComparer.OrdinalIgnoreCase)

    Private Sub New()
    End Sub

    Public Shared Function Map(document As OssieDocument) As FBM.Model
        If document Is Nothing Then
            Throw New ArgumentNullException(NameOf(document))
        End If

        If TypeOf document Is OntologyDocument Then
            Return MapOntology(DirectCast(document, OntologyDocument))
        End If

        If TypeOf document Is SemanticModelDocument Then
            Return MapSemanticModelDocument(
                DirectCast(document, SemanticModelDocument))
        End If

        Throw New NotSupportedException(
            $"Unsupported Ossie document type: {document.GetType().Name}")
    End Function

    Private Shared Function MapOntology(document As OntologyDocument) As FBM.Model
        Dim modelName = TextOrDefault(document.Name, "Imported Ossie ontology")
        Dim model = CreateModel(modelName)
        Dim objectTypes As New Dictionary(Of String, FBM.ModelObject)(
            StringComparer.OrdinalIgnoreCase)

        ' Fact types can only bind their roles after every object type exists.
        AddOntologyValueTypes(document, model, objectTypes)
        AddOntologyEntityTypes(document, model, objectTypes)
        AddOntologyFactTypes(document, model, objectTypes)
        ApplyCompoundPreferredIdentifiers(document, model)

        Return model
    End Function

    Private Shared Sub AddOntologyValueTypes(
        document As OntologyDocument,
        model As FBM.Model,
        objectTypes As Dictionary(Of String, FBM.ModelObject))

        If document.Ontology Is Nothing Then
            Return
        End If

        Dim ldrValueTypeConcepts As Dictionary(Of String, Concept) =
            document.Ontology.
                Where(
                    Function(arComponent)
                        Return arComponent?.Concept IsNot Nothing AndAlso
                            arComponent.Concept.Type = ConceptType.ValueType AndAlso
                            Not String.IsNullOrWhiteSpace(
                                arComponent.Concept.Name)
                    End Function).
                Select(
                    Function(arComponent)
                        Return arComponent.Concept
                    End Function).
                ToDictionary(
                    Function(arConcept)
                        Return arConcept.Name
                    End Function,
                    StringComparer.OrdinalIgnoreCase)

        For Each component In document.Ontology
            Dim concept = component?.Concept
            If concept Is Nothing OrElse
                concept.Type <> ConceptType.ValueType OrElse
                String.IsNullOrWhiteSpace(concept.Name) Then
                Continue For
            End If

            Dim valueType = AddValueType(
                model,
                objectTypes,
                concept.Name,
                concept.Name,
                concept.Description,
                ResolveOntologyValueTypeDataType(
                    concept,
                    ldrValueTypeConcepts,
                    New HashSet(Of String)(
                        StringComparer.OrdinalIgnoreCase)))
            ApplyDerivation(valueType, concept.DerivedBy)
        Next
    End Sub

    ''' <summary>
    ''' Resolves an Ossie Value Type's FBM datatype through its inheritance
    ''' chain.
    ''' </summary>
    ''' <param name="arConcept">The Ossie Value Type concept.</param>
    ''' <param name="adrValueTypeConcepts">
    ''' The ontology's Value Types indexed by name.
    ''' </param>
    ''' <param name="arVisitedConceptNames">
    ''' Value Type names already visited while resolving inheritance.
    ''' </param>
    ''' <returns>The corresponding FactEngine datatype.</returns>
    Private Shared Function ResolveOntologyValueTypeDataType(
        arConcept As Concept,
        adrValueTypeConcepts As Dictionary(Of String, Concept),
        arVisitedConceptNames As HashSet(Of String)) As FactEngineForServices.publicFBMConstants.pcenumORMDataType

        If arConcept Is Nothing OrElse
            String.IsNullOrWhiteSpace(arConcept.Name) OrElse
            Not arVisitedConceptNames.Add(arConcept.Name) Then
            Return FactEngineForServices.publicFBMConstants.
                pcenumORMDataType.DataTypeNotSet
        End If

        Dim liORMDataType As FactEngineForServices.publicFBMConstants.pcenumORMDataType =
            InferDataType(arConcept.Name)
        If liORMDataType <>
            FactEngineForServices.publicFBMConstants.
                pcenumORMDataType.DataTypeNotSet Then
            Return liORMDataType
        End If

        If arConcept.Extends Is Nothing Then
            Return FactEngineForServices.publicFBMConstants.
                pcenumORMDataType.DataTypeNotSet
        End If

        For Each lsSupertypeName As String In arConcept.Extends
            liORMDataType = InferDataType(lsSupertypeName)
            If liORMDataType <>
                FactEngineForServices.publicFBMConstants.
                    pcenumORMDataType.DataTypeNotSet Then
                Return liORMDataType
            End If

            Dim lrSupertypeConcept As Concept = Nothing
            If adrValueTypeConcepts.TryGetValue(
                lsSupertypeName,
                lrSupertypeConcept) Then
                liORMDataType =
                    ResolveOntologyValueTypeDataType(
                        lrSupertypeConcept,
                        adrValueTypeConcepts,
                        arVisitedConceptNames)
                If liORMDataType <>
                    FactEngineForServices.publicFBMConstants.
                        pcenumORMDataType.DataTypeNotSet Then
                    Return liORMDataType
                End If
            End If
        Next

        Return FactEngineForServices.publicFBMConstants.
            pcenumORMDataType.DataTypeNotSet
    End Function

    ''' <summary>
    ''' Applies compound preferred identifiers after every ontology Fact Type
    ''' and Role has been added to the Model.
    ''' </summary>
    ''' <param name="arDocument">The Ossie ontology document.</param>
    ''' <param name="arModel">The FBM Model being populated.</param>
    Private Shared Sub ApplyCompoundPreferredIdentifiers(
        arDocument As OntologyDocument,
        arModel As FBM.Model)

        If arDocument.Ontology Is Nothing Then
            Return
        End If

        For Each lrComponent As OntologyComponent In arDocument.Ontology
            Dim lrConcept As Concept = lrComponent?.Concept
            If lrConcept Is Nothing OrElse
                lrConcept.Type <> ConceptType.EntityType OrElse
                lrConcept.IdentifyBy Is Nothing OrElse
                lrConcept.IdentifyBy.Count < 2 OrElse
                lrComponent.Relationships Is Nothing Then
                Continue For
            End If

            Dim larIdentifierRoles As New List(Of FBM.Role)
            Dim lbIsValidCompoundReferenceScheme As Boolean = True

            For Each lsRelationshipName As String In lrConcept.IdentifyBy
                Dim lrRelationship As OntologyRelationship =
                    lrComponent.Relationships.FirstOrDefault(
                        Function(arRelationship)
                            Return arRelationship IsNot Nothing AndAlso
                                String.Equals(
                                    arRelationship.Name,
                                    lsRelationshipName,
                                    StringComparison.OrdinalIgnoreCase)
                        End Function)
                If lrRelationship Is Nothing Then
                    lbIsValidCompoundReferenceScheme = False
                    Exit For
                End If

                Dim lsFactTypeAlias As String =
                    $"{lrConcept.Name}.{lrRelationship.Name}"
                Dim lrFactType As FBM.FactType =
                    arModel.FactType.FirstOrDefault(
                        Function(arFactType)
                            Return arFactType.Alias.Any(
                                Function(arAlias)
                                    Return String.Equals(
                                        arAlias.Alias,
                                        lsFactTypeAlias,
                                        StringComparison.OrdinalIgnoreCase)
                                End Function)
                        End Function)
                If lrFactType Is Nothing OrElse
                    lrFactType.RoleGroup.Count <> 2 Then
                    lbIsValidCompoundReferenceScheme = False
                    Exit For
                End If

                Dim larIdentifierRolesForFactType As List(Of FBM.Role) =
                    lrFactType.RoleGroup.
                        Where(
                            Function(arRole)
                                Return RoleCanIdentifyCompoundEntityType(
                                    arRole)
                            End Function).
                        ToList()
                If larIdentifierRolesForFactType.Count <> 1 Then
                    lbIsValidCompoundReferenceScheme = False
                    Exit For
                End If

                larIdentifierRoles.Add(larIdentifierRolesForFactType(0))
            Next

            If Not lbIsValidCompoundReferenceScheme OrElse
                larIdentifierRoles.Count <> lrConcept.IdentifyBy.Count Then
                Continue For
            End If

            Dim lrEntityType As FBM.EntityType =
                arModel.EntityType.FirstOrDefault(
                    Function(arEntityType)
                        Return String.Equals(
                            arEntityType.Name,
                            lrConcept.Name,
                            StringComparison.OrdinalIgnoreCase)
                    End Function)
            If lrEntityType Is Nothing Then
                Continue For
            End If

            Dim lrExternalUniquenessConstraint As FBM.RoleConstraint =
                CreateExternalUniquenessConstraint(
                    lrEntityType,
                    larIdentifierRoles)
            Dim lrExistingReferenceModeRoleConstraint As FBM.RoleConstraint =
                lrEntityType.ReferenceModeRoleConstraint

            lrEntityType.ReferenceModeRoleConstraint =
                lrExternalUniquenessConstraint
            lrEntityType.PreferredIdentifierRCId =
                lrExternalUniquenessConstraint.Id
            lrExternalUniquenessConstraint.SetIsPreferredIdentifier(
                True,
                True,
                lrExistingReferenceModeRoleConstraint)
        Next
    End Sub

    ''' <summary>
    ''' Tests whether a Role can contribute to a compound reference scheme.
    ''' </summary>
    ''' <param name="arRole">The candidate identifying Role.</param>
    ''' <returns>
    ''' True when the Role joins a Value Type or an Entity Type that has a
    ''' completed simple reference scheme; otherwise False.
    ''' </returns>
    Private Shared Function RoleCanIdentifyCompoundEntityType(
        arRole As FBM.Role) As Boolean

        If TypeOf arRole.JoinedORMObject Is FBM.ValueType Then
            Return True
        End If

        If Not TypeOf arRole.JoinedORMObject Is FBM.EntityType Then
            Return False
        End If

        Dim lrJoinedEntityType As FBM.EntityType =
            DirectCast(
                arRole.JoinedORMObject,
                FBM.EntityType)

        Return lrJoinedEntityType.ReferenceModeFactType IsNot Nothing AndAlso
            lrJoinedEntityType.ReferenceModeValueType IsNot Nothing AndAlso
            lrJoinedEntityType.ReferenceModeRoleConstraint IsNot Nothing AndAlso
            lrJoinedEntityType.ReferenceModeRoleConstraint.RoleConstraintType =
                FactEngineForServices.publicConstants.
                    pcenumRoleConstraintType.InternalUniquenessConstraint
    End Function

    ''' <summary>
    ''' Creates an External Uniqueness Constraint over Roles from multiple
    ''' binary Fact Types and adds it to the Model.
    ''' </summary>
    ''' <param name="arEntityType">
    ''' The Entity Type identified by the constrained Roles.
    ''' </param>
    ''' <param name="aarIdentifierRoles">
    ''' The identifying Roles in preferred-identifier order.
    ''' </param>
    ''' <returns>The new External Uniqueness Constraint.</returns>
    Private Shared Function CreateExternalUniquenessConstraint(
        arEntityType As FBM.EntityType,
        aarIdentifierRoles As List(Of FBM.Role)) As FBM.RoleConstraint

        Dim lsRoleConstraintId As String =
            $"{arEntityType.Id}.ExternalUniquenessConstraint"
        Dim lrRoleConstraint As New FBM.RoleConstraint With {
            .Model = arEntityType.Model,
            .Id = lsRoleConstraintId,
            .Name = lsRoleConstraintId,
            .ConceptType =
                FactEngineForServices.publicConstants.
                    pcenumConceptType.RoleConstraint,
            .RoleConstraintType =
                FactEngineForServices.publicConstants.
                    pcenumRoleConstraintType.ExternalUniquenessConstraint
        }

        For liRoleIndex As Integer = 0 To aarIdentifierRoles.Count - 1
            Dim lrRole As FBM.Role = aarIdentifierRoles(liRoleIndex)
            Dim lrRoleConstraintRole As New FBM.RoleConstraintRole With {
                .Model = arEntityType.Model,
                .RoleConstraint = lrRoleConstraint,
                .Role = lrRole,
                .SequenceNr = liRoleIndex + 1
            }

            lrRoleConstraint.RoleConstraintRole.Add(lrRoleConstraintRole)
            lrRoleConstraint.Role.Add(lrRole)
            lrRole.RoleConstraintRole.Add(lrRoleConstraintRole)
        Next

        arEntityType.Model.AddRoleConstraint(
            lrRoleConstraint,
            abMakeModelDirty:=False,
            abBroadcastInterfaceEvent:=False,
            abIgnoreRDSProcessing:=True)

        Return lrRoleConstraint
    End Function

    Private Shared Sub AddOntologyEntityTypes(
        document As OntologyDocument,
        model As FBM.Model,
        objectTypes As Dictionary(Of String, FBM.ModelObject))

        If document.Ontology Is Nothing Then
            Return
        End If

        For Each component In document.Ontology
            Dim concept = component?.Concept
            If concept Is Nothing OrElse
                concept.Type <> ConceptType.EntityType OrElse
                String.IsNullOrWhiteSpace(concept.Name) Then
                Continue For
            End If

            Dim entityType = AddEntityType(
                model,
                objectTypes,
                concept.Name,
                concept.Name,
                concept.Description)
            ApplyDerivation(entityType, concept.DerivedBy)
        Next
    End Sub

    Private Shared Sub AddOntologyFactTypes(
        document As OntologyDocument,
        model As FBM.Model,
        objectTypes As Dictionary(Of String, FBM.ModelObject))

        If document.Ontology Is Nothing Then
            Return
        End If

        For Each component In document.Ontology
            Dim conceptName = component?.Concept?.Name
            If String.IsNullOrWhiteSpace(conceptName) OrElse
                component.Relationships Is Nothing Then
                Continue For
            End If

            For Each relationship In component.Relationships
                If relationship Is Nothing Then
                    Continue For
                End If

                Dim roleObjectNames As New List(Of String) From {conceptName}
                Dim roleNames As New List(Of String) From {String.Empty}

                If relationship.Roles IsNot Nothing Then
                    For Each role In relationship.Roles
                        roleObjectNames.Add(role?.Concept)
                        roleNames.Add(If(role?.Name, String.Empty))
                    Next
                End If

                Dim fallbackFactTypeName =
                    $"{conceptName}.{TextOrDefault(relationship.Name, "Relationship")}"
                Dim firstVerbalization = If(
                    relationship.Verbalizes?.FirstOrDefault(
                        Function(value) Not String.IsNullOrWhiteSpace(value)),
                    String.Empty)
                Dim factType = AddFactType(
                    model,
                    objectTypes,
                    CreateFactTypeRootName(
                        firstVerbalization,
                        fallbackFactTypeName),
                    fallbackFactTypeName,
                    relationship.Description,
                    roleObjectNames,
                    roleNames)
                ApplyDerivation(factType, relationship.DerivedBy)

                If relationship.Verbalizes IsNot Nothing Then
                    For Each verbalization In relationship.Verbalizes
                        AddFactTypeReading(factType, verbalization)
                    Next
                End If

                ApplyRelationshipMultiplicity(
                    relationship,
                    factType)
                ApplySingleRelationshipPreferredIdentifier(
                    component.Concept,
                    relationship,
                    factType)
            Next
        Next
    End Sub

    ''' <summary>
    ''' Creates the Internal Uniqueness Constraint implied by an Ossie
    ''' many-to-one relationship.
    ''' </summary>
    ''' <param name="arRelationship">The Ossie relationship.</param>
    ''' <param name="arFactType">
    ''' The FBM Fact Type created for the relationship.
    ''' </param>
    Private Shared Sub ApplyRelationshipMultiplicity(
        arRelationship As OntologyRelationship,
        arFactType As FBM.FactType)

        If arRelationship Is Nothing OrElse
            arRelationship.Multiplicity Is Nothing OrElse
            arRelationship.Multiplicity.Value <> Multiplicity.ManyToOne OrElse
            arFactType Is Nothing OrElse
            arFactType.RoleGroup.Count = 0 Then
            Return
        End If

        ' The implicit first Role is the many side of an Ossie many-to-one
        ' relationship and is therefore constrained to be unique.
        CreateSingleRoleInternalUniquenessConstraint(
            arFactType,
            arFactType.RoleGroup(0))
    End Sub

    ''' <summary>
    ''' Applies a simple reference scheme when an Entity Type is identified by
    ''' one relationship to a Value Type.
    ''' </summary>
    ''' <param name="arConcept">The Ossie Entity Type concept.</param>
    ''' <param name="arRelationship">
    ''' The relationship currently being mapped.
    ''' </param>
    ''' <param name="arFactType">
    ''' The FBM Fact Type created for the relationship.
    ''' </param>
    Private Shared Sub ApplySingleRelationshipPreferredIdentifier(
        arConcept As Concept,
        arRelationship As OntologyRelationship,
        arFactType As FBM.FactType)

        If arConcept Is Nothing OrElse
            arConcept.Type <> ConceptType.EntityType OrElse
            arConcept.IdentifyBy Is Nothing OrElse
            arConcept.IdentifyBy.Count <> 1 OrElse
            arRelationship Is Nothing OrElse
            arFactType Is Nothing Then
            Return
        End If

        Dim lsIdentifierRelationshipName As String = arConcept.IdentifyBy(0)
        If String.IsNullOrWhiteSpace(lsIdentifierRelationshipName) OrElse
            Not String.Equals(
                lsIdentifierRelationshipName,
                arRelationship.Name,
                StringComparison.OrdinalIgnoreCase) Then
            Return
        End If

        ' Compound and non-Value-Type identifiers require External Uniqueness
        ' Constraints and are intentionally left for later processing.
        If arFactType.RoleGroup.Count <> 2 OrElse
            Not TypeOf arFactType.RoleGroup(0).JoinedORMObject Is FBM.EntityType OrElse
            Not TypeOf arFactType.RoleGroup(1).JoinedORMObject Is FBM.ValueType Then
            Return
        End If

        Dim lrEntityType As FBM.EntityType =
            DirectCast(
                arFactType.RoleGroup(0).JoinedORMObject,
                FBM.EntityType)
        Dim lrValueType As FBM.ValueType =
            DirectCast(
                arFactType.RoleGroup(1).JoinedORMObject,
                FBM.ValueType)

        CreateSingleRoleInternalUniquenessConstraint(
            arFactType,
            arFactType.RoleGroup(0))
        Dim lrPreferredIdentifier As FBM.RoleConstraint =
            CreateSingleRoleInternalUniquenessConstraint(
                arFactType,
                arFactType.RoleGroup(1))

        Dim lrExistingReferenceModeRoleConstraint As FBM.RoleConstraint =
            lrEntityType.ReferenceModeRoleConstraint
        lrEntityType.ReferenceModeRoleConstraint = lrPreferredIdentifier
        lrEntityType.PreferredIdentifierRCId = lrPreferredIdentifier.Id
        lrPreferredIdentifier.SetIsPreferredIdentifier(
            True,
            True,
            lrExistingReferenceModeRoleConstraint)
        lrEntityType.SetReferenceMode(
            lrValueType.Name,
            abSimpleAssignment:=True,
            abBroadcastInterfaceEvent:=False,
            abSuppressModelSave:=True)
    End Sub

    ''' <summary>
    ''' Creates an Internal Uniqueness Constraint over one Fact Type Role.
    ''' </summary>
    ''' <param name="arFactType">The Fact Type that owns the Role.</param>
    ''' <param name="arRole">The Role constrained to be unique.</param>
    ''' <returns>The new Internal Uniqueness Constraint.</returns>
    Private Shared Function CreateSingleRoleInternalUniquenessConstraint(
        arFactType As FBM.FactType,
        arRole As FBM.Role) As FBM.RoleConstraint

        Dim liNextInternalUniquenessConstraintLevel As Integer =
            arFactType.HighestInternalUniquenessConstraintLevel + 1
        Dim lrRoleConstraint As New FBM.RoleConstraint With {
            .Model = arFactType.Model,
            .Id = $"{arRole.Id}.InternalUniquenessConstraint",
            .Name = $"{arRole.Id}.InternalUniquenessConstraint",
            .LevelNr = liNextInternalUniquenessConstraintLevel,
            .ConceptType =
                FactEngineForServices.publicConstants.
                    pcenumConceptType.RoleConstraint,
            .RoleConstraintType =
                FactEngineForServices.publicConstants.
                    pcenumRoleConstraintType.InternalUniquenessConstraint
        }
        Dim lrRoleConstraintRole As New FBM.RoleConstraintRole With {
            .Model = arFactType.Model,
            .RoleConstraint = lrRoleConstraint,
            .Role = arRole,
            .SequenceNr = 1
        }

        lrRoleConstraint.RoleConstraintRole.Add(lrRoleConstraintRole)
        lrRoleConstraint.Role.Add(arRole)
        arRole.RoleConstraintRole.Add(lrRoleConstraintRole)

        arFactType.AddInternalUniquenessConstraint(lrRoleConstraint)
        arFactType.Model.AddRoleConstraint(
            lrRoleConstraint,
            abMakeModelDirty:=False,
            abBroadcastInterfaceEvent:=False,
            abIgnoreRDSProcessing:=True)

        Return lrRoleConstraint
    End Function

    Private Shared Function MapSemanticModelDocument(
        document As SemanticModelDocument) As FBM.Model

        Dim semanticModels = If(
            document.SemanticModel,
            New List(Of SemanticModel)())
        Dim modelName = If(
            semanticModels.Count = 1,
            TextOrDefault(semanticModels(0)?.Name, "Imported Ossie semantic model"),
            "Imported Ossie semantic models")
        Dim model = CreateModel(modelName)
        Dim objectTypes As New Dictionary(Of String, FBM.ModelObject)(
            StringComparer.OrdinalIgnoreCase)
        Dim datasets As New Dictionary(Of String, FBM.EntityType)(
            StringComparer.OrdinalIgnoreCase)

        ' Stage 1: fields become reusable value types.
        For Each semanticModel In semanticModels
            If semanticModel?.Datasets Is Nothing Then
                Continue For
            End If

            For Each dataset In semanticModel.Datasets
                If dataset?.Fields Is Nothing Then
                    Continue For
                End If

                For Each field In dataset.Fields
                    If field Is Nothing OrElse
                        String.IsNullOrWhiteSpace(field.Name) Then
                        Continue For
                    End If

                    AddValueType(
                        model,
                        objectTypes,
                        field.Name,
                        field.Name,
                        field.Description)
                Next
            Next
        Next

        ' Stage 2: datasets become entity types.
        For Each semanticModel In semanticModels
            If semanticModel?.Datasets Is Nothing Then
                Continue For
            End If

            For Each dataset In semanticModel.Datasets
                If dataset Is Nothing OrElse
                    String.IsNullOrWhiteSpace(dataset.Name) Then
                    Continue For
                End If

                Dim entityType = AddEntityType(
                    model,
                    objectTypes,
                    dataset.Name,
                    dataset.Name,
                    dataset.Description)
                datasets(dataset.Name) = entityType
            Next
        Next

        ' Stage 3: dataset fields and dataset relationships become fact types.
        For Each semanticModel In semanticModels
            If semanticModel?.Datasets IsNot Nothing Then
                For Each dataset In semanticModel.Datasets
                    AddDatasetFieldFactTypes(dataset, model, objectTypes)
                Next
            End If

            If semanticModel?.Relationships IsNot Nothing Then
                For Each relationship In semanticModel.Relationships
                    AddDatasetRelationshipFactType(
                        relationship,
                        model,
                        objectTypes)
                Next
            End If
        Next

        Return model
    End Function

    Private Shared Sub AddDatasetFieldFactTypes(
        dataset As Dataset,
        model As FBM.Model,
        objectTypes As Dictionary(Of String, FBM.ModelObject))

        If dataset Is Nothing OrElse
            String.IsNullOrWhiteSpace(dataset.Name) OrElse
            dataset.Fields Is Nothing Then
            Return
        End If

        For Each field In dataset.Fields
            If field Is Nothing OrElse String.IsNullOrWhiteSpace(field.Name) Then
                Continue For
            End If

            Dim verbalization = $"{{{dataset.Name}}} has {{{field.Name}}}"
            Dim ossieFactTypeName = $"{dataset.Name}.{field.Name}"
            Dim factType = AddFactType(
                model,
                objectTypes,
                CreateFactTypeRootName(
                    verbalization,
                    ossieFactTypeName),
                ossieFactTypeName,
                field.Description,
                New List(Of String) From {dataset.Name, field.Name},
                New List(Of String) From {String.Empty, String.Empty})

            AddFactTypeReading(factType, verbalization)
        Next
    End Sub

    Private Shared Sub AddDatasetRelationshipFactType(
        relationship As DatasetRelationship,
        model As FBM.Model,
        objectTypes As Dictionary(Of String, FBM.ModelObject))

        If relationship Is Nothing OrElse
            String.IsNullOrWhiteSpace(relationship.FromDataset) OrElse
            String.IsNullOrWhiteSpace(relationship.ToDataset) Then
            Return
        End If

        Dim relationshipName = TextOrDefault(
            relationship.Name,
            $"{relationship.FromDataset} to {relationship.ToDataset}")
        Dim ossieFactTypeName =
            $"{relationship.FromDataset}.{relationshipName}"
        Dim verbalization =
            $"{{{relationship.FromDataset}}} relates to {{{relationship.ToDataset}}}"
        Dim factType = AddFactType(
            model,
            objectTypes,
            CreateFactTypeRootName(verbalization, relationshipName),
            ossieFactTypeName,
            Nothing,
            New List(Of String) From {
                relationship.FromDataset,
                relationship.ToDataset
            },
            New List(Of String) From {String.Empty, String.Empty})

        AddFactTypeReading(factType, verbalization)
    End Sub

    Private Shared Function CreateModel(modelName As String) As FBM.Model
        Dim modelId = $"Ossie.{modelName}"
        Dim model As New FBM.Model(modelName, modelId) With {
            .Loaded = True,
            .StoreAsXML = False,
            .IsConceptualModel = True
        }
        Return model
    End Function

    Private Shared Function AddValueType(
        model As FBM.Model,
        objectTypes As Dictionary(Of String, FBM.ModelObject),
        lookupName As String,
        valueTypeName As String,
        description As String,
        Optional aiORMDataType As FactEngineForServices.publicFBMConstants.pcenumORMDataType =
            FactEngineForServices.publicFBMConstants.pcenumORMDataType.DataTypeNotSet) As FBM.ValueType

        Dim existing As FBM.ModelObject = Nothing
        If objectTypes.TryGetValue(lookupName, existing) AndAlso
            TypeOf existing Is FBM.ValueType Then
            Return DirectCast(existing, FBM.ValueType)
        End If

        If aiORMDataType =
            FactEngineForServices.publicFBMConstants.
                pcenumORMDataType.DataTypeNotSet Then
            aiORMDataType = InferDataType(valueTypeName)
        End If

        Dim valueType As New FBM.ValueType With {
            .Model = model,
            .Id = lookupName,
            .Name = valueTypeName,
            .DataType = aiORMDataType,
            .ShortDescription = description,
            .LongDescription = description
        }
        model.AddValueType(
            valueType,
            abMakeModelDirty:=False,
            abBroadcastInterfaceEvent:=False)
        objectTypes(lookupName) = valueType
        Return valueType
    End Function

    Private Shared Function AddEntityType(
        model As FBM.Model,
        objectTypes As Dictionary(Of String, FBM.ModelObject),
        lookupName As String,
        entityTypeName As String,
        description As String) As FBM.EntityType

        Dim existing As FBM.ModelObject = Nothing
        If objectTypes.TryGetValue(lookupName, existing) AndAlso
            TypeOf existing Is FBM.EntityType Then
            Return DirectCast(existing, FBM.EntityType)
        End If

        Dim entityType As New FBM.EntityType With {
            .Model = model,
            .Id = lookupName,
            .Name = entityTypeName,
            .ShortDescription = description,
            .LongDescription = description
        }
        model.AddEntityType(
            entityType,
            abMakeModelDirty:=False,
            abBroadcastInterfaceEvent:=False,
            abIgnoreRDSProcessing:=True)
        objectTypes(lookupName) = entityType
        Return entityType
    End Function

    Private Shared Function AddFactType(
        model As FBM.Model,
        objectTypes As Dictionary(Of String, FBM.ModelObject),
        proposedFactTypeName As String,
        ossieFactTypeName As String,
        description As String,
        roleObjectNames As IList(Of String),
        roleNames As IList(Of String)) As FBM.FactType

        Dim rootName = ToPascalCase(proposedFactTypeName)
        Dim uniqueName = CreateUniqueFactTypeName(model, rootName)
        Dim factType As New FBM.FactType(model, uniqueName, uniqueName) With {
            .ShortDescription = description,
            .LongDescription = description
        }
        AddOssieAlias(factType, ossieFactTypeName)

        For index = 0 To roleObjectNames.Count - 1
            Dim objectName = roleObjectNames(index)
            Dim joinedObject = ResolveObjectType(
                model,
                objectTypes,
                objectName)
            Dim roleName = If(
                index < roleNames.Count,
                roleNames(index),
                String.Empty)
            Dim role As New FBM.Role With {
                .Model = model,
                .FactType = factType,
                .Id = $"{uniqueName}.Role{index + 1}",
                .Name = roleName,
                .SequenceNr = index + 1,
                .JoinedORMObject = joinedObject
            }

            model.AddRole(role)
            factType.RoleGroup.Add(role)
        Next

        model.AddFactType(
            factType,
            abMakeModelDirty:=False,
            abBroadcastInterfaceEvent:=False)
        Return factType
    End Function

    Private Shared Sub AddOssieAlias(
        modelObject As FBM.ModelObject,
        ossieName As String)

        If modelObject Is Nothing OrElse
            String.IsNullOrWhiteSpace(ossieName) Then
            Return
        End If

        modelObject.Alias.Add(
            New FBM.Alias With {
                .AliasType =
                    FactEngineForServices.publicFBMConstants.
                        pcenumORMAliasType.Ossie,
                .Alias = ossieName.Trim()
            })
    End Sub

    Private Shared Sub ApplyDerivation(
        modelObject As FBM.ModelObject,
        expressions As ICollection(Of String))

        If modelObject Is Nothing OrElse expressions Is Nothing Then
            Return
        End If

        Dim rules = expressions.
            Where(Function(expression) Not String.IsNullOrWhiteSpace(expression)).
            Select(Function(expression) expression.Trim()).
            ToList()
        If rules.Count = 0 Then
            Return
        End If

        Dim derivationText As String
        If rules.Count = 1 Then
            derivationText = rules(0)
        Else
            Dim formattedRules As New List(Of String) From {
                $"({rules(0)})"
            }
            For index = 1 To rules.Count - 1
                formattedRules.Add($"OR ({rules(index)})")
            Next

            derivationText = String.Join(
                Environment.NewLine,
                formattedRules)
        End If

        If TypeOf modelObject Is FBM.EntityType Then
            Dim entityType = DirectCast(modelObject, FBM.EntityType)
            entityType.IsDerived = True
            entityType.DerivationText = derivationText
        ElseIf TypeOf modelObject Is FBM.FactType Then
            Dim factType = DirectCast(modelObject, FBM.FactType)
            factType.IsDerived = True
            factType.DerivationText = derivationText
        Else
            modelObject.IsDerived = True
            modelObject.DerivationText = derivationText
        End If
    End Sub

    Private Shared Function CreateUniqueFactTypeName(
        model As FBM.Model,
        rootName As String) As String

        Try
            Return model.CreateUniqueFactTypeName(
                rootName,
                0,
                False)
        Catch ex As System.IO.FileNotFoundException
            If ex.FileName IsNot Nothing AndAlso
                ex.FileName.IndexOf(
                    "ADODB",
                    StringComparison.OrdinalIgnoreCase) >= 0 Then

                ' The DLL's recursive collision path does not forward its
                ' abIncludeDatabaseLookup argument and can therefore try ADODB
                ' even when False was supplied. Keep the viewer database-free.
                Return CreateUniqueFactTypeNameInMemory(model, rootName)
            End If

            Throw
        End Try
    End Function

    Private Shared Function CreateUniqueFactTypeNameInMemory(
        model As FBM.Model,
        rootName As String) As String

        Dim counter = 0
        Do
            Dim candidate = If(
                counter = 0,
                rootName,
                rootName & counter.ToString())
            If Not ModelElementNameExists(model, candidate) Then
                Return candidate
            End If
            counter += 1
        Loop
    End Function

    Private Shared Function ModelElementNameExists(
        model As FBM.Model,
        candidate As String) As Boolean

        Dim isMatch As Func(Of String, Boolean) =
            Function(value As String) String.Equals(
                value,
                candidate,
                StringComparison.OrdinalIgnoreCase)

        Return model.ValueType.Any(
                Function(item) isMatch(item.Name) OrElse isMatch(item.Id)) OrElse
            model.EntityType.Any(
                Function(item) isMatch(item.Name) OrElse isMatch(item.Id)) OrElse
            model.FactType.Any(
                Function(item) isMatch(item.Name) OrElse isMatch(item.Id)) OrElse
            model.ModelDictionary.Any(
                Function(item) isMatch(item.Term))
    End Function

    Private Shared Function CreateFactTypeRootName(
        firstVerbalization As String,
        fallbackName As String) As String

        Return ToPascalCase(
            If(
                String.IsNullOrWhiteSpace(firstVerbalization),
                fallbackName,
                firstVerbalization))
    End Function

    Private Shared Function ToPascalCase(value As String) As String
        Dim words = Regex.Matches(
            If(value, String.Empty),
            "[\p{L}\p{Nd}]+",
            RegexOptions.CultureInvariant)
        Dim result As New Text.StringBuilder()

        For Each word As Match In words
            If word.Length = 0 Then
                Continue For
            End If

            result.Append(Char.ToUpperInvariant(word.Value(0)))
            If word.Length > 1 Then
                result.Append(word.Value.Substring(1))
            End If
        Next

        If result.Length = 0 Then
            Return "FactType"
        End If

        If Char.IsDigit(result(0)) Then
            result.Insert(0, "FactType")
        End If

        Return result.ToString()
    End Function

    Private Shared Function ResolveObjectType(
        model As FBM.Model,
        objectTypes As Dictionary(Of String, FBM.ModelObject),
        objectName As String) As FBM.ModelObject

        Dim normalizedName = TextOrDefault(objectName, "(unspecified object type)")
        Dim joinedObject As FBM.ModelObject = Nothing
        If objectTypes.TryGetValue(normalizedName, joinedObject) Then
            Return joinedObject
        End If

        If PrimitiveValueTypeNames.Contains(normalizedName) Then
            Return AddValueType(
                model,
                objectTypes,
                normalizedName,
                normalizedName,
                "Primitive value type referenced by an Ossie relationship.")
        End If

        ' Preserve the fact and make an unresolved Ossie reference visible.
        Return AddEntityType(
            model,
            objectTypes,
            normalizedName,
            normalizedName,
            "Object type referenced by an Ossie relationship but not declared.")
    End Function

    Private Shared Function InferDataType(
        valueTypeName As String) As FactEngineForServices.publicFBMConstants.pcenumORMDataType

        Select Case If(valueTypeName, String.Empty).Trim().ToLowerInvariant()
            Case "boolean"
                Return FactEngineForServices.publicFBMConstants.pcenumORMDataType.Boolean
            Case "byte"
                Return FactEngineForServices.publicFBMConstants.pcenumORMDataType.NumericUnsignedTinyInteger
            Case "date"
                Return FactEngineForServices.publicFBMConstants.pcenumORMDataType.TemporalDate
            Case "datetime"
                Return FactEngineForServices.publicFBMConstants.pcenumORMDataType.TemporalDateAndTime
            Case "decimal", "number"
                Return FactEngineForServices.publicFBMConstants.pcenumORMDataType.NumericDecimal
            Case "double"
                Return FactEngineForServices.publicFBMConstants.pcenumORMDataType.NumericFloatDoublePrecision
            Case "float"
                Return FactEngineForServices.publicFBMConstants.pcenumORMDataType.NumericFloatSinglePrecision
            Case "integer"
                Return FactEngineForServices.publicFBMConstants.pcenumORMDataType.NumericSignedInteger
            Case "long"
                Return FactEngineForServices.publicFBMConstants.pcenumORMDataType.NumericSignedBigInteger
            Case "short"
                Return FactEngineForServices.publicFBMConstants.pcenumORMDataType.NumericSignedSmallInteger
            Case "string"
                Return FactEngineForServices.publicFBMConstants.pcenumORMDataType.TextVariableLength
            Case "time"
                Return FactEngineForServices.publicFBMConstants.pcenumORMDataType.TemporalTime
            Case Else
                Return FactEngineForServices.publicFBMConstants.pcenumORMDataType.DataTypeNotSet
        End Select
    End Function

    Private Shared Sub AddFactTypeReading(
        factType As FBM.FactType,
        verbalization As String)

        If factType Is Nothing OrElse String.IsNullOrWhiteSpace(verbalization) Then
            Return
        End If

        Dim placeholders = PlaceholderPattern.Matches(verbalization)
        If placeholders.Count = 0 OrElse factType.RoleGroup.Count = 0 Then
            Return
        End If

        Dim reading As New FBM.FactTypeReading With {
            .Model = factType.Model,
            .FactType = factType,
            .Id = $"{factType.Id}.Reading{factType.FactTypeReading.Count + 1}",
            .FrontText = verbalization.Substring(0, placeholders(0).Index),
            .FollowingText = verbalization.Substring(
                placeholders(placeholders.Count - 1).Index +
                placeholders(placeholders.Count - 1).Length)
        }

        Dim roleCount = Math.Min(placeholders.Count, factType.RoleGroup.Count)
        For index = 0 To roleCount - 1
            Dim currentPlaceholder = placeholders(index)
            Dim followingStart =
                currentPlaceholder.Index + currentPlaceholder.Length
            Dim predicateText As String

            If index + 1 < placeholders.Count Then
                predicateText = verbalization.Substring(
                    followingStart,
                    placeholders(index + 1).Index - followingStart).
                    TrimEnd()
            Else
                predicateText = String.Empty
            End If

            Dim role = factType.RoleGroup(index)
            Dim predicatePart As New FBM.PredicatePart With {
                .Model = factType.Model,
                .FactTypeReading = reading,
                .Role = role,
                .SequenceNr = index + 1,
                .PreBoundText = String.Empty,
                .PostBoundText = String.Empty,
                .PredicatePartText = predicateText
            }

            reading.RoleList.Add(role)
            reading.PredicatePart.Add(predicatePart)
        Next

        factType.FactTypeReading.Add(reading)
    End Sub

    Private Shared Function TextOrDefault(
        value As String,
        defaultValue As String) As String

        Return If(String.IsNullOrWhiteSpace(value), defaultValue, value)
    End Function

End Class
