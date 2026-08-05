using UnityEngine;
using UnityEngine.Rendering;

namespace VexUnbound
{
    public static class GothicFortressEnvironment
    {
        private static readonly Color DistantStone = new(0.28f, 0.31f, 0.43f);
        private static readonly Color NearStone = new(0.15f, 0.17f, 0.25f);
        private static Mesh cubeMesh;
        private static Mesh spireMesh;
        private static Material platformSideMaterial;
        private static Material platformTopMaterial;
        private static Material darkStoneMaterial;
        private static Material ironMaterial;
        private static Material distantMaterial;
        private static Material nearMaterial;
        private static Material bannerMaterial;
        private static Material barricadeWoodMaterial;
        private static Material warmMaterial;
        private static Material rainMaterial;
        private static Texture2D barricadeWoodTexture;

        public static void CreatePlatform(string name, Vector3 position, Vector3 size)
        {
            GameObject platform = new(name);
            platform.transform.position = position;

            BoxCollider collider = platform.AddComponent<BoxCollider>();
            collider.size = size;

            Transform visuals = new GameObject("Visuals").transform;
            visuals.SetParent(platform.transform, false);

            const float topHeight = 0.14f;
            GameObject foundation = CreateCube(
                visuals,
                "Continuous Foundation",
                new Vector3(0f, -topHeight * 0.5f, 0f),
                new Vector3(size.x, size.y - topHeight, size.z),
                PlatformSideMaterial,
                true);
            SetTextureTiling(foundation, new Vector2(Mathf.Max(1f, size.x / 1.35f), Mathf.Max(1f, size.y / 0.65f)));

            GameObject cap = CreateCube(
                visuals,
                "Continuous Walkable Cap",
                new Vector3(0f, size.y * 0.5f - topHeight * 0.5f, 0f),
                new Vector3(size.x, topHeight, size.z),
                PlatformTopMaterial,
                true);
            SetTextureTiling(cap, new Vector2(Mathf.Max(1f, size.x / 1.2f), Mathf.Max(1f, size.z / 1.2f)));

            GameObject edgeCourse = CreateCube(
                visuals,
                "Front Edge Course",
                new Vector3(0f, size.y * 0.5f - topHeight - 0.055f, -size.z * 0.5f - 0.035f),
                new Vector3(size.x, 0.11f, 0.07f),
                PlatformTopMaterial,
                true);
            SetTextureTiling(edgeCourse, new Vector2(Mathf.Max(1f, size.x / 1.2f), 1f));
        }

        public static void CreateObstacle(string name, Vector3 position, Vector3 size)
        {
            GameObject obstacle = new(name);
            obstacle.transform.position = position;
            obstacle.AddComponent<BoxCollider>().size = size;
            GameObject block = CreateCube(obstacle.transform, "Wooden Barricade", Vector3.zero, size, BarricadeWoodMaterial, true);
            SetTextureTiling(block, new Vector2(2f, Mathf.Max(2f, size.y * 4f)));
        }

        public static void ConfigureScene(Camera camera)
        {
            camera.backgroundColor = new Color(0.055f, 0.07f, 0.13f);
            camera.transform.rotation = Quaternion.Euler(5f, 0f, 0f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.15f, 0.175f, 0.27f);
            RenderSettings.fogStartDistance = 10.5f;
            RenderSettings.fogEndDistance = 20f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.34f, 0.39f, 0.57f);
            RenderSettings.ambientEquatorColor = new Color(0.2f, 0.24f, 0.37f);
            RenderSettings.ambientGroundColor = new Color(0.085f, 0.1f, 0.17f);
            RenderSettings.ambientIntensity = 1.1f;

            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                if (light.type != LightType.Directional)
                {
                    continue;
                }

                light.color = new Color(0.7f, 0.76f, 1f);
                light.intensity = 1.25f;
                light.shadows = LightShadows.Hard;
                light.shadowStrength = 0.35f;
            }

            CreateDistantKeep(camera.transform);
            CreateNearRamparts(camera.transform);
            CreateDecorations();
            CreateRain();
        }

        public static void CreateFinishVisual(Transform finish)
        {
            Transform gate = new GameObject("Fortress Gate Visuals").transform;
            gate.SetParent(finish, false);

            CreateCube(gate, "Left Stone Pier", new Vector3(-0.63f, 0f, 0.05f), new Vector3(0.3f, 2.55f, 0.52f), DarkStoneMaterial, true);
            CreateCube(gate, "Right Stone Pier", new Vector3(0.63f, 0f, 0.05f), new Vector3(0.3f, 2.55f, 0.52f), DarkStoneMaterial, true);
            CreateCube(gate, "Gate Lintel", new Vector3(0f, 1.18f, 0.05f), new Vector3(1.55f, 0.28f, 0.52f), DarkStoneMaterial, true);
            CreateSpire(gate, "Pointed Crown", new Vector3(0f, 1.32f, 0.05f), new Vector3(1.5f, 0.72f, 0.5f), DarkStoneMaterial);

            for (int i = -2; i <= 2; i++)
            {
                CreateCube(gate, $"Iron Bar {i + 3}", new Vector3(i * 0.2f, -0.05f, -0.24f), new Vector3(0.045f, 2.25f, 0.045f), IronMaterial, false);
            }

            CreateCube(gate, "Iron Brace", new Vector3(0f, 0.2f, -0.24f), new Vector3(1.05f, 0.06f, 0.055f), IronMaterial, false);
            CreateLantern(gate, new Vector3(-0.92f, 0.42f, -0.42f), true);
        }

        private static void CreateDistantKeep(Transform camera)
        {
            Transform layer = CreateParallaxLayer("Distant Keep", camera, 0.1f);
            CreateCube(layer, "Distant Curtain Wall", new Vector3(22f, -1.15f, 6f), new Vector3(82f, 2.4f, 0.55f), DistantMaterial, false);

            float[] towerX = { -11f, -5f, 1.5f, 8.5f, 15f, 22f, 29f, 36f, 43f, 50f, 57f };
            float[] towerHeights = { 4.2f, 5.4f, 4.7f, 5.8f, 4.5f, 5.3f, 4.8f, 5.6f, 4.4f, 5.2f, 4.9f };
            for (int i = 0; i < towerX.Length; i++)
            {
                float height = towerHeights[i];
                CreateCube(layer, $"Distant Tower {i + 1}", new Vector3(towerX[i], -0.55f + height * 0.5f, 5.9f), new Vector3(1.35f, height, 0.7f), DistantMaterial, false);
                CreateBattlements(layer, $"Distant Tower {i + 1}", towerX[i], -0.55f + height, 5.9f, 1.6f, DistantMaterial);
                CreateSpire(layer, $"Distant Spire {i + 1}", new Vector3(towerX[i], -0.55f + height, 5.9f), new Vector3(1.45f, 1.7f + i % 2 * 0.5f, 0.75f), DistantMaterial);

                if (i % 2 == 1)
                {
                    CreateCube(layer, $"Distant Window {i + 1}", new Vector3(towerX[i], height * 0.55f, 5.5f), new Vector3(0.16f, 0.55f, 0.04f), WarmMaterial, false);
                }
            }

            StaticBatchingUtility.Combine(layer.gameObject);
        }

        private static void CreateNearRamparts(Transform camera)
        {
            Transform layer = CreateParallaxLayer("Near Ramparts", camera, 0.27f);
            CreateCube(layer, "Near Curtain Wall", new Vector3(22f, -1.85f, 3.6f), new Vector3(82f, 2.5f, 0.7f), NearMaterial, false);
            CreateBattlements(layer, "Near Wall", 22f, -0.55f, 3.6f, 82f, NearMaterial, 1.1f);

            float[] towerX = { -12f, -2.5f, 6f, 15f, 24f, 31f, 39f, 47f, 55f };
            for (int i = 0; i < towerX.Length; i++)
            {
                float height = 3.2f + (i % 2) * 0.75f;
                CreateCube(layer, $"Near Tower {i + 1}", new Vector3(towerX[i], -1.35f + height * 0.5f, 3.5f), new Vector3(1.8f, height, 0.85f), NearMaterial, false);
                CreateBattlements(layer, $"Near Tower {i + 1}", towerX[i], -1.35f + height, 3.5f, 2.05f, NearMaterial);
            }

            StaticBatchingUtility.Combine(layer.gameObject);
        }

        private static Transform CreateParallaxLayer(string name, Transform camera, float factor)
        {
            GameObject layer = new(name);
            ParallaxLayer parallax = layer.AddComponent<ParallaxLayer>();
            parallax.Target = camera;
            parallax.Factor = factor;
            return layer.transform;
        }

        private static void CreateDecorations()
        {
            Transform decorations = new GameObject("Fortress Decorations").transform;

            Transform railing = new GameObject("Rampart Railing").transform;
            railing.SetParent(decorations, false);
            railing.position = new Vector3(-1.35f, -0.15f, 1.15f);
            CreateCube(railing, "Top Rail", new Vector3(0f, 0.35f, 0f), new Vector3(2.4f, 0.07f, 0.07f), IronMaterial, false);
            for (int i = -2; i <= 2; i++)
            {
                CreateCube(railing, $"Railing Post {i + 3}", new Vector3(i * 0.55f, 0f, 0f), new Vector3(0.06f, 0.72f, 0.06f), IronMaterial, false);
            }

            CreateCube(decorations, "Banner Pole", new Vector3(2.1f, 1.15f, 1.2f), new Vector3(0.055f, 2.6f, 0.055f), IronMaterial, false);
            CreateCube(decorations, "Hanging Banner", new Vector3(2.42f, 1.1f, 1.15f), new Vector3(0.58f, 1.35f, 0.035f), BannerMaterial, false);
            CreateSpire(decorations, "Banner Point", new Vector3(2.42f, 0.39f, 1.15f), new Vector3(0.58f, 0.35f, 0.035f), BannerMaterial);

            CreateCube(decorations, "Rubble 1", new Vector3(3.25f, -1.12f, -1.62f), new Vector3(0.42f, 0.3f, 0.38f), DarkStoneMaterial, true, Quaternion.Euler(8f, 14f, 16f));
            CreateCube(decorations, "Rubble 2", new Vector3(3.62f, -1.16f, -1.52f), new Vector3(0.3f, 0.22f, 0.3f), DarkStoneMaterial, true, Quaternion.Euler(-4f, -20f, 9f));
            CreateLantern(decorations, new Vector3(16.5f, -0.05f, -0.45f), false);
        }

        private static void CreateLantern(Transform parent, Vector3 position, bool addLight)
        {
            Transform lantern = new GameObject("Warm Lantern").transform;
            lantern.SetParent(parent, false);
            lantern.localPosition = position;

            CreateCube(lantern, "Lantern Top", new Vector3(0f, 0.2f, 0f), new Vector3(0.32f, 0.055f, 0.24f), IronMaterial, false);
            CreateCube(lantern, "Lantern Bottom", new Vector3(0f, -0.2f, 0f), new Vector3(0.32f, 0.055f, 0.24f), IronMaterial, false);
            CreateCube(lantern, "Lantern Glow", Vector3.zero, new Vector3(0.2f, 0.34f, 0.14f), WarmMaterial, false);

            if (!addLight)
            {
                return;
            }

            Light light = lantern.gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.48f, 0.17f);
            light.intensity = 2.1f;
            light.range = 4f;
            light.shadows = LightShadows.None;
            light.renderMode = LightRenderMode.ForcePixel;
        }

        private static void CreateRain()
        {
            GameObject rainObject = new("Light Rain");
            rainObject.transform.position = new Vector3(22f, 5f, -1f);
            ParticleSystem rain = rainObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = rain.main;
            main.loop = true;
            main.prewarm = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.2f, 3.2f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.018f, 0.032f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.48f, 0.58f, 0.78f, 0.2f), new Color(0.66f, 0.7f, 0.88f, 0.36f));
            main.maxParticles = 70;

            ParticleSystem.EmissionModule emission = rain.emission;
            emission.rateOverTime = 18f;

            ParticleSystem.ShapeModule shape = rain.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(85f, 0.2f, 2f);

            ParticleSystem.VelocityOverLifetimeModule velocity = rain.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = -0.35f;
            velocity.y = -3.2f;

            ParticleSystemRenderer renderer = rain.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.06f;
            renderer.lengthScale = 1.8f;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = RainMaterial;
            rain.Play();
        }

        private static void CreateBattlements(Transform parent, string prefix, float centerX, float y, float z, float width, Material material, float spacing = 0.48f)
        {
            int count = Mathf.Max(2, Mathf.FloorToInt(width / spacing));
            float step = width / count;
            float start = centerX - width * 0.5f + step * 0.5f;
            for (int i = 0; i < count; i += 2)
            {
                CreateCube(parent, $"{prefix} Crenel {i + 1}", new Vector3(start + i * step, y, z), new Vector3(step * 0.72f, 0.35f, 0.72f), material, false);
            }
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 position, Vector3 scale, Material material, bool shadows, Quaternion? rotation = null)
        {
            GameObject cube = new(name, typeof(MeshFilter), typeof(MeshRenderer));
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = position;
            cube.transform.localRotation = rotation ?? Quaternion.identity;
            cube.transform.localScale = scale;
            cube.GetComponent<MeshFilter>().sharedMesh = CubeMesh;
            ConfigureRenderer(cube.GetComponent<MeshRenderer>(), material, shadows);
            return cube;
        }

        private static void CreateSpire(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject spire = new(name, typeof(MeshFilter), typeof(MeshRenderer));
            spire.transform.SetParent(parent, false);
            spire.transform.localPosition = position;
            spire.transform.localScale = scale;
            spire.GetComponent<MeshFilter>().sharedMesh = SpireMesh;
            ConfigureRenderer(spire.GetComponent<MeshRenderer>(), material, false);
        }

        private static void ConfigureRenderer(MeshRenderer renderer, Material material, bool shadows)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            renderer.receiveShadows = shadows;
        }

        private static void SetTextureTiling(GameObject target, Vector2 tiling)
        {
            MaterialPropertyBlock properties = new();
            properties.SetVector("_BaseMap_ST", new Vector4(tiling.x, tiling.y, 0f, 0f));
            properties.SetVector("_BumpMap_ST", new Vector4(tiling.x, tiling.y, 0f, 0f));
            properties.SetVector("_OcclusionMap_ST", new Vector4(tiling.x, tiling.y, 0f, 0f));
            target.GetComponent<MeshRenderer>().SetPropertyBlock(properties);
        }

        private static Material CreateUnlitMaterial(string name, Color color, bool emissive = false)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            Material material = new(shader) { name = name };
            material.SetColor("_BaseColor", color);
            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2.2f);
            }

            return material;
        }

        private static Material CreateBarricadeWoodMaterial()
        {
            Material material = CreateUnlitMaterial("Weathered Barricade Wood", Color.white);
            material.SetTexture("_BaseMap", BarricadeWoodTexture);
            return material;
        }

        private static Texture2D CreateBarricadeWoodTexture()
        {
            const int size = 64;
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
            {
                name = "Generated Weathered Wood",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Repeat
            };

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                bool plankSeam = y % 16 < 2;
                for (int x = 0; x < size; x++)
                {
                    float grain = Mathf.Sin(x * 0.42f + Mathf.Sin(y * 0.31f) * 2f) * 0.06f;
                    float knot = Mathf.PerlinNoise(x * 0.12f, y * 0.08f) * 0.12f;
                    pixels[y * size + x] = plankSeam
                        ? new Color(0.12f, 0.055f, 0.025f)
                        : new Color(0.52f + grain + knot, 0.25f + grain * 0.45f, 0.075f);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static Mesh CubeMesh => cubeMesh ??= Resources.GetBuiltinResource<Mesh>("Cube.fbx");

        private static Mesh SpireMesh
        {
            get
            {
                if (spireMesh != null)
                {
                    return spireMesh;
                }

                spireMesh = new Mesh { name = "Gothic Spire" };
                spireMesh.vertices = new[]
                {
                    new Vector3(-0.5f, 0f, -0.5f),
                    new Vector3(0.5f, 0f, -0.5f),
                    new Vector3(0.5f, 0f, 0.5f),
                    new Vector3(-0.5f, 0f, 0.5f),
                    new Vector3(0f, 1f, 0f)
                };
                spireMesh.triangles = new[]
                {
                    0, 4, 1,
                    1, 4, 2,
                    2, 4, 3,
                    3, 4, 0,
                    0, 1, 2,
                    0, 2, 3
                };
                spireMesh.RecalculateNormals();
                spireMesh.RecalculateBounds();
                return spireMesh;
            }
        }

        private static Material PlatformSideMaterial => platformSideMaterial ??= Resources.Load<Material>(
            "Environment/GothicFortress/Materials/WeatheredStoneSides");

        private static Material PlatformTopMaterial => platformTopMaterial ??= Resources.Load<Material>(
            "Environment/GothicFortress/Materials/WeatheredStoneTops");

        private static Material DarkStoneMaterial => darkStoneMaterial ??= Resources.Load<Material>(
            "Environment/GothicFortress/Materials/DarkFortressStone");

        private static Material IronMaterial => ironMaterial ??= Resources.Load<Material>(
            "Environment/GothicFortress/Materials/BlackenedIron");
        private static Material DistantMaterial => distantMaterial ??= CreateUnlitMaterial("Fogged Distant Fortress", DistantStone);
        private static Material NearMaterial => nearMaterial ??= CreateUnlitMaterial("Near Fortress Silhouette", NearStone);
        private static Material BannerMaterial => bannerMaterial ??= CreateUnlitMaterial("Faded Oxblood Banner", new Color(0.24f, 0.055f, 0.075f));
        private static Material BarricadeWoodMaterial => barricadeWoodMaterial ??= CreateBarricadeWoodMaterial();
        private static Texture2D BarricadeWoodTexture => barricadeWoodTexture ??= CreateBarricadeWoodTexture();
        private static Material WarmMaterial => warmMaterial ??= CreateUnlitMaterial("Lantern Glow", new Color(1f, 0.34f, 0.08f), true);
        private static Material RainMaterial => rainMaterial ??= Resources.Load<Material>("Environment/GothicFortress/Materials/Rain");
    }
}
