Namespace Ossie
    ''' <summary>Supported SQL and expression language dialects.</summary>
    Public Enum Dialect
        ''' <summary>ANSI SQL dialect.</summary>
        ANSI_SQL

        ''' <summary>Snowflake SQL dialect.</summary>
        SNOWFLAKE

        ''' <summary>MDX expression language.</summary>
        MDX

        ''' <summary>Tableau calculation language.</summary>
        TABLEAU

        ''' <summary>Databricks SQL dialect.</summary>
        DATABRICKS

        ''' <summary>GoodData MAQL language.</summary>
        MAQL

        ''' <summary>Google BigQuery SQL dialect.</summary>
        BIGQUERY
    End Enum
End Namespace
