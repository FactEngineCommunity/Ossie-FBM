Imports System.Collections.Generic
Imports YamlDotNet.Serialization
Imports YamlDotNet.Serialization.NamingConventions

Namespace Ossie
    ''' <summary>Root document for an Apache Ossie core semantic model definition.</summary>
    Public Class SemanticModelDocument
        Inherits OssieDocument

        ''' <summary>Collection of semantic models in the document.</summary>
        <YamlMember(Alias:="semantic_model")>
        Public Property SemanticModel As List(Of SemanticModel)
    End Class
End Namespace
