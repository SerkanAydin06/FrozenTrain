#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using FrozenTrain.Atmosphere;
using FrozenTrain.Interaction;
using FrozenTrain.Management;
using FrozenTrain.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using L = FrozenTrain.EditorTools.FrozenTrainLayout;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Builds the whole train from FrozenTrainLayout in one pass. Every car prefab, the connector prefab
    /// and the train's scene placement are regenerated from scratch on each run, so nothing depends on
    /// objects left behind by an earlier build. Player, lighting and atmosphere are created when missing.
    /// </summary>
    public static class FrozenTrainBuilder
    {
        public const string ScenePath = "Assets/Scenes/FPSPrototype.unity";
        public const string TrainRootName = "TRAIN_INTERIOR_PROTOTYPE";
        private const string TrainPrefabFolder = "Assets/Prefabs/Train";
        private const string ConnectorPath = TrainPrefabFolder + "/Shared/CarConnector.prefab";
        private const string MaterialFolder = "Assets/Materials/Prototype";
        private const string SnowParticleMaterialPath = "Assets/Materials/Atmosphere/Train_SnowParticles.mat";
        private const string PostProfilePath = "Assets/Settings/FrozenTrain_PostProcess.asset";
        private const string ExteriorRootName = "OUTSIDE_ENVIRONMENT";
        private const string AtmosphereRootName = "SCENE_ATMOSPHERE";
        private const string PlayerName = "FPS_Player";

        private static Dictionary<TrainMat, Material> mats;

        public static bool Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;

            EnsureFolder(TrainPrefabFolder + "/Shared");
            EnsureFolder(MaterialFolder);
            EnsureFolder("Assets/Materials/Atmosphere");
            EnsureFolder("Assets/Settings");
            EnsureFolder("Assets/Scenes");

            Scene scene = OpenOrCreateScene();
            mats = CreateMaterials();
            try
            {
                GameObject connector = BuildConnectorPrefab();
                GameObject[] cars = new GameObject[L.Cars.Length];
                for (int i = 0; i < cars.Length; i++) cars[i] = BuildCarPrefab(L.Cars[i], i);

                ConfigureLighting();
                TrainPrototypeState state = PlaceTrain(cars, connector);
                BuildExterior();
                BuildAtmosphere(scene);
                EnsurePlayer(state);
            }
            finally { mats = null; }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            return true;
        }

        // ---- Car prefabs --------------------------------------------------------------------

        private static GameObject BuildCarPrefab(CarSpec car, int index)
        {
            bool first = index == 0, last = index == L.Cars.Length - 1;
            // Upper sleeping deck and its ladders are authored for +X; s = -1 mirrors them to the front.
            float s = car.UpperLaddersAtFront ? -1f : 1f;

            GameObject root = new GameObject(car.Name);
            try
            {
                Transform shell = Group(root.transform, "Shell");
                BuildFloors(shell, car, s);
                BuildSides(shell, car);
                BuildEnds(shell, first, last);
                if (car.PanoramicFront)
                    Cube(shell, "PanoramicWindshield", new Vector3(-L.HalfL - .015f, 1.925f, 0f), new Vector3(.035f, 1.65f, 8.66f), M(TrainMat.Glass));
                BuildUnderframe(shell);
                BuildExteriorDressing(Group(root.transform, "ExteriorDressing"), car, index);

                Transform lower = Group(root.transform, "LowerTechnicalDeck");
                BuildProps(lower, L.LowerServices);
                BuildProps(lower, car.Lower);

                BuildProps(Group(root.transform, car.MainGroupName), car.Main);
                BuildUpperSleeping(Group(root.transform, "UpperSleepingDeck"), s);

                BuildCarAccess(Group(root.transform, "CarAccess"), car, last);

                BuildLights(Group(root.transform, "Lighting"), car);

                return PrefabUtility.SaveAsPrefabAsset(root, TrainPrefabFolder + "/" + car.Name + ".prefab");
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static void BuildFloors(Transform p, CarSpec car, float s)
        {
            Material floor = M(TrainMat.Floor);
            var mainHoles = new List<Rect>();
            if (car.TechnicalLadder)
                mainHoles.Add(HoleRect(L.TechnicalHoleX0, L.TechnicalHoleX1, L.TechnicalHoleZ0, L.TechnicalHoleZ1));
            // One upper hatch per car, at the rear end of the +Z public corridor.
            var upperHoles = new List<Rect>
            {
                HoleRect(L.UpperHoleX0, L.UpperHoleX1, L.UpperHoleZ0, L.UpperHoleZ1),
            };

            Cube(p, "LowerFloor", new Vector3(0f, L.LowerY, 0f), new Vector3(L.CarLength, L.LowerFloorT, L.CarWidth), floor);
            FloorWithHoles(p, "MainFloor", L.MainY, L.MainFloorT, mainHoles, floor);
            FloorWithHoles(p, "UpperFloor", L.UpperY, L.UpperFloorT, upperHoles, floor);
            Cube(p, "Roof", new Vector3(0f, L.RoofY, 0f), new Vector3(L.CarLength, L.RoofT, L.CarWidth), M(TrainMat.Wall));
        }

        private static Rect HoleRect(float x0, float x1, float z0, float z1) =>
            Rect.MinMaxRect(Mathf.Min(x0, x1), Mathf.Min(z0, z1), Mathf.Max(x0, x1), Mathf.Max(z0, z1));

        /// <summary>Splits a full-footprint floor into tiles around rectangular openings.</summary>
        private static void FloorWithHoles(Transform p, string prefix, float y, float t, List<Rect> holes, Material m)
        {
            List<float> xs = new List<float> { -L.HalfL, L.HalfL }, zs = new List<float> { -L.HalfW, L.HalfW };
            foreach (Rect h in holes) { xs.Add(h.xMin); xs.Add(h.xMax); zs.Add(h.yMin); zs.Add(h.yMax); }
            xs = xs.Distinct().OrderBy(v => v).ToList();
            zs = zs.Distinct().OrderBy(v => v).ToList();
            int n = 0;
            for (int xi = 0; xi < xs.Count - 1; xi++)
                for (int zi = 0; zi < zs.Count - 1; zi++)
                {
                    float x0 = xs[xi], x1 = xs[xi + 1], z0 = zs[zi], z1 = zs[zi + 1];
                    Vector2 c = new Vector2((x0 + x1) * .5f, (z0 + z1) * .5f);
                    if (holes.Any(h => h.Contains(c))) continue;
                    Cube(p, prefix + n++, new Vector3(c.x, y, c.y), new Vector3(x1 - x0, t, z1 - z0), m);
                }
        }

        private static void BuildSides(Transform p, CarSpec car)
        {
            Material wall = M(TrainMat.Wall);
            float lowerH = L.MainY - L.LowerY, upperH = L.RoofY - L.UpperY;
            foreach (float z in new[] { -L.HalfW, L.HalfW })
            {
                string side = z < 0f ? "L" : "R";
                // Recess the lower side panels slightly at both coupling faces so
                // each car reads as a separate lower body, not one continuous skirt.
                Cube(p, "LowerSide" + side, new Vector3(0f, L.LowerY + lowerH * .5f, z),
                    new Vector3(L.CarLength - .34f, lowerH, L.WallT), wall);
                Cube(p, "UpperSide" + side, new Vector3(0f, L.UpperY + upperH * .5f, z), new Vector3(L.CarLength, upperH, L.WallT), wall);
                BuildMainSide(p, car, z, side);
            }
        }

        private static void BuildMainSide(Transform p, CarSpec car, float z, string side)
        {
            Material wall = M(TrainMat.Wall);
            float top = L.UpperY - L.UpperFloorT * .5f;
            WindowSpan[] windows = z > 0f && car.CorridorSideWindows != null
                ? car.CorridorSideWindows
                : car.MainWindows;
            if (windows.Length == 0)
            {
                Cube(p, "MainSide" + side, new Vector3(0f, top * .5f, z), new Vector3(L.CarLength, top, L.WallT), wall);
                return;
            }

            float wb = L.WindowBottom, wt = L.WindowTop, wy = (wb + wt) * .5f, wh = wt - wb;
            Cube(p, "MainSideBelowWindows" + side, new Vector3(0f, wb * .5f, z), new Vector3(L.CarLength, wb, L.WallT), wall);
            Cube(p, "MainSideAboveWindows" + side, new Vector3(0f, wt + (top - wt) * .5f, z), new Vector3(L.CarLength, top - wt, L.WallT), wall);

            float cursor = -L.HalfL;
            foreach (WindowSpan w in windows.OrderBy(w => w.CenterX))
            {
                float left = w.CenterX - w.Width * .5f;
                if (left > cursor)
                    Cube(p, "MainSidePier" + side, new Vector3((cursor + left) * .5f, wy, z), new Vector3(left - cursor, wh, L.WallT), wall);
                Window(p, w.CenterX, w.Width, z, wb, wt);
                cursor = w.CenterX + w.Width * .5f;
            }
            if (cursor < L.HalfL)
                Cube(p, "MainSidePier" + side, new Vector3((cursor + L.HalfL) * .5f, wy, z), new Vector3(L.HalfL - cursor, wh, L.WallT), wall);
        }

        private static void Window(Transform p, float x, float w, float z, float b, float t)
        {
            Material frame = M(TrainMat.Frame);
            float y = (b + t) * .5f, h = t - b, inward = z > 0f ? -1f : 1f;
            Transform g = Group(p, "Window");
            Cube(g, "WindowTopFrame", new Vector3(x, t, z), new Vector3(w + .16f, .10f, .14f), frame);
            Cube(g, "WindowBottomFrame", new Vector3(x, b, z), new Vector3(w + .16f, .10f, .14f), frame);
            Cube(g, "WindowLeftFrame", new Vector3(x - w * .5f, y, z), new Vector3(.10f, h, .14f), frame);
            Cube(g, "WindowRightFrame", new Vector3(x + w * .5f, y, z), new Vector3(.10f, h, .14f), frame);
            Cube(g, "WindowGlass", new Vector3(x, y, z + inward * .025f), new Vector3(w - .12f, h - .12f, .035f), M(TrainMat.Glass));
        }

        private static void BuildEnds(Transform p, bool first, bool last)
        {
            // Coupled cars have narrow, aligned passages at BOTH middle and technical
            // deck heights. Upper cabin decks remain sealed at the car ends.
            float mainTop = L.UpperY - L.UpperFloorT * .5f;
            float lowerTop = L.MainY - L.MainFloorT * .5f;
            foreach (float sign in new[] { -1f, 1f })
            {
                bool closed = sign < 0f ? first : last;
                string end = sign < 0f ? "Front" : "Rear";
                float x = sign * L.HalfL;

                if (closed) SolidEnd(p, x, L.LowerY, lowerTop, "Lower" + end + "Solid");
                else EndOpening(p, x, L.LowerY, lowerTop, L.LowerEndOpening, L.EndDoorCenterZ, "Lower" + end);

                if (!closed) EndOpening(p, x, L.MainY, mainTop, L.MainEndOpening, L.EndDoorCenterZ, "Main" + end);
                else if (sign > 0f) SolidEnd(p, x, L.MainY, mainTop, "MainRearSolid");
                else BuildCabFront(p, x);

                SolidEnd(p, x, L.UpperY, L.RoofY, "Upper" + end + "Solid");
            }
        }

        /// <summary>Locomotive nose: a wall below and above one panoramic windshield (1.10 m to 2.75 m).</summary>
        private static void BuildCabFront(Transform p, float x)
        {
            Material wall = M(TrainMat.Wall), frame = M(TrainMat.Frame);
            float mainTop = L.UpperY - L.UpperFloorT * .5f;
            Cube(p, "CabFrontLower", new Vector3(x, .55f, 0f), new Vector3(L.WallT, 1.10f, L.CarWidth), wall);
            Cube(p, "CabFrontHeader", new Vector3(x, (2.75f + mainTop) * .5f, 0f), new Vector3(L.WallT, mainTop - 2.75f, L.CarWidth), wall);
            Cube(p, "CabFrontEdgeL", new Vector3(x, 1.925f, -4.43f), new Vector3(L.WallT, 1.65f, .22f), frame);
            Cube(p, "CabFrontEdgeR", new Vector3(x, 1.925f, 4.43f), new Vector3(L.WallT, 1.65f, .22f), frame);
        }

        private static void SolidEnd(Transform p, float x, float bottom, float top, string name) =>
            Cube(p, name, new Vector3(x, (bottom + top) * .5f, 0f), new Vector3(L.WallT, top - bottom, L.CarWidth), M(TrainMat.Wall));

        private static void EndOpening(Transform p, float x, float bottom, float top, Vector2 opening, float centerZ, string prefix)
        {
            float width = opening.x, height = opening.y;
            float h = top - bottom, cy = (bottom + top) * .5f;
            float leftEdge = centerZ - width * .5f, rightEdge = centerZ + width * .5f;
            Cube(p, prefix + "L", new Vector3(x, cy, (-L.HalfW + leftEdge) * .5f), new Vector3(L.WallT, h, leftEdge + L.HalfW), M(TrainMat.Wall));
            Cube(p, prefix + "R", new Vector3(x, cy, (rightEdge + L.HalfW) * .5f), new Vector3(L.WallT, h, L.HalfW - rightEdge), M(TrainMat.Wall));
            float lintel = Mathf.Max(.1f, h - height);
            Cube(p, prefix + "Lintel", new Vector3(x, top - lintel * .5f, centerZ), new Vector3(L.WallT, lintel, width), M(TrainMat.Frame));
        }

        private static void BuildUnderframe(Transform p)
        {
            float frameTop = L.LowerY - L.LowerFloorT * .5f;
            Cube(p, "Underframe", new Vector3(0f, (frameTop + L.UnderframeBottom) * .5f, 0f),
                new Vector3(L.CarLength - .6f, frameTop - L.UnderframeBottom, L.CarWidth - .8f), M(TrainMat.Frame));
            float bogieH = L.UnderframeBottom - L.RailTopY;
            foreach (float x in new[] { -L.HalfL + 3f, L.HalfL - 3f })
                Cube(p, "Bogie", new Vector3(x, L.RailTopY + bogieH * .5f, 0f), new Vector3(3.2f, bogieH, 6.0f), M(TrainMat.Steel));
        }

        /// <summary>Outside-only detail: roof snow, eave icicles and, on the locomotive, the plow and headlights.</summary>
        private static void BuildExteriorDressing(Transform p, CarSpec car, int index)
        {
            Random.State saved = Random.state;
            Random.InitState(4100 + index);
            try
            {
                float roofTop = L.RoofY + L.RoofT * .5f;
                Cube(p, "RoofSnow", new Vector3(0f, roofTop + .06f, 0f), new Vector3(L.CarLength - .5f, .12f, L.CarWidth - .35f), M(TrainMat.Snow));
                for (int i = 0; i < 3; i++)
                {
                    Vector3 size = new Vector3(Random.Range(2.5f, 5f), Random.Range(.10f, .22f), Random.Range(2f, 5f));
                    Cube(p, "RoofSnowDrift", new Vector3(-6f + i * 6f + Random.Range(-1f, 1f), roofTop + .12f + size.y * .5f, Random.Range(-1.5f, 1.5f)), size, M(TrainMat.Snow));
                }

                foreach (float side in new[] { -1f, 1f })
                    for (float x = -L.HalfL + .6f; x < L.HalfL - .4f; x += Random.Range(.8f, 1.6f))
                    {
                        float len = Random.Range(.12f, .55f);
                        Detail(p, "Icicle", new Vector3(x, L.RoofY - L.RoofT * .5f - len * .5f, side * (L.HalfW + .03f)), new Vector3(.05f, len, .05f), TrainMat.Ice);
                    }
            }
            finally { Random.state = saved; }

            if (car.PanoramicFront) BuildPlowAndHeadlights(p);
        }

        /// <summary>Wedge snowplow under the cab windshield and two headlights; keeps clear of the glass (bottom 1.10 m).</summary>
        private static void BuildPlowAndHeadlights(Transform p)
        {
            float x0 = -L.HalfL, reach = 3.2f, halfW = L.HalfW - .1f;
            float bottom = L.RailTopY + .12f, top = L.MainY - .15f, h = top - bottom, cy = bottom + h * .5f;
            Vector3 tip = new Vector3(x0 - reach, cy, 0f);
            Transform plow = Group(p, "SnowPlow");
            foreach (float side in new[] { -1f, 1f })
            {
                Vector3 corner = new Vector3(x0, cy, side * halfW);
                Vector3 dir = corner - tip;
                GameObject blade = Cube(plow, "PlowBlade", (tip + corner) * .5f, new Vector3(dir.magnitude, h, .22f), M(TrainMat.Steel));
                blade.transform.localRotation = Quaternion.FromToRotation(Vector3.right, dir);
                GameObject stripe = Cube(plow, "PlowHazardStripe", (tip + corner) * .5f + new Vector3(0f, h * .28f, 0f), new Vector3(dir.magnitude, .28f, .24f), M(TrainMat.Amber));
                stripe.transform.localRotation = blade.transform.localRotation;
                GameObject snow = Cube(plow, "PlowSnowCrust", (tip + corner) * .5f + new Vector3(0f, -h * .5f + .2f, 0f), new Vector3(dir.magnitude * .9f, .4f, .32f), M(TrainMat.Snow));
                snow.transform.localRotation = blade.transform.localRotation;
            }
            Cube(plow, "PlowNoseRam", tip + new Vector3(.12f, 0f, 0f), new Vector3(.42f, h, .42f), M(TrainMat.Frame));
            Cube(plow, "PlowTopDeck", new Vector3(x0 - reach * .32f, top - .06f, 0f), new Vector3(reach * .64f, .12f, halfW * 1.25f), M(TrainMat.Frame));

            foreach (float z in new[] { -3.25f, 3.25f })
            {
                Vector3 pos = new Vector3(x0 - .14f, .55f, z);
                Detail(p, "HeadlampHousing", pos + new Vector3(.04f, 0f, 0f), new Vector3(.16f, .52f, .62f), TrainMat.Frame);
                Detail(p, "HeadlampLens", pos + new Vector3(-.05f, 0f, 0f), new Vector3(.04f, .40f, .50f), TrainMat.Headlamp);
                Transform t = Empty(p, "Headlight", pos + new Vector3(-.2f, 0f, 0f));
                t.localRotation = Quaternion.Euler(4f, -90f, 0f);
                Light light = t.gameObject.AddComponent<Light>();
                light.type = LightType.Spot;
                light.range = 60f;
                light.spotAngle = 38f;
                light.innerSpotAngle = 18f;
                light.intensity = 4.5f;
                light.color = new Color(1f, .93f, .80f);
                light.shadows = LightShadows.None;
            }
        }

        private static void BuildUpperSleeping(Transform u, float s)
        {
            Material wall = M(TrainMat.Wall);
            float top = L.UpperTop, ceiling = L.RoofBottom, h = ceiling - top, cy = top + h * .5f;
            float[] doors = L.CorridorDoorX.Select(x => s * x).ToArray();

            foreach (float z in new[] { -L.CorridorHalf, L.CorridorHalf })
            {
                // +Z hall wall stops at the stair landing so the ladder hatch opens onto the hall.
                CorridorWall(u, z, cy, h, doors, wall, z > 0f ? L.UpperLandingX : L.HalfL);
                float lintelH = h - L.CorridorDoorHeight;
                foreach (float x in doors)
                    Cube(u, "CorridorDoorLintel", new Vector3(x, ceiling - lintelH * .5f, z), new Vector3(L.CorridorDoorWidth, lintelH, .12f), wall);
            }

            float sideDepth = L.HalfW - L.CorridorHalf;
            foreach (float x in L.SleepingDividerX)
                foreach (float sign in new[] { -1f, 1f })
                    Cube(u, "RoomDivider", new Vector3(s * x, cy, sign * (L.CorridorHalf + sideDepth * .5f)), new Vector3(.12f, h, sideDepth - .04f), wall);

            // Landing partition: shortens the rear +Z cabin and closes it from the stair landing.
            Cube(u, "RoomDivider", new Vector3(L.UpperLandingX, cy, L.CorridorHalf + sideDepth * .5f), new Vector3(.12f, h, sideDepth - .04f), wall);

            foreach (float x in L.SleepingPodX)
                foreach (float sign in new[] { -1f, 1f })
                    SleepPod(u, s * x, sign * L.SleepingPodZ, s);

            Cube(u, "CorridorRunner", new Vector3(0f, top + .005f, 0f), new Vector3(L.CarLength - .8f, .01f, 2f * L.CorridorHalf - .2f), M(TrainMat.Living));
        }

        private static void CorridorWall(Transform p, float z, float y, float h, float[] doorCenters, Material wall, float xEnd)
        {
            float half = L.CorridorDoorWidth * .5f, cursor = -L.HalfL;
            foreach (float c in doorCenters.OrderBy(v => v))
            {
                if (c - half >= xEnd) break;
                if (c - half > cursor)
                {
                    float w = c - half - cursor;
                    Cube(p, "CorridorWall", new Vector3(cursor + w * .5f, y, z), new Vector3(w, h, .12f), wall);
                }
                cursor = Mathf.Max(cursor, c + half);
            }
            if (cursor < xEnd)
            {
                float w = xEnd - cursor;
                Cube(p, "CorridorWall", new Vector3(cursor + w * .5f, y, z), new Vector3(w, h, .12f), wall);
            }
        }

        private static void SleepPod(Transform p, float x, float z, float s)
        {
            float top = L.UpperTop, outer = Mathf.Sign(z);
            Transform g = Group(p, "SleepPod");
            Cube(g, "BedFrame", new Vector3(x, top + .22f, z), new Vector3(2.30f, .44f, .92f), M(TrainMat.Frame));
            Cube(g, "Mattress", new Vector3(x, top + .51f, z), new Vector3(2.14f, .14f, .80f), M(TrainMat.Mattress));
            Cube(g, "Locker", new Vector3(x + s * 1.28f, top + .90f, outer * 4.08f), new Vector3(.62f, 1.80f, .54f), M(TrainMat.Steel));
            Cube(g, "Shelf", new Vector3(x - s * .75f, top + 1.30f, outer * 4.36f), new Vector3(.74f, .08f, .24f), M(TrainMat.Living));
        }

        // ---- In-car access: rear end door and vertical ladders ------------------------------

        /// <summary>
        /// Rear (+X) end of every car: the corridor door to the next car and, beside it on the +Z wall,
        /// the ladder to the private upper deck. The locomotive also gets the only technical-deck ladder
        /// on its -Z wall. Hatch guard rails are ladder children, so they forward the E interaction.
        /// </summary>
        private static void BuildCarAccess(Transform a, CarSpec car, bool last)
        {
            if (!last)
            {
                BuildRearEndDoor(a);
                BuildLowerRearEndDoor(a);
            }

            BuildWallLadder(a, "UpperDeckLadder", L.UpperLadderZ, L.MainTop, L.UpperTop,
                new Vector3(L.AccessLadderX, L.MainTop + .06f, L.HalfW - 1.40f),
                new Vector3(L.AccessLadderX, L.UpperTop + .06f, L.UpperHoleZ0 - .70f),
                Rect.MinMaxRect(L.UpperHoleX0, L.UpperHoleZ0, L.UpperHoleX1, L.UpperHoleZ1),
                "E - Üst kata çık", "E - Ana kata in");

            if (car.TechnicalLadder)
                BuildWallLadder(a, "TechnicalDeckLadder", L.TechnicalLadderZ, L.LowerTop, L.MainTop,
                    new Vector3(L.AccessLadderX, L.LowerTop + .06f, -(L.HalfW - 1.40f)),
                    new Vector3(L.AccessLadderX, L.MainTop + .06f, L.TechnicalHoleZ1 + .70f),
                    Rect.MinMaxRect(L.TechnicalHoleX0, L.TechnicalHoleZ0, L.TechnicalHoleX1, L.TechnicalHoleZ1),
                    "E - Ana kata çık", "E - Teknik kata in");
        }

        private static void BuildRearEndDoor(Transform a)
        {
            float floor = L.MainTop, lintelBottom = L.MainY + L.MainEndOpening.y;
            float height = lintelBottom - floor - .02f, width = L.MainEndOpening.x - .02f;
            Transform door = Group(a, "RearEndDoor");
            // Inside face of the rear end wall; parks toward the +Z outer wall, clear of the ladder.
            door.localPosition = new Vector3(L.HalfL - L.WallT * .5f - .06f, (floor + lintelBottom) * .5f, L.EndDoorCenterZ);
            GameObject panel = Cube(door, "DoorPanel", Vector3.zero, new Vector3(.085f, height, width), M(TrainMat.Frame));
            Object.DestroyImmediate(panel.GetComponent<Collider>());
            door.gameObject.AddComponent<BoxCollider>().size = new Vector3(.10f, height, width);
            door.gameObject.AddComponent<SlidingDoor>().Configure(new Vector3(0f, 0f, width + .01f), 2.8f);
        }

        private static void BuildLowerRearEndDoor(Transform a)
        {
            // One service door per technical-deck gangway. All other lower
            // car ends stay closed to the exterior, and no extra ladders are added.
            float floor = L.LowerTop;
            float lintelBottom = L.LowerY + L.LowerEndOpening.y;
            float height = lintelBottom - floor - .02f;
            float width = L.LowerEndOpening.x - .02f;
            Transform door = Group(a, "LowerRearEndDoor");
            door.localPosition = new Vector3(
                L.HalfL - L.WallT * .5f - .06f,
                (floor + lintelBottom) * .5f, L.EndDoorCenterZ);
            GameObject panel = Cube(door, "DoorPanel", Vector3.zero,
                new Vector3(.085f, height, width), M(TrainMat.Frame));
            Object.DestroyImmediate(panel.GetComponent<Collider>());
            door.gameObject.AddComponent<BoxCollider>().size = new Vector3(.10f, height, width);
            door.gameObject.AddComponent<SlidingDoor>().Configure(
                new Vector3(0f, 0f, width + .01f), 2.8f);
        }

        private static void BuildWallLadder(Transform a, string name, float railZ, float lowY, float highY,
            Vector3 lowExit, Vector3 highExit, Rect hatch, string upText, string downText)
        {
            Material frame = M(TrainMat.Frame), steel = M(TrainMat.Steel);
            float wallSide = Mathf.Sign(railZ);
            Transform ladder = Group(a, name);
            ladder.localPosition = new Vector3(L.AccessLadderX, 0f, railZ);

            const float railHalfWidth = .255f;
            float railBottom = lowY + .02f, railTop = highY + .14f;
            float wallGap = L.HalfW - L.WallT * .5f - Mathf.Abs(railZ);
            foreach (float side in new[] { -1f, 1f })
            {
                Cube(ladder, "LadderRail", new Vector3(side * railHalfWidth, (railBottom + railTop) * .5f, 0f),
                    new Vector3(.065f, railTop - railBottom, .065f), frame);
                foreach (float y in new[] { lowY + .63f, highY - .31f })
                {
                    GameObject mount = Cube(ladder, "WallMount", new Vector3(side * railHalfWidth, y, wallSide * wallGap * .5f),
                        new Vector3(.07f, .075f, wallGap), frame);
                    Object.DestroyImmediate(mount.GetComponent<Collider>());
                }
            }

            int rungCount = Mathf.Max(8, Mathf.CeilToInt((highY - lowY) / .31f));
            for (int i = 0; i < rungCount; i++)
            {
                float y = Mathf.Lerp(lowY + .25f, highY - .23f, i / (float)(rungCount - 1));
                Cube(ladder, "LadderRung", new Vector3(0f, y, -wallSide * .03f), new Vector3(.52f, .05f, .07f), steel);
            }

            // A crossbar in front of the hatch was obstructing the intended
            // walk-on/walk-off path in both the upper and locomotive technical
            // ladders. The rear edge already faces the solid end bulkhead;
            // leave the approach side completely open for the CharacterController.
            // Do not generate the former HatchGuardRail/HatchGuardPost cage.
            // Exit coordinates are authored in car-local space. Translate them
            // to ladder-local space so the E interaction lands on the deck.
            Vector3 origin = ladder.localPosition;
            Transform low = Empty(ladder, "LowerExit", lowExit - origin);
            Transform high = Empty(ladder, "UpperExit", highExit - origin);
            ladder.gameObject.AddComponent<DeckLadder>().Configure(low, high, upText, downText);
        }

        private static void BuildLights(Transform l, CarSpec car)
        {
            PointLight(l, "LowerDeckLight", new Vector3(0f, -.52f, 0f), .78f, new Color(.72f, .82f, .90f));
            PointLight(l, "MainDeckLight", new Vector3(0f, 2.72f, 0f), car.MainLightIntensity, car.MainLightColor);
            PointLight(l, "UpperDeckLight", new Vector3(0f, 5.58f, 0f), .82f, new Color(.86f, .80f, .70f));
            float[] ceilings = { L.MainY - L.MainFloorT * .5f, L.UpperY - L.UpperFloorT * .5f, L.RoofBottom };
            foreach (float ceiling in ceilings)
                foreach (float x in new[] { -5.8f, 0f, 5.8f })
                {
                    Cube(l, "LightHousing", new Vector3(x, ceiling - .035f, 0f), new Vector3(2.8f, .07f, .28f), M(TrainMat.Frame));
                    Cube(l, "LightStrip", new Vector3(x, ceiling - .08f, 0f), new Vector3(2.4f, .03f, .16f), M(TrainMat.Amber));
                }
        }

        private static void PointLight(Transform p, string name, Vector3 pos, float intensity, Color color)
        {
            Transform t = Empty(p, name, pos);
            Light light = t.gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 9.5f;
            light.intensity = intensity;
            light.color = color;
            light.shadows = LightShadows.None;
            t.gameObject.AddComponent<TrainLightDynamics>();
        }

        // ---- Props --------------------------------------------------------------------------

        private static void BuildProps(Transform parent, IEnumerable<Prop> props)
        {
            foreach (Prop prop in props) BuildProp(parent, prop);
        }

        private static GameObject BuildProp(Transform parent, Prop prop)
        {
            GameObject go;
            switch (prop.Shape)
            {
                case PropShape.Box:
                    go = Cube(parent, prop.Name, prop.Position, prop.Size, M(prop.Body));
                    break;
                case PropShape.Pipe:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    go.name = prop.Name;
                    go.transform.SetParent(parent, false);
                    go.transform.localPosition = prop.Position;
                    go.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                    go.transform.localScale = new Vector3(prop.Size.y, prop.Size.x * .5f, prop.Size.z);
                    go.GetComponent<Renderer>().sharedMaterial = M(prop.Body);
                    break;
                case PropShape.Machine:
                case PropShape.Cabinet:
                {
                    go = Empty(parent, prop.Name, prop.Position).gameObject;
                    // Detail faces the car centreline (the aisle) whichever wall the object stands against.
                    float face = parent.TransformPoint(prop.Position).z > .001f ? -1f : 1f;
                    Vector3 size = prop.Size;
                    Cube(go.transform, "Body", Vector3.zero, size, M(prop.Body));
                    if (prop.Shape == PropShape.Machine)
                        Cube(go.transform, "Trim", new Vector3(0f, size.y * .18f, face * size.z * .5f), new Vector3(size.x * .84f, size.y * .18f, .08f), M(prop.Trim));
                    else
                        Cube(go.transform, "Door", new Vector3(0f, 0f, face * size.z * .51f), new Vector3(size.x * .88f, size.y * .86f, .04f), M(prop.Trim));
                    break;
                }
                default:
                    go = Empty(parent, prop.Name, prop.Position).gameObject;
                    go.transform.localRotation = Quaternion.Euler(0f, prop.Yaw, 0f);
                    BuildProps(go.transform, prop.Children);
                    break;
            }

            if (prop.Control.HasValue) go.AddComponent<TrainSystemInteractable>().Configure(null, prop.Control.Value);
            if (prop.Terminal) go.AddComponent<ManagementTerminal>();
            return go;
        }

        // ---- Connector ----------------------------------------------------------------------

        /// <summary>
        /// Two narrow, independent enclosed gangways between coupled car bodies:
        /// one on the public deck, the other on the technical deck. Upper stays sealed.
        /// Full-width side shells never bridge the coupling gap.
        /// </summary>
        private static GameObject BuildConnectorPrefab()
        {
            float gap = L.VestibuleLength - L.WallT; // clear space between end-wall outer faces
            float width = L.MainEndOpening.x + .20f, cz = L.EndDoorCenterZ;
            float roofY = L.MainY + L.MainEndOpening.y + .06f;
            Material floor = M(TrainMat.Floor), rubber = M(TrainMat.Rubber), frame = M(TrainMat.Frame);
            GameObject root = new GameObject("CarConnector");
            try
            {
                Transform g = root.transform;
                Cube(g, "GangwayFloor", new Vector3(0f, L.MainY, cz), new Vector3(L.VestibuleLength, L.MainFloorT, width), floor);
                foreach (float sign in new[] { -1f, 1f })
                    Cube(g, sign < 0f ? "GangwayBellowsL" : "GangwayBellowsR",
                        new Vector3(0f, (L.MainY + roofY) * .5f, cz + sign * (width * .5f + .06f)),
                        new Vector3(gap, roofY - L.MainY, .12f), rubber);
                Cube(g, "GangwayRoof", new Vector3(0f, roofY, cz), new Vector3(gap, .12f, width + .24f), rubber);

                // Service gangway is separately enclosed and the same width as
                // the middle passage. Its side bellows make the lower-car gap
                // visible; no full-width bridging wall or lower side skirt.
                float lowerTop = L.LowerY + L.LowerEndOpening.y + .06f;
                Cube(g, "LowerGangwayFloor", new Vector3(0f, L.LowerY, cz),
                    new Vector3(L.VestibuleLength, L.LowerFloorT, width), floor);
                foreach (float sign in new[] { -1f, 1f })
                    Cube(g, sign < 0f ? "LowerGangwayBellowsL" : "LowerGangwayBellowsR",
                        new Vector3(0f, (L.LowerY + lowerTop) * .5f,
                            cz + sign * (width * .5f + .06f)),
                        new Vector3(gap, lowerTop - L.LowerY, .12f), rubber);
                Cube(g, "LowerGangwayRoof", new Vector3(0f, lowerTop, cz),
                    new Vector3(gap, .12f, width + .24f), rubber);

                GameObject coupler = Cube(g, "Coupler", new Vector3(0f, L.UnderframeBottom + .15f, 0f), new Vector3(L.VestibuleLength + .4f, .22f, .30f), frame);
                Object.DestroyImmediate(coupler.GetComponent<Collider>());
                return PrefabUtility.SaveAsPrefabAsset(root, ConnectorPath);
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static void Detail(Transform p, string name, Vector3 pos, Vector3 size, TrainMat mat)
        {
            GameObject go = Cube(p, name, pos, size, M(mat));
            Object.DestroyImmediate(go.GetComponent<Collider>());
        }

        // ---- Scene --------------------------------------------------------------------------

        private static Scene OpenOrCreateScene()
        {
            if (System.IO.File.Exists(ScenePath))
                return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
            return scene;
        }

        /// <summary>Recreates the cars and connectors under the train root; the root keeps its survival state values.</summary>
        private static TrainPrototypeState PlaceTrain(GameObject[] cars, GameObject connector)
        {
            GameObject train = GameObject.Find(TrainRootName);
            if (train == null) train = new GameObject(TrainRootName);
            train.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            TrainPrototypeState state = GetOrAdd<TrainPrototypeState>(train);
            GetOrAdd<SurvivalSimulation>(train);
            GetOrAdd<AudioSource>(train);
            GetOrAdd<TrainAmbience>(train);

            for (int i = train.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(train.transform.GetChild(i).gameObject);

            for (int i = 0; i < cars.Length; i++)
            {
                var car = (GameObject)PrefabUtility.InstantiatePrefab(cars[i], train.transform);
                car.transform.localPosition = new Vector3(L.CarCenterX(i), 0f, 0f);
                if (i == cars.Length - 1) continue;
                var c = (GameObject)PrefabUtility.InstantiatePrefab(connector, train.transform);
                c.transform.localPosition = new Vector3((L.CarCenterX(i) + L.CarCenterX(i + 1)) * .5f, 0f, 0f);
            }
            return state;
        }

        private static void ConfigureLighting()
        {
            Light sun = Object.FindObjectsByType<Light>(FindObjectsSortMode.None).FirstOrDefault(l => l.type == LightType.Directional);
            if (sun == null)
            {
                sun = new GameObject("ColdExteriorLight").AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.intensity = .58f;
                sun.color = new Color(.60f, .72f, .94f);
                sun.shadows = LightShadows.Soft;
                sun.transform.rotation = Quaternion.Euler(33f, -28f, 0f);
            }

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.085f, .12f, .17f);
            RenderSettings.ambientEquatorColor = new Color(.045f, .055f, .070f);
            RenderSettings.ambientGroundColor = new Color(.018f, .022f, .028f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(.46f, .53f, .60f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 42f;
            RenderSettings.fogEndDistance = 155f;
        }

        /// <summary>Ground, track and passing scenery at the height the train actually stands on.</summary>
        private static void BuildExterior()
        {
            GameObject old = GameObject.Find(ExteriorRootName);
            if (old != null) Object.DestroyImmediate(old);
            Transform env = new GameObject(ExteriorRootName).transform;
            Random.State saved = Random.state;
            Random.InitState(1907);
            try
            {
                float g = L.GroundY;
                GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "SnowGround";
                ground.transform.SetParent(env, false);
                ground.transform.localPosition = new Vector3(0f, g, 0f);
                ground.transform.localScale = new Vector3(28f, 1f, 18f);
                ground.GetComponent<Renderer>().sharedMaterial = M(TrainMat.Snow);

                Cube(env, "TrackBed", new Vector3(0f, g + .02f, 0f), new Vector3(280f, .04f, 6.4f), M(TrainMat.NearRock));
                foreach (float z in new[] { -2.4f, 2.4f })
                    Cube(env, "Rail", new Vector3(0f, L.RailTopY - .07f, z), new Vector3(280f, .14f, .12f), M(TrainMat.Steel));

                Transform near = Group(env, "NEAR_SCENERY");
                near.gameObject.AddComponent<MovingScenery>().Configure(14f, -55f, 90f);
                const float sleeperStep = 1.45f;
                for (int i = 0; i < 100; i++)
                    Cube(near, "Sleeper", new Vector3(-55f + i * sleeperStep, g + .07f, 0f), new Vector3(.30f, .10f, 6.0f), M(TrainMat.Frame));
                // Low, snow-capped rocks beyond 11 m so they never hide the train or crowd the windows.
                for (int i = 0; i < 18; i++)
                {
                    float side = i % 2 == 0 ? -1f : 1f;
                    Vector3 size = new Vector3(Random.Range(2.2f, 5.0f), Random.Range(.6f, 1.6f), Random.Range(1.6f, 3.4f));
                    GameObject rock = Cube(near, "PassingRock", new Vector3(-55f + i * 7.2f, g + size.y * .35f, side * Random.Range(11f, 17f)), size, M(TrainMat.NearRock));
                    rock.transform.localRotation = Quaternion.Euler(Random.Range(-4f, 4f), Random.Range(0f, 180f), Random.Range(-4f, 4f));
                    Cube(rock.transform, "RockSnowCap", new Vector3(0f, .5f, 0f), new Vector3(.92f, .22f, .9f), M(TrainMat.Snow));
                }
                // Wind-packed drifts along the trackside, inside the rocks.
                for (int i = 0; i < 22; i++)
                {
                    float side = i % 2 == 0 ? -1f : 1f;
                    Vector3 size = new Vector3(Random.Range(4f, 9f), Random.Range(.25f, .6f), Random.Range(1.2f, 2.6f));
                    GameObject drift = Cube(near, "SnowDrift", new Vector3(-55f + i * 6.6f, g + size.y * .3f, side * Random.Range(5.6f, 8f)), size, M(TrainMat.Snow));
                    drift.transform.localRotation = Quaternion.Euler(0f, Random.Range(-12f, 12f), side * Random.Range(2f, 6f));
                }
                for (int i = 0; i < 14; i++)
                {
                    float x = -58f + i * 9f, z = i % 2 == 0 ? -8.2f : 8.2f;
                    Cube(near, "TrackSidePost", new Vector3(x, g + 3.5f, z), new Vector3(.18f, 7f, .18f), M(TrainMat.Frame));
                    Cube(near, "PostCrossArm", new Vector3(x, g + 6.6f, z), new Vector3(.12f, .12f, 1.6f), M(TrainMat.Frame));
                }

                Transform far = Group(env, "DISTANT_SCENERY");
                far.gameObject.AddComponent<MovingScenery>().Configure(4f, -75f, 115f);
                // Ridges: a box turned 45 degrees about its long axis, half buried, with snow along the crest.
                for (int i = 0; i < 12; i++)
                {
                    float side = i % 2 == 0 ? -1f : 1f;
                    Vector3 size = new Vector3(Random.Range(16f, 28f), Random.Range(12f, 20f), Random.Range(12f, 20f));
                    float crest = .354f * (size.y + size.z);
                    GameObject m = Cube(far, "DistantMountain", new Vector3(-75f + i * 15f, g - crest * .1f, side * Random.Range(30f, 42f)), size, M(TrainMat.FarRock));
                    m.transform.localRotation = Quaternion.Euler(45f, Random.Range(-25f, 25f), 0f);
                    Cube(m.transform, "RidgeSnow", new Vector3(0f, .5f, -.5f), new Vector3(1.01f, .34f, .34f), M(TrainMat.Snow));
                }
            }
            finally { Random.state = saved; }
        }

        private static void BuildAtmosphere(Scene scene)
        {
            GameObject root = GameObject.Find(AtmosphereRootName);
            if (root == null) root = new GameObject(AtmosphereRootName);
            if (root.GetComponentInChildren<Volume>(true) == null)
            {
                Volume volume = new GameObject("GlobalPostProcessing").AddComponent<Volume>();
                volume.transform.SetParent(root.transform, false);
                volume.isGlobal = true;
                volume.priority = 50f;
                volume.sharedProfile = GetOrCreatePostProfile();
            }

            foreach (GameObject r in scene.GetRootGameObjects())
                foreach (ParticleSystem ps in r.GetComponentsInChildren<ParticleSystem>(true))
                {
                    string n = ps.name.ToLowerInvariant();
                    if (n.Contains("snow") || n.Contains("kar")) Object.DestroyImmediate(ps.gameObject);
                }

            Material snow = GetOrCreateSnowParticleMaterial();
            SnowEmitter(root.transform, "ExteriorSnowFar_Left", -20.5f, snow);
            SnowEmitter(root.transform, "ExteriorSnowFar_Right", 20.5f, snow);
        }

        /// <summary>Snow falls well outside the car body and drifts +X with the passing world.</summary>
        private static void SnowEmitter(Transform p, string name, float z, Material m)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(p, false);
            go.transform.localPosition = new Vector3(0f, 3f, z);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true; main.startLifetime = 6.5f; main.startSpeed = 0f; main.startSize = .075f;
            main.maxParticles = 600; main.simulationSpace = ParticleSystemSimulationSpace.Local; main.gravityModifier = .025f;
            var emission = ps.emission; emission.rateOverTime = 60f;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(135f, 16f, 1.4f);
            var velocity = ps.velocityOverLifetime; velocity.enabled = true; velocity.x = 10f; velocity.y = -1.35f; velocity.z = 0f;
            ParticleSystemRenderer r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = m;
            r.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private static Vector3 GetPrototypeSpawn()
        {
            // Place the player on the +Z public corridor of the first non-cab
            // car; for a locomotive-only preview, use its open central aisle.
            bool hasSecondCar = L.Cars.Length > 1;
            int carIndex = hasSecondCar ? 1 : 0;
            return new Vector3(L.CarCenterX(carIndex), L.MainTop + .04f,
                hasSecondCar ? 2.95f : 0f);
        }

        private static void EnsurePlayer(TrainPrototypeState state)
        {
            GameObject player = GameObject.Find(PlayerName);
            if (player == null)
            {
                player = new GameObject(PlayerName);
                player.transform.position = GetPrototypeSpawn();
                CharacterController cc = player.AddComponent<CharacterController>();
                cc.height = 1.8f; cc.radius = .34f; cc.center = new Vector3(0f, .9f, 0f); cc.stepOffset = .32f; cc.slopeLimit = 50f;

                GameObject camObject = new GameObject("PlayerCamera");
                camObject.transform.SetParent(player.transform, false);
                camObject.transform.localPosition = new Vector3(0f, 1.62f, 0f);
                camObject.tag = "MainCamera";
                Camera cam = camObject.AddComponent<Camera>();
                cam.fieldOfView = 72f; cam.nearClipPlane = .10f; cam.farClipPlane = 220f;
                camObject.AddComponent<AudioListener>();
            }

            // Builder recreates cars but preserves the FPS_Player GameObject. A
            // previously saved five-car player position can otherwise end up on
            // the snow or inside the 1 m inter-car gap after reducing the train.
            CharacterController playerBody = player.GetComponent<CharacterController>();
            bool wasEnabled = playerBody != null && playerBody.enabled;
            if (wasEnabled) playerBody.enabled = false;
            player.transform.position = GetPrototypeSpawn();
            Physics.SyncTransforms();
            if (wasEnabled) playerBody.enabled = true;

            Camera camera = player.GetComponentInChildren<Camera>(true);
            FpsPlayerController controller = GetOrAdd<FpsPlayerController>(player);
            GetOrAdd<PlayerInteraction>(player).Configure(camera, 3.1f);
            GetOrAdd<TrainManagementPanel>(player).Configure(state, controller);
            GetOrAdd<ProceduralMetalFootsteps>(player);
            if (camera != null)
            {
                // Also upgrade existing authored cameras, not just new scenes.
                // A 4cm near clip versus a 260m far clip wasted depth precision
                // in the small interior and amplified coplanar-surface flicker.
                camera.nearClipPlane = .10f;
                camera.farClipPlane = 220f;
                camera.allowMSAA = true;
                UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();
                data.renderPostProcessing = true;
                data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                data.antialiasingQuality = AntialiasingQuality.High;
                data.dithering = true;
            }
        }

        // ---- Assets -------------------------------------------------------------------------

        private static Dictionary<TrainMat, Material> CreateMaterials() => new Dictionary<TrainMat, Material>
        {
            [TrainMat.Floor] = Lit("Train_Floor", new Color(.075f, .085f, .095f), .24f, .44f),
            [TrainMat.Wall] = Lit("Train_Wall", new Color(.16f, .18f, .19f), .24f, .34f),
            [TrainMat.Frame] = Lit("Train_Frame", new Color(.045f, .052f, .060f), .38f, .78f),
            [TrainMat.Steel] = Lit("Train_Steel", new Color(.23f, .26f, .28f), .42f, .80f),
            [TrainMat.Rubber] = Lit("Train_Rubber", new Color(.025f, .030f, .035f), .10f, .02f),
            [TrainMat.Generator] = Lit("Train_Generator", new Color(.26f, .10f, .045f), .32f, .56f),
            [TrainMat.Living] = Lit("Train_Living", new Color(.10f, .19f, .22f), .18f, .05f),
            [TrainMat.Mattress] = Lit("Train_Mattress", new Color(.20f, .25f, .27f), .08f, 0f),
            [TrainMat.Workshop] = Lit("Train_Workshop", new Color(.25f, .18f, .08f), .23f, .36f),
            [TrainMat.Greenhouse] = Lit("Train_Greenhouse", new Color(.07f, .25f, .12f), .16f, .04f),
            [TrainMat.Plant] = Lit("Train_Plant", new Color(.035f, .18f, .07f), .08f, 0f),
            [TrainMat.Screen] = Lit("Train_Screen", new Color(.025f, .21f, .28f), .58f, .10f, new Color(.03f, .70f, .90f) * 1.5f),
            [TrainMat.Amber] = Lit("Train_Amber", new Color(.46f, .16f, .025f), .30f, .10f, new Color(.95f, .31f, .035f) * 1.35f),
            [TrainMat.Snow] = Lit("Train_Snow", new Color(.76f, .83f, .90f), .08f, 0f),
            [TrainMat.NearRock] = Lit("Train_ExteriorRock", new Color(.090f, .105f, .12f), .06f, 0f),
            [TrainMat.FarRock] = Lit("Train_DistantRock", new Color(.16f, .19f, .22f), .04f, 0f),
            [TrainMat.Ice] = Lit("Train_Ice", new Color(.70f, .84f, .92f), .88f, 0f),
            [TrainMat.Headlamp] = Lit("Train_Headlamp", new Color(.90f, .86f, .76f), .70f, .05f, new Color(1f, .92f, .74f) * 3.2f),
            [TrainMat.Glass] = Transparent("Train_WindowGlass", new Color(.42f, .62f, .72f, .13f), .80f, .04f),
        };

        private static Material M(TrainMat m) => mats[m];

        private static Material Lit(string fileName, Color color, float smoothness, float metallic, Color? emission = null)
        {
            string path = MaterialFolder + "/" + fileName + ".mat";
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new System.InvalidOperationException("URP/Lit shader not found.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = fileName };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.color = color;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (emission.HasValue && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material Transparent(string fileName, Color color, float smoothness, float metallic)
        {
            Material material = Lit(fileName, color, smoothness, metallic);
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
            if (material.HasProperty("_SrcBlend")) material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            if (material.HasProperty("_DstBlend")) material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material GetOrCreateSnowParticleMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(SnowParticleMaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "Train_SnowParticles" };
                AssetDatabase.CreateAsset(material, SnowParticleMaterialPath);
            }
            material.shader = shader;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", new Color(.92f, .96f, 1f, .72f));
            EditorUtility.SetDirty(material);
            return material;
        }

        private static VolumeProfile GetOrCreatePostProfile()
        {
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(PostProfilePath);
            if (profile != null) return profile;

            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "FrozenTrain_PostProcess";
            AssetDatabase.CreateAsset(profile, PostProfilePath);
            profile.Add<Tonemapping>(true).mode.Override(TonemappingMode.ACES);
            Bloom bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(.13f);
            bloom.threshold.Override(1.10f);
            ColorAdjustments color = profile.Add<ColorAdjustments>(true);
            color.contrast.Override(7f);
            color.saturation.Override(-5f);
            Vignette vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(.11f);
            vignette.smoothness.Override(.58f);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        // ---- Helpers ------------------------------------------------------------------------

        // Explicit null checks: Unity's fake-null objects defeat the ?? operator.
        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T existing = go.GetComponent<T>();
            return existing != null ? existing : go.AddComponent<T>();
        }

        private static Transform Group(Transform p, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(p, false);
            return go.transform;
        }

        private static Transform Empty(Transform p, string name, Vector3 pos)
        {
            Transform t = Group(p, name);
            t.localPosition = pos;
            return t;
        }

        private static GameObject Cube(Transform p, string name, Vector3 pos, Vector3 scale, Material m)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(p, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            if (m != null) go.GetComponent<Renderer>().sharedMaterial = m;
            return go;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;
            string parent = System.IO.Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string name = System.IO.Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent ?? "Assets", name);
        }
    }
}
#endif
