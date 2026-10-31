# FrozenTrain — Main-deck public corridor

See `Docs/FiveCarArchitecture.md` for the full current architecture.

- Cars 2–5: straight +Z public corridor, Z 1.5–4.5 (3 m), 0.16 m partition with its corridor face at Z 1.5 and one 2 m functional-room door in the middle.
- Corridor outer wall: six standard mirror-symmetric 2 m windows on the 1 m grid (`FrozenTrainLayout.StandardCorridorWindows`).
- Corridor ends: rear end door to the next car (Z 1.5..2.5) with the upper-deck ladder beside it on the +Z wall; the front end opening receives the previous car's gangway.
- Passes: `FrozenTrainMainStreetArchitecture` (corridor partition) and `FrozenTrainMainStreetSafety` (partition colliders). The former `MainStreetConnector` was removed with the 4 m vestibules.

- Technical deck inter-car circulation: lower gangway is aligned at Z 1.5..2.5; only the locomotive has the vertical entry ladder. Each joint has a separate lower sliding door and covered service gangway; upper car ends stay sealed.
- Upper cabin doors slide consistently toward +X (train rear); hatches are unobstructed for ladder access.
