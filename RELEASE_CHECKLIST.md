# Public Release Checklist

This checklist is the release gate for the first public FactEngineCommunity version of Ossie-FBM.

## Blocking

- [x] Include `Dependencies/FactEngineForServices.dll` only with its FactEngine.AI non-commercial-use licence and copyright notice.
- [ ] Confirm that “FactEngineCommunity contributors” is the desired copyright statement for original project code.
- [ ] Review every example’s provenance and retain or add the required attribution.
- [ ] Remove generated, user-specific, temporary, and validation files from the publication set.
- [ ] Confirm the repository can be cloned and built on a clean Windows environment.
- [ ] Run representative Ossie → FBM and FBM → Ossie conversions and record the results.

## Documentation

- [x] Add a project overview and status statement.
- [x] Document features, requirements, build steps, examples, and known limitations.
- [x] State that the project is independent and not an official Apache Ossie project.
- [x] Document the unresolved binary dependency prominently.
- [x] Separate Apache-licensed original work from the FactEngine.AI non-commercial binary terms.
- [x] Document the bundled dependency path and non-commercial redistribution scope.
- [ ] Add at least two screenshots: the Ossie tree/YAML view and the converted FBM view.
- [x] Add stable links to the FBM background and upstream Ossie project.
- [x] Add a concise “What we learned implementing Ossie” note.

## Engineering

- [x] Verify a Release build.
- [x] Achieve a build with zero warnings and zero errors.
- [x] Add automated verification for YAML root detection, parsing, and serialisation.
- [ ] Add conversion tests for at least one model in each direction.
- [ ] Decide whether .NET 10 is appropriate for the first release or whether to target an LTS runtime.
- [ ] Produce and smoke-test a distributable Windows build.

## Community launch

- [ ] Choose the public FactEngineCommunity repository name and stable URL.
- [ ] Publish the implementation note.
- [ ] Prepare the self-contained `dev@ossie.apache.org` introduction.
- [ ] Prepare the linked GitHub Discussion with the objectification example and candidate YAML.
- [ ] Identify the first bounded documentation, example, test, or schema contribution to offer.

## Verified locally

| Date | Check | Result |
| --- | --- | --- |
| 2026-07-26 | `dotnet build .\Ossie-FBM.slnx --configuration Release --nologo` using SDK 10.0.302 | Passed: 0 warnings, 0 errors |
