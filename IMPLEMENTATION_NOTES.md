# What We Learned Implementing Ossie

## Purpose

FBM-Ossie is a working experiment in two-way interchange between Fact-Based Modelling/Object-Role Modeling (FBM/ORM) and Apache Ossie. This note records what became visible through implementation. It is not a claim that every construct is absent from the complete or current Ossie specification.

## What maps cleanly

Several Ossie ontology constructs have direct and useful FBM/ORM counterparts:

| Ossie | FBM/ORM implementation |
| --- | --- |
| `EntityType` and `ValueType` | Entity Types and Value Types |
| `extends` | Supertype relationships and primitive value domains |
| `relationships` and `roles` | Fact Types and their ordered Roles |
| `verbalizes` | Fact Type readings with role placeholders |
| `identify_by` | Preferred reference schemes, including multi-relationship identifiers |
| `multiplicity` | Common single-role internal uniqueness patterns |
| `derived_by` | Derivation text on Value Types, Entity Types, and Fact Types |
| `ontology_mappings` | Mappings from conceptual model elements to datasets and fields |

The strongest alignment is the preservation of natural-language readings. The converter assembles Ossie `verbalizes` entries from ordered FBM predicate parts and can reconstruct FBM readings from Ossie role placeholders. This makes the interchange understandable to subject-matter experts rather than only to YAML tooling.

## What implementation exposed

### 1. Objectified Fact Types need explicit semantics

In FBM/ORM, a Fact Type can be objectified: the Fact Type is treated as an Entity Type that can itself play roles. For example:

```text
Part is in Bin in Warehouse
```

may be objectified as `StockedItem`, with Link Fact Types connecting `StockedItem` to `Part`, `Bin`, and `Warehouse`.

The current converter can recognise this FBM structure, but using ordinary `identify_by` relationships in Ossie preserves only part of its meaning. It does not explicitly state:

- that `StockedItem` objectifies the ternary Fact Type;
- the ordered Roles of that objectified Fact Type; or
- which Link Fact Type corresponds to each objectified Role.

This is therefore a round-trip problem rather than a request for different terminology. The working question for the Ossie community is:

> How should an Ossie document state that an Entity Type objectifies a Fact Type while preserving each Role and its corresponding Link Fact Type?

A candidate `objectifies` structure and complete `StockedItem` YAML example are maintained in `Notes/Proposal for Objectified Fact Types.md`.

### 2. Common uniqueness maps; richer uniqueness needs investigation

Ossie `ManyToOne` and `OneToOne` multiplicities can be mapped to common single-role internal uniqueness constraints. `identify_by` can also produce preferred reference schemes, including an external uniqueness constraint when several relationships jointly identify an Entity Type.

Further work is needed to determine the intended portable representation of arbitrary joint uniqueness and uniqueness across role paths. An expression may enforce the same population rule, but a named constraint remains easier for downstream software to recognise, diagram, and verbalise.

### 3. `requires` carries several kinds of meaning

The examined examples use `requires` for value ranges, mandatory participation, comparison rules, consistency between paths, and exclusions. This is expressive, but downstream tooling has to interpret the expression before it can identify the underlying constraint family.

That matters for an FBM converter because “this expression must be true” is not always enough to reconstruct whether the author intended a mandatory-role, exclusion, ring, subset, or another recognised conceptual constraint. The next step is to verify this reading against the complete current specification before proposing any vocabulary.

### 4. Physical mapping is a notable strength

Ossie makes the binding between conceptual ontology elements and physical datasets explicit and portable. The FBM-to-Ossie mapper can use the relational data structure associated with an FBM model to create semantic-model datasets, object mappings, referent mappings, and link mappings.

This is an area where the Ossie format adds practical interchange value beyond the conceptual model alone.

## Current implementation boundary

The implementation currently provides useful evidence, but it is not complete:

- parsing and serialisation cover ontology and standalone semantic-model document roots;
- conversion concentrates on ontology concepts, relationships, readings, identifiers, derivations, common multiplicities, and physical mappings;
- objectification is recognised in the FBM source but cannot yet be represented without candidate Ossie syntax;
- not every FBM constraint family is emitted or reconstructed;
- round-trip preservation still needs an automated model-level test corpus.

## Proposed way to collaborate

The most useful first discussion is one bounded objectification example, supported by the converter and a minimal model. Broader constraint findings should be verified and discussed separately.

If the community agrees that a representation gap exists, this project can contribute whichever artifact is most helpful: example YAML, schema changes, documentation, validation cases, or conversion tests.
