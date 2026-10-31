# FrozenTrain — Main-deck public corridor

See `Docs/FiveCarArchitecture.md` for the full current architecture.

- Cars 2–5: straight +Z public corridor, Z 1.90–4.40, partition at Z 1.78 with one functional-room door in the middle.
- Corridor outer wall: six standard mirror-symmetric 2.2 m windows (`FrozenTrainLayout.StandardCorridorWindows`).
- Corridor ends: rear end door to the next car (Z 2.55) with the upper-deck ladder beside it on the +Z wall; the front end opening receives the previous car's gangway.
- Passes: `FrozenTrainMainStreetArchitecture` (corridor partition) and `FrozenTrainMainStreetSafety` (partition colliders). The former `MainStreetConnector` was removed with the 4 m vestibules.
