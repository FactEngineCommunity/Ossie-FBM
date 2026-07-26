Imports YamlDotNet.Serialization

Namespace Ossie
    ''' <summary>Marks supplementary metadata for a dimension field.</summary>
    Public Class Dimension
        ''' <summary>Indicates whether the dimension is time-based.</summary>
        <YamlMember(Alias:="is_time")>
        Public Property IsTime As Boolean?
    End Class
End Namespace
