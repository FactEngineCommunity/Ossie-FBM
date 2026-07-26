Imports YamlDotNet.Serialization

Namespace Ossie
    ''' <summary>Vendor-specific attributes that extend the core model.</summary>
    Public Class CustomExtension
        ''' <summary>Name of the vendor owning the extension.</summary>
        <YamlMember(Alias:="vendor_name")>
        Public Property VendorName As String

        ''' <summary>JSON text containing vendor-specific data.</summary>
        Public Property Data As String
    End Class
End Namespace
