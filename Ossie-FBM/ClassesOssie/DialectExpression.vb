Namespace Ossie
    ''' <summary>Pairs an expression with the SQL or analytics dialect it uses.</summary>
    Public Class DialectExpression
        ''' <summary>Dialect used by the expression.</summary>
        Public Property Dialect As Dialect

        ''' <summary>SQL or dialect-specific expression text.</summary>
        Public Property Expression As String
    End Class
End Namespace
