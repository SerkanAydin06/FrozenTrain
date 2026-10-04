# FT_LIV_ProductionPack_V1

## Goal
Replace the most obvious greybox furniture in the Living car with a free, reproducible production-art pass that requires no manual Blender modelling.

## Scope
- Four-person facing train seating bays
- Dining table + pedestal
- Upper-deck sleep pod details
- Galley counter details
- Medical cabinet details
- Wall heater details
- Generated tileable fabric / bedding / painted-metal PBR textures

## Style
Semi-realistic industrial survival train. Cold exterior, warmer occupied-space materials. Functional, durable, slightly worn, not luxury and not sci-fi neon.

## Geometry rules
- Keep current placement and circulation from `FrozenTrainLayout.cs`.
- Do not block windows, ladders, connector doors, or the central aisle.
- Keep interaction components on their existing root objects.
- Rebuild must remain deterministic and idempotent.
- Production visual changes are a skin/detail pass; architecture remains owned by `FrozenTrainLayout.cs` + `FrozenTrainBuilder.cs`.

## Materials
- Dark blue-grey woven seat fabric
- Dark painted steel seat frames
- Warm grey table surface
- Muted bedding fabric for sleep pods
- Desaturated medical/galley metals

## Automation
`Frozen Train > Rebuild Authored Train` runs the production-art pass automatically after the canonical builder. No second public menu command.

## Source / license
Generated procedurally inside the Unity Editor from repository code. No external paid service and no third-party copyrighted texture source.
