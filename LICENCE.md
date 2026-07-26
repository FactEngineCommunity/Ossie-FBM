# Licensing Guide

FBM-Ossie contains material under more than one set of terms. This guide explains which licence applies to each part of the repository; it does not replace the full licence texts or notices.

## Original FBM-Ossie work

Unless a file states otherwise, the original source code and documentation in this repository are:

> Copyright 2026 FactEngineCommunity contributors

and licensed under the [Apache License, Version 2.0](LICENSE).

This includes the Ossie .NET document model and YAML support, the WinForms viewer and mapping source, the verification project, and original documentation.

## FactEngineForServices binary

`Dependencies/FactEngineForServices.dll` is a separately licensed component:

> Copyright FactEngine.AI

FactEngine.AI permits the binary to be used for non-commercial purposes but does not permit redistribution. It is not licensed under Apache 2.0. See [Dependencies/LICENCE.md](Dependencies/LICENCE.md).

Commercial use of the complete application requires the necessary permission or licence for this component from FactEngine.AI.

The binary must not be committed to this repository or included in an FBM-Ossie release. Each user must obtain it directly from FactEngine.AI.

## Examples and upstream material

Example files may contain material originating from Apache Ossie or FactEngineCommunity. Their scope and retained notices are described in [FBM-Ossie/Example/LICENCE.md](FBM-Ossie/Example/LICENCE.md) and [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

The repository-level Apache 2.0 licence does not relicense third-party material. A component’s own copyright and licence notice takes precedence for that component.
