Yes—I understand now. The diagram makes the missing structure much clearer.

There are two related layers:

1. The objectified ternary Fact Type:

```text
{Part} is in {Bin} in {Warehouse}
```

2. Three Link Fact Types:

```text
StockedItem involves Part
StockedItem involves Bin
StockedItem involves Warehouse
```

Therefore, each role in `objectifies` should identify:

- The role-playing concept.
- The corresponding relationship/Link Fact Type connecting `StockedItem` to that concept.

It does not need an independently invented role `name`.

I would propose this YAML:

```yaml
- concept:
    name: StockedItem
    type: EntityType

    objectifies:
      roles:
      - concept: Part
        relationship: part

      - concept: Bin
        relationship: bin

      - concept: Warehouse
        relationship: warehouse

      verbalizes:
      - "{Part} is in {Bin} in {Warehouse}"

  relationships:
  - name: part
    roles:
    - concept: Part
    verbalizes:
    - "{StockedItem} involves {Part}"
    - "{Part} is involved in {StockedItem}"

  - name: bin
    roles:
    - concept: Bin
    verbalizes:
    - "{StockedItem} involves {Bin}"
    - "{Bin} is involved in {StockedItem}"

  - name: warehouse
    roles:
    - concept: Warehouse
    verbalizes:
    - "{StockedItem} involves {Warehouse}"
    - "{Warehouse} is involved in {StockedItem}"
```

The references resolve to the qualified Ossie relationship names:

```text
StockedItem.part
StockedItem.bin
StockedItem.warehouse
```

We could make that explicit in the YAML:

```yaml
objectifies:
  roles:
  - concept: Part
    relationship: StockedItem.part
  - concept: Bin
    relationship: StockedItem.bin
  - concept: Warehouse
    relationship: StockedItem.warehouse

  verbalizes:
  - "{Part} is in {Bin} in {Warehouse}"
```

I prefer the fully qualified form because it is unambiguous and aligns directly with the aliases we are now storing in FBM.

The resulting mapping would be:

| Objectified role | Role player | Link Fact Type          |
| ---------------- | ----------- | ----------------------- |
| 1                | `Part`      | `StockedItem.part`      |
| 2                | `Bin`       | `StockedItem.bin`       |
| 3                | `Warehouse` | `StockedItem.warehouse` |

The mapper would:

- Create the `StockedItem` Entity Type.
- Create the three binary relationships as Link Fact Types.
- Create the ternary Fact Type, also named `StockedItem`.
- Join its roles to `Part`, `Bin`, and `Warehouse`.
- Associate each role with its corresponding Link Fact Type.
- Mark `StockedItem` as the objectifying Entity Type.

That is substantially more faithful to the FBM/ORM structure shown in your diagram.


=====================

For objectified Fact Types, I recommend:

1. Create a **GitHub Discussion**, because this is initially a design idea rather than a confirmed defect or implementation task.
2. Send one concise email to `dev@ossie.apache.org` with a self-contained summary and a link to the Discussion.
3. If the community likes the direction, turn it into a concrete specification issue or pull request.

Ossie explicitly says the mailing list is its primary decision-making channel; GitHub Issues and Discussions are secondary. Their formal process for specification changes ultimately requires announcement on `dev@` and a pull request. [Ossie contribution guidance](https://github.com/apache/ossie/blob/main/CONTRIBUTING.md)

A suitable email subject would be:

```text
[DISCUSS] Proposal: Representing objectified fact types in Ossie ontology
```

The email should contain perhaps four short paragraphs:

- The missing use case.
- The `StockedItem` example.
- Why ordinary `identify_by` is not semantically equivalent.
- A link to the GitHub Discussion containing the diagram and proposed YAML.

I would not send only a naked link—the email should make sense in the permanent Apache archive.

That approach is unlikely to annoy anyone. It is one considered, technically grounded proposal with an actual interoperability use case. The project actively invites specification feedback and use-case discussions, and this is exactly that. [Apache Ossie community page](https://ossie.apache.org/)