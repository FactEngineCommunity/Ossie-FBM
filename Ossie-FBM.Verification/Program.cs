using FBM_Ossie.Ossie;

var checks = new (string Name, Action Run)[]
{
    ("Detect and parse an ontology document", DetectOntology),
    ("Detect and parse a semantic-model document", DetectSemanticModel),
    ("Round-trip snake_case ontology YAML", RoundTripOntology),
    ("Reject an unsupported root document", RejectUnsupportedDocument)
};

var failures = new List<string>();

foreach (var check in checks)
{
    try
    {
        check.Run();
        Console.WriteLine($"PASS  {check.Name}");
    }
    catch (Exception exception)
    {
        failures.Add($"{check.Name}: {exception.Message}");
        Console.Error.WriteLine($"FAIL  {check.Name}");
        Console.Error.WriteLine($"      {exception.Message}");
    }
}

Console.WriteLine();
Console.WriteLine($"{checks.Length - failures.Count}/{checks.Length} verification checks passed.");

if (failures.Count > 0)
{
    Environment.ExitCode = 1;
}

return;

static void DetectOntology()
{
    const string yaml = """
        version: "1.0"
        name: Cinema
        ontology:
          - concept:
              name: Person
              type: EntityType
        """;

    var document = OssieYaml.DeserializeDocument(yaml);
    var ontology = RequireType<OntologyDocument>(document);

    RequireEqual("Cinema", ontology.Name, "ontology name");
    RequireEqual(1, ontology.Ontology.Count, "ontology component count");
    RequireEqual("Person", ontology.Ontology[0].Concept.Name, "concept name");
    RequireEqual(ConceptType.EntityType, ontology.Ontology[0].Concept.Type, "concept type");
}

static void DetectSemanticModel()
{
    const string yaml = """
        version: "1.0"
        semantic_model:
          - name: cinema_analytics
            datasets: []
        """;

    var document = OssieYaml.DeserializeDocument(yaml);
    var semanticModel = RequireType<SemanticModelDocument>(document);

    RequireEqual(1, semanticModel.SemanticModel.Count, "semantic-model count");
    RequireEqual("cinema_analytics", semanticModel.SemanticModel[0].Name, "semantic-model name");
}

static void RoundTripOntology()
{
    const string yaml = """
        version: "1.0"
        name: Flights
        ontology:
          - concept:
              name: Airport
              type: EntityType
              identify_by:
                - code
            relationships:
              - name: code
                roles:
                  - concept: AirportCode
                verbalizes:
                  - "{Airport} has {AirportCode}"
        """;

    var source = OssieYaml.DeserializeOntology(yaml);
    var serialised = OssieYaml.CreateSerializer().Serialize(source);

    Require(serialised.Contains("identify_by:"), "serialised YAML must use identify_by");
    Require(serialised.Contains("verbalizes:"), "serialised YAML must preserve verbalizes");

    var roundTripped = OssieYaml.DeserializeOntology(serialised);
    RequireEqual("Flights", roundTripped.Name, "round-tripped ontology name");
    RequireEqual("code", roundTripped.Ontology[0].Concept.IdentifyBy[0], "round-tripped identifier");
    RequireEqual(
        "{Airport} has {AirportCode}",
        roundTripped.Ontology[0].Relationships[0].Verbalizes[0],
        "round-tripped verbalisation");
}

static void RejectUnsupportedDocument()
{
    const string yaml = """
        version: "1.0"
        unknown_section: []
        """;

    try
    {
        OssieYaml.DeserializeDocument(yaml);
    }
    catch (InvalidDataException)
    {
        return;
    }

    throw new InvalidOperationException("An unsupported root document was accepted.");
}

static T RequireType<T>(object value)
{
    if (value is T typedValue)
    {
        return typedValue;
    }

    throw new InvalidOperationException(
        $"Expected {typeof(T).Name}, received {value?.GetType().Name ?? "Nothing"}.");
}

static void Require(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static void RequireEqual<T>(T expected, T actual, string subject)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException(
            $"Unexpected {subject}. Expected '{expected}', received '{actual}'.");
    }
}
