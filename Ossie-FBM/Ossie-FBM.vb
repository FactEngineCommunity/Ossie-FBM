Public Class FBMOssie

    ''' <summary>Imports an Apache Ossie ontology from a YAML file.</summary>
    ''' <param name="filePath">Path to the .yaml or .yml file.</param>
    ''' <returns>The deserialized Ossie ontology document.</returns>
    Public Shared Function Import(filePath As String) As Ossie.OntologyDocument
        Return Ossie.OssieYaml.ImportOntology(filePath)
    End Function

    ''' <summary>
    ''' Imports either an Apache Ossie ontology or standalone semantic-model YAML file.
    ''' </summary>
    ''' <param name="filePath">Path to the .yaml or .yml file.</param>
    ''' <returns>The detected, deserialized Ossie document.</returns>
    Public Shared Function ImportDocument(filePath As String) As Ossie.OssieDocument
        Return Ossie.OssieYaml.ImportDocument(filePath)
    End Function

End Class
