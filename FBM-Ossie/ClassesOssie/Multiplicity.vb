Namespace Ossie
    ''' <summary>Identifies the multiplicity semantics of a relationship.</summary>
    Public Enum Multiplicity
        ''' <summary>The last role is determined by the preceding roles.</summary>
        ManyToOne

        ''' <summary>A binary relationship is many-to-one in both directions.</summary>
        OneToOne
    End Enum
End Namespace
