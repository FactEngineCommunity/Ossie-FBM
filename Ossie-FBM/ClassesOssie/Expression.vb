Imports System.Collections.Generic

Namespace Ossie
    ''' <summary>Defines one expression in one or more supported dialects.</summary>
    Public Class Expression
        ''' <summary>Dialect-specific implementations of the expression.</summary>
        Public Property Dialects As List(Of DialectExpression)
    End Class
End Namespace
