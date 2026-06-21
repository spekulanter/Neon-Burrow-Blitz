using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class NeonBurrowSpritePrepTool
{
    const string RawPath = "Assets/Art/Raw";
    const string ProcessedPath = "Assets/Art/Processed";
    const int PlayerFrameSize = 320;
    const int ItemFrameSize = 320;

    static readonly string[] Folders =
    {
        RawPath, ProcessedPath, "Assets/Art/Processed/Players/Rix", "Assets/Art/Processed/Players/Nova",
        "Assets/Art/Processed/Items", "Assets/Animations/Rix", "Assets/Animations/Nova",
        "Assets/AnimatorControllers", "Assets/Prefabs/Player", "Assets/Prefabs/Weapons",
        "Assets/Prefabs/Items", "Assets/Prefabs/Enemies", "Assets/Prefabs/Level", "Assets/Materials",
        "Assets/Scenes", "Assets/Scripts/Core", "Assets/Scripts/Player", "Assets/Scripts/Weapons",
        "Assets/Scripts/Enemies", "Assets/Scripts/Level", "Assets/Scripts/UI", "Assets/Scripts/Editor"
    };

    [MenuItem("Tools/Neon Burrow/Prepare Project")]
    public static void PrepareProject()
    {
        CreateFolders();
        ConfigureRawImports();
        GenerateProcessedSprites();
        AssetDatabase.Refresh();
        ConfigureProcessedImports();
        BuildAnimationsAndControllers();
        BuildPrefabsAndScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Neon Burrow project preparation complete.");
    }

    [MenuItem("Tools/Neon Burrow/Prepare Imported Art")]
    public static void PrepareImportedArt()
    {
        CreateFolders();
        ConfigureRawImports();
        GenerateProcessedSprites();
        AssetDatabase.Refresh();
        ConfigureProcessedImports();
        BuildAnimationsAndControllers();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Neon Burrow art preparation complete.");
    }

    static void CreateFolders()
    {
        foreach (string folder in Folders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
                Directory.CreateDirectory(ProjectPath(folder));
        }
    }

    static void ConfigureRawImports()
    {
        foreach (string path in Directory.GetFiles(ProjectPath(RawPath), "*.png", SearchOption.TopDirectoryOnly))
            ConfigureTextureImport(ToAssetPath(path), path.Contains("Background") ? SpriteImportMode.Single : SpriteImportMode.Multiple);
    }

    static void ConfigureProcessedImports()
    {
        foreach (string path in Directory.GetFiles(ProjectPath(ProcessedPath), "*.png", SearchOption.AllDirectories))
            ConfigureTextureImport(ToAssetPath(path), SpriteImportMode.Single);
    }

    static void ConfigureTextureImport(string assetPath, SpriteImportMode mode)
    {
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = mode;
        importer.spritePixelsPerUnit = 100f;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = assetPath.Contains("Background") ? 2048 : 4096;
        importer.SaveAndReimport();
    }

    static void GenerateProcessedSprites()
    {
        GeneratePlayer("Player1_Rix_SpriteSheet.png", "Rix", "Assets/Art/Processed/Players/Rix");
        GeneratePlayer("Player2_Nova_SpriteSheet.png", "Nova", "Assets/Art/Processed/Players/Nova");
        GenerateItems();
    }

    static void GeneratePlayer(string sourceName, string prefix, string outputFolder)
    {
        Texture2D source = LoadTexture($"{RawPath}/{sourceName}");
        if (source == null)
            return;

        var specs = new List<(string anim, int row, int count)>
        {
            ("Idle", 0, 4),
            ("Run", 1, 8),
            ("Jump", 2, 4),
            ("Crouch", 3, 4),
            ("Shoot", 4, 4)
        };

        foreach (var spec in specs)
        {
            for (int col = 0; col < spec.count; col++)
            {
                RectInt cell = NormalizedCell(source.width, source.height, 8, 5, col, spec.row);
                Texture2D frame = ExtractFrame(source, cell, PlayerFrameSize);
                SaveTexture(frame, $"{outputFolder}/{prefix}_{spec.anim}_{col:00}.png");
            }
        }
    }

    static void GenerateItems()
    {
        Texture2D source = LoadTexture($"{RawPath}/Level01_Items_SpriteSheet.png");
        if (source == null)
            return;

        string[,] names =
        {
            { "CrystalShard", "BigCrystal", "HealthCell", "RocketAmmo", "ExtraLife" },
            { "SparkRocketPickup", "CheckpointInactive", "CheckpointActive", "SpikeCrystalHazard", "ExitTeleporter" }
        };

        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                RectInt cell = NormalizedCell(source.width, source.height, 5, 2, col, row);
                Texture2D frame = ExtractFrame(source, cell, ItemFrameSize);
                SaveTexture(frame, $"Assets/Art/Processed/Items/{names[row, col]}.png");
            }
        }
    }

    static Texture2D ExtractFrame(Texture2D source, RectInt cell, int canvasSize)
    {
        Color32[] sourcePixels = source.GetPixels32();
        bool[,] mask = new bool[cell.width, cell.height];
        RectInt bounds = new RectInt(cell.width, cell.height, 0, 0);

        for (int y = 0; y < cell.height; y++)
        {
            for (int x = 0; x < cell.width; x++)
            {
                Color32 c = sourcePixels[(cell.y + y) * source.width + cell.x + x];
                bool foreground = IsForegroundSeed(c);
                mask[x, y] = foreground;
                if (foreground)
                    ExpandBounds(ref bounds, x, y);
            }
        }

        if (bounds.width <= 0 || bounds.height <= 0)
            return TransparentTexture(canvasSize, canvasSize);

        bounds = PadBounds(bounds, 12, cell.width, cell.height);
        Texture2D output = TransparentTexture(canvasSize, canvasSize);
        int offsetX = (canvasSize - bounds.width) / 2;
        int offsetY = 16;
        if (bounds.height + offsetY > canvasSize)
            offsetY = Mathf.Max(0, (canvasSize - bounds.height) / 2);

        for (int y = 0; y < bounds.height && y + offsetY < canvasSize; y++)
        {
            for (int x = 0; x < bounds.width && x + offsetX < canvasSize; x++)
            {
                int sx = bounds.x + x;
                int sy = bounds.y + y;
                Color32 c = sourcePixels[(cell.y + sy) * source.width + cell.x + sx];
                if (IsForegroundPixel(c))
                    output.SetPixel(x + offsetX, y + offsetY, c);
            }
        }

        AlphaBleed(output, 6);
        output.Apply();
        return output;
    }

    static bool IsForegroundSeed(Color32 c)
    {
        if (c.a < 20)
            return false;

        int max = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
        int min = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
        bool saturated = max - min > 24;
        bool dark = max < 210;
        return (saturated || dark) && !IsCheckerOrWhiteBackground(c);
    }

    static bool IsForegroundPixel(Color32 c)
    {
        if (c.a < 20)
            return false;
        return !IsCheckerOrWhiteBackground(c);
    }

    static bool IsCheckerOrWhiteBackground(Color32 c)
    {
        int max = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
        int min = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
        bool neutral = max - min < 12;
        return neutral && max > 216;
    }

    static void AlphaBleed(Texture2D texture, int iterations)
    {
        int width = texture.width;
        int height = texture.height;
        for (int i = 0; i < iterations; i++)
        {
            Color[] copy = texture.GetPixels();
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    Color c = copy[y * width + x];
                    if (c.a > 0.01f)
                        continue;

                    Color neighbor = Color.clear;
                    bool found = false;
                    for (int oy = -1; oy <= 1 && !found; oy++)
                    {
                        for (int ox = -1; ox <= 1; ox++)
                        {
                            Color n = copy[(y + oy) * width + x + ox];
                            if (n.a > 0.1f)
                            {
                                neighbor = new Color(n.r, n.g, n.b, 0f);
                                found = true;
                                break;
                            }
                        }
                    }
                    if (found)
                        texture.SetPixel(x, y, neighbor);
                }
            }
            texture.Apply();
        }
    }

    public static void BuildAnimationsAndControllers()
    {
        BuildPlayerAnimations("Rix", "Assets/Art/Processed/Players/Rix", "Assets/Animations/Rix", "Assets/AnimatorControllers/Rix.controller");
        BuildPlayerAnimations("Nova", "Assets/Art/Processed/Players/Nova", "Assets/Animations/Nova", "Assets/AnimatorControllers/Nova.controller");
    }

    static void BuildPlayerAnimations(string prefix, string spriteFolder, string animationFolder, string controllerPath)
    {
        var clips = new Dictionary<string, AnimationClip>
        {
            ["Idle"] = CreateClip(prefix, "Idle", 1, 1f, true, spriteFolder, animationFolder, "Idle"),
            ["Run"] = CreateFilteredClip(prefix, "Run", 8, 12f, true, spriteFolder, animationFolder),
            ["Jump"] = CreateClip(prefix, "Jump", 1, 1f, false, spriteFolder, animationFolder, "Idle"),
            ["Crouch"] = CreateClip(prefix, "Crouch", 1, 1f, false, spriteFolder, animationFolder, "Idle"),
            ["Shoot"] = CreateClip(prefix, "Shoot", 1, 1f, false, spriteFolder, animationFolder, "Idle")
        };

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        EnsureParameter(controller, "Speed", AnimatorControllerParameterType.Float);
        EnsureParameter(controller, "IsGrounded", AnimatorControllerParameterType.Bool);
        EnsureParameter(controller, "VerticalVelocity", AnimatorControllerParameterType.Float);
        EnsureParameter(controller, "IsCrouching", AnimatorControllerParameterType.Bool);
        EnsureParameter(controller, "Shoot", AnimatorControllerParameterType.Trigger);
        EnsureParameter(controller, "Hurt", AnimatorControllerParameterType.Trigger);
        EnsureParameter(controller, "IsDead", AnimatorControllerParameterType.Bool);

        var machine = controller.layers[0].stateMachine;
        foreach (var state in machine.states)
            machine.RemoveState(state.state);

        var idle = machine.AddState("Idle");
        idle.motion = clips["Idle"];
        machine.defaultState = idle;
        var run = machine.AddState("Run");
        run.motion = clips["Run"];
        var jump = machine.AddState("Jump");
        jump.motion = clips["Jump"];
        var crouch = machine.AddState("Crouch");
        crouch.motion = clips["Crouch"];
        var shoot = machine.AddState("Shoot");
        shoot.motion = clips["Shoot"];

        AddTransition(idle, run, "Speed", AnimatorConditionMode.Greater, 0.1f);
        AddTransition(run, idle, "Speed", AnimatorConditionMode.Less, 0.1f);
        AddTransition(idle, jump, "IsGrounded", AnimatorConditionMode.IfNot, 0f);
        AddTransition(run, jump, "IsGrounded", AnimatorConditionMode.IfNot, 0f);
        AddTransition(jump, idle, "IsGrounded", AnimatorConditionMode.If, 0f);
        AddTransition(idle, crouch, "IsCrouching", AnimatorConditionMode.If, 0f);
        AddTransition(run, crouch, "IsCrouching", AnimatorConditionMode.If, 0f);
        AddTransition(crouch, idle, "IsCrouching", AnimatorConditionMode.IfNot, 0f);
        var shootTransition = machine.AddAnyStateTransition(shoot);
        shootTransition.AddCondition(AnimatorConditionMode.If, 0f, "Shoot");
        shootTransition.hasExitTime = false;
        shootTransition.duration = 0.03f;
        AddExitTransition(shoot, idle);
    }

    static AnimationClip CreateClip(string prefix, string anim, int count, float fps, bool loop, string spriteFolder, string animationFolder, string sourceAnim = null)
    {
        sourceAnim ??= anim;
        var spritePaths = new List<string>();
        for (int i = 0; i < count; i++)
            spritePaths.Add($"{spriteFolder}/{prefix}_{sourceAnim}_{i:00}.png");
        return CreateClipFromSprites(prefix, anim, spritePaths, fps, loop, animationFolder);
    }

    static AnimationClip CreateFilteredClip(string prefix, string anim, int count, float fps, bool loop, string spriteFolder, string animationFolder)
    {
        var spritePaths = new List<string>();
        for (int i = 0; i < count; i++)
        {
            string path = $"{spriteFolder}/{prefix}_{anim}_{i:00}.png";
            if (IsUsablePlayerFrame(path))
                spritePaths.Add(path);
        }

        if (spritePaths.Count == 0)
            spritePaths.Add($"{spriteFolder}/{prefix}_Idle_00.png");
        return CreateClipFromSprites(prefix, anim, spritePaths, fps, loop, animationFolder);
    }

    static AnimationClip CreateClipFromSprites(string prefix, string anim, List<string> spritePaths, float fps, bool loop, string animationFolder)
    {
        var clip = new AnimationClip { frameRate = fps };
        var frames = new ObjectReferenceKeyframe[spritePaths.Count];
        for (int i = 0; i < spritePaths.Count; i++)
        {
            frames[i] = new ObjectReferenceKeyframe
            {
                time = i / fps,
                value = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[i])
            };
        }

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        string path = $"{animationFolder}/{prefix}_{anim}.anim";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    static bool IsUsablePlayerFrame(string assetPath)
    {
        Texture2D texture = LoadTexture(assetPath);
        if (texture == null)
            return false;

        Color32[] pixels = texture.GetPixels32();
        int minX = texture.width;
        int minY = texture.height;
        int maxX = -1;
        int maxY = -1;
        int opaque = 0;
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                if (pixels[y * texture.width + x].a <= 10)
                    continue;
                opaque++;
                minX = Mathf.Min(minX, x);
                minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);
            }
        }

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;
        return opaque >= 5000 && width >= 90 && height >= 120;
    }

    public static void BuildPrefabsAndScenes()
    {
        CreateLayersAndSortingLayers();
        var pulse = CreateWeaponData("Pulse Blaster", false, 1, 0.18f, 18f, 1, 0f);
        var splitter = CreateWeaponData("Crystal Splitter", false, 1, 0.34f, 15f, 3, 18f);
        var lance = CreateWeaponData("Plasma Lance", false, 2, 0.42f, 26f, 1, 0f);
        var rocket = CreateWeaponData("Spark Rocket", true, 3, 0.6f, 13f, 1, 0f);
        var pulsePrefab = CreateProjectilePrefab("Projectile_Pulse", new Color(0.1f, 0.9f, 1f), 0.28f, 0.18f);
        var splitterPrefab = CreateProjectilePrefab("Projectile_CrystalSplitter", new Color(0.95f, 0.25f, 1f), 0.22f, 0.15f);
        var lancePrefab = CreateProjectilePrefab("Projectile_PlasmaLance", new Color(0.1f, 1f, 0.45f), 0.42f, 0.25f);
        var rocketPrefab = CreateProjectilePrefab("Projectile_Rocket", new Color(1f, 0.55f, 0.05f), 0.46f, 0.28f);
        var muzzle = CreateMuzzleFlashPrefab();
        pulse.projectilePrefab = pulsePrefab;
        splitter.projectilePrefab = splitterPrefab;
        lance.projectilePrefab = lancePrefab;
        rocket.projectilePrefab = rocketPrefab;
        pulse.muzzleFlashPrefab = muzzle;
        splitter.muzzleFlashPrefab = muzzle;
        lance.muzzleFlashPrefab = muzzle;
        rocket.muzzleFlashPrefab = muzzle;
        EditorUtility.SetDirty(pulse);
        EditorUtility.SetDirty(splitter);
        EditorUtility.SetDirty(lance);
        EditorUtility.SetDirty(rocket);

        var weaponList = new[] { pulse, splitter, lance, rocket };
        var rix = CreatePlayerPrefab("Player_Rix", PlayerSlot.Player1, "Rix", "Assets/AnimatorControllers/Rix.controller", pulse, rocket, weaponList);
        var nova = CreatePlayerPrefab("Player_Nova", PlayerSlot.Player2, "Nova", "Assets/AnimatorControllers/Nova.controller", pulse, rocket, weaponList);
        CreateItemPrefabs();
        CreateEnemyPrefabs();
        CreateBackgroundPrefab();
        CreateMainMenuScene();
        CreateLevelScene(rix, nova);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level_01_CrystalBurrow.unity", true)
        };
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
    }

    static WeaponData CreateWeaponData(string name, bool usesAmmo, int damage, float fireRate, float speed, int projectileCount, float spreadAngle)
    {
        string path = $"Assets/Prefabs/Weapons/{name.Replace(' ', '_')}.asset";
        AssetDatabase.DeleteAsset(path);
        var data = ScriptableObject.CreateInstance<WeaponData>();
        data.weaponName = name;
        data.usesAmmo = usesAmmo;
        data.damage = damage;
        data.fireRate = fireRate;
        data.projectileSpeed = speed;
        data.projectileLifetime = usesAmmo ? 3f : 2f;
        data.projectileCount = projectileCount;
        data.spreadAngle = spreadAngle;
        AssetDatabase.CreateAsset(data, path);
        return data;
    }

    static GameObject CreateProjectilePrefab(string name, Color color, float size, float trailWidth)
    {
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSolidSprite(name + "_Sprite", color);
        sr.sortingLayerName = "Projectiles";
        sr.sortingOrder = 40;
        go.transform.localScale = Vector3.one * size;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        var trail = go.AddComponent<TrailRenderer>();
        trail.time = 0.18f;
        trail.startWidth = trailWidth;
        trail.endWidth = 0f;
        trail.material = GetGeneratedMaterial(name + "_Trail_Material", color);
        trail.startColor = color;
        trail.endColor = new Color(color.r, color.g, color.b, 0f);
        trail.sortingLayerName = "Projectiles";
        trail.sortingOrder = 39;
        go.AddComponent<Projectile>();
        string path = $"Assets/Prefabs/Weapons/{name}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        UnityEngine.Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreateMuzzleFlashPrefab()
    {
        var go = new GameObject("MuzzleFlash");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSolidSprite("MuzzleFlash_Sprite", new Color(0.25f, 1f, 0.85f, 0.9f));
        sr.sortingLayerName = "Projectiles";
        sr.sortingOrder = 45;
        go.transform.localScale = new Vector3(0.55f, 0.32f, 1f);
        var effect = go.AddComponent<ExplosionEffect>();
        effect.lifetime = 0.12f;
        effect.growSpeed = 4f;
        return SavePrefab(go, "Assets/Prefabs/Weapons/MuzzleFlash.prefab");
    }

    static GameObject CreatePlayerPrefab(string name, PlayerSlot slot, string prefix, string controllerPath, WeaponData pulse, WeaponData rocket, WeaponData[] weaponList)
    {
        var go = new GameObject(name);
        go.layer = LayerMask.NameToLayer("Player");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/Processed/Players/{prefix}/{prefix}_Idle_00.png");
        sr.sortingLayerName = "Player";
        sr.sortingOrder = 30;
        var animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 4f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var capsule = go.AddComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(1f, 1.8f);
        capsule.offset = new Vector2(0f, 0.9f);
        var controller = go.AddComponent<PlayerController2D>();
        controller.input.playerSlot = slot;
        controller.groundMask = LayerMask.GetMask("Ground");
        go.AddComponent<PlayerHealth>();
        go.AddComponent<PlayerRespawn>();
        var weapon = go.AddComponent<WeaponController>();
        weapon.pulseBlaster = pulse;
        weapon.sparkRocket = rocket;
        weapon.weapons = weaponList;
        go.AddComponent<AudioSource>();

        var ground = new GameObject("GroundCheck").transform;
        ground.SetParent(go.transform);
        ground.localPosition = new Vector3(0f, -0.08f, 0f);
        controller.groundCheck = ground;
        var fire = new GameObject("FirePoint").transform;
        fire.SetParent(go.transform);
        fire.localPosition = new Vector3(0.9f, 1.05f, 0f);
        weapon.firePoint = fire;

        string path = $"Assets/Prefabs/Player/{name}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        UnityEngine.Object.DestroyImmediate(go);
        return prefab;
    }

    static void CreateItemPrefabs()
    {
        CreatePickupPrefab("Pickup_CrystalShard", "CrystalShard", PickupType.CrystalShard);
        CreatePickupPrefab("Pickup_BigCrystal", "BigCrystal", PickupType.BigCrystal);
        CreatePickupPrefab("Pickup_HealthCell", "HealthCell", PickupType.HealthCell);
        CreatePickupPrefab("Pickup_RocketAmmo", "RocketAmmo", PickupType.RocketAmmo);
        CreatePickupPrefab("Pickup_ExtraLife", "ExtraLife", PickupType.ExtraLife);
        CreatePickupPrefab("Pickup_SparkRocket", "SparkRocketPickup", PickupType.SparkRocket);
        CreateCheckpointPrefab();
        CreateSimpleTriggerPrefab("SpikeCrystalHazard", "SpikeCrystalHazard", typeof(Hazard), "Hazard");
        CreateSimpleTriggerPrefab("ExitTeleporter", "ExitTeleporter", typeof(LevelExit), "Default");
    }

    static void CreatePickupPrefab(string prefabName, string spriteName, PickupType type)
    {
        var go = ItemBase(prefabName, spriteName, "Pickup");
        var pickup = go.AddComponent<Pickup>();
        pickup.pickupType = type;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        SavePrefab(go, $"Assets/Prefabs/Items/{prefabName}.prefab");
    }

    static void CreateCheckpointPrefab()
    {
        var go = ItemBase("Checkpoint", "CheckpointInactive", "Default");
        var cp = go.AddComponent<Checkpoint>();
        cp.inactiveSprite = ItemSprite("CheckpointInactive");
        cp.activeSprite = ItemSprite("CheckpointActive");
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        SavePrefab(go, "Assets/Prefabs/Items/Checkpoint.prefab");
    }

    static void CreateSimpleTriggerPrefab(string prefabName, string spriteName, Type componentType, string layer)
    {
        var go = ItemBase(prefabName, spriteName, layer);
        go.AddComponent(componentType);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        SavePrefab(go, $"Assets/Prefabs/Items/{prefabName}.prefab");
    }

    static GameObject ItemBase(string name, string spriteName, string layer)
    {
        var go = new GameObject(name);
        go.layer = LayerMask.NameToLayer(layer);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = ItemSprite(spriteName);
        sr.sortingLayerName = name.Contains("Hazard") ? "Ground" : "Pickups";
        sr.sortingOrder = 10;
        return go;
    }

    static Sprite ItemSprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/Processed/Items/{name}.png");

    static void CreateEnemyPrefabs()
    {
        var beetle = new GameObject("BeetleBot");
        beetle.layer = LayerMask.NameToLayer("Enemy");
        beetle.transform.localScale = new Vector3(1.5f, 0.9f, 1f);
        var sr = beetle.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSolidSprite("BeetleBot_Sprite", new Color(1f, 0.85f, 0.05f));
        sr.sortingLayerName = "Enemies";
        sr.sortingOrder = 20;
        var rb = beetle.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        beetle.AddComponent<BoxCollider2D>();
        beetle.AddComponent<EnemyHealth>();
        var patrol = beetle.AddComponent<PatrolEnemy>();
        patrol.groundMask = LayerMask.GetMask("Ground");
        var gp = new GameObject("GroundProbe").transform;
        gp.SetParent(beetle.transform);
        gp.localPosition = new Vector3(0.55f, -0.45f, 0f);
        patrol.groundProbe = gp;
        var wp = new GameObject("WallProbe").transform;
        wp.SetParent(beetle.transform);
        wp.localPosition = new Vector3(0.6f, 0f, 0f);
        patrol.wallProbe = wp;
        SavePrefab(beetle, "Assets/Prefabs/Enemies/BeetleBot.prefab");

        var turret = new GameObject("SporeTurret");
        turret.layer = LayerMask.NameToLayer("Enemy");
        turret.transform.localScale = new Vector3(1.25f, 1.6f, 1f);
        var tsr = turret.AddComponent<SpriteRenderer>();
        tsr.sprite = CreateSolidSprite("SporeTurret_Sprite", new Color(1f, 0.15f, 0.65f));
        tsr.sortingLayerName = "Enemies";
        tsr.sortingOrder = 20;
        turret.AddComponent<BoxCollider2D>();
        var eh = turret.AddComponent<EnemyHealth>();
        eh.maxHealth = 3;
        eh.scoreOnDeath = 100;
        var te = turret.AddComponent<TurretEnemy>();
        te.projectilePrefab = CreateEnemyProjectilePrefab();
        SavePrefab(turret, "Assets/Prefabs/Enemies/SporeTurret.prefab");
    }

    static GameObject CreateEnemyProjectilePrefab()
    {
        var go = new GameObject("EnemyProjectile");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSolidSprite("EnemyProjectile_Sprite", new Color(0.5f, 1f, 0.1f));
        sr.sortingLayerName = "Projectiles";
        go.transform.localScale = Vector3.one * 0.18f;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        go.AddComponent<EnemyProjectile>();
        return SavePrefab(go, "Assets/Prefabs/Enemies/EnemyProjectile.prefab");
    }

    static void CreateBackgroundPrefab()
    {
        ConfigureTextureImport($"{RawPath}/Level01_CrystalBurrow_Background.png", SpriteImportMode.Single);
        var go = new GameObject("Background_CrystalBurrow");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{RawPath}/Level01_CrystalBurrow_Background.png");
        sr.sortingLayerName = "Background";
        sr.sortingOrder = -100;
        go.AddComponent<ParallaxBackground>().parallaxFactor = 0.12f;
        SavePrefab(go, "Assets/Prefabs/Level/Background_CrystalBurrow.prefab");
    }

    static void CreateMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("GameSession").AddComponent<GameSession>();
        new GameObject("SceneLoader").AddComponent<SceneLoader>();
        var cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        cam.AddComponent<Camera>().orthographic = true;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        CreateDirectionalLight();

        var canvas = CreateCanvas("MainMenuCanvas");
        var title = CreateText(canvas.transform, "NEON BURROW BLITZ", 44, new Vector2(0f, 170f), new Vector2(720f, 70f));
        title.color = new Color(0.2f, 1f, 0.9f);
        var menu = canvas.AddComponent<MainMenuUI>();
        menu.bestScoreText = CreateText(canvas.transform, "Best Score: 0", 22, new Vector2(0f, 80f), new Vector2(400f, 36f));
        menu.bestTimeText = CreateText(canvas.transform, "Best Time: --", 22, new Vector2(0f, 40f), new Vector2(400f, 36f));
        CreateButton(canvas.transform, "Start 1 Player", new Vector2(0f, -30f), menu.StartOnePlayer);
        CreateButton(canvas.transform, "Start 2 Players", new Vector2(0f, -95f), menu.StartTwoPlayers);
        CreateButton(canvas.transform, "Quit", new Vector2(0f, -160f), menu.Quit);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
    }

    static void CreateLevelScene(GameObject rixPrefab, GameObject novaPrefab)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("GameSession").AddComponent<GameSession>();
        new GameObject("SceneLoader").AddComponent<SceneLoader>();
        var manager = new GameObject("GameManager").AddComponent<GameManager>();
        CreateDirectionalLight();
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6.5f;
        camGo.transform.position = new Vector3(4f, 7f, -10f);
        camGo.AddComponent<AudioListener>();
        camGo.AddComponent<CameraFollow2D>();

        GameObject levelBuilderGo = new GameObject("LevelBuilder");
        var builder = levelBuilderGo.AddComponent<LevelBuilder>();
        builder.player1Prefab = rixPrefab;
        builder.player2Prefab = novaPrefab;
        builder.player1Spawn = CreateMarker("Player1Spawn", new Vector3(4f, 1.52f, 0f)).transform;
        builder.player2Spawn = CreateMarker("Player2Spawn", new Vector3(5.6f, 1.52f, 0f)).transform;

        BuildBackgrounds();
        BuildPlatforms();
        PlaceGameplayObjects();
        CreateHudAndPanels(manager);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Level_01_CrystalBurrow.unity");
    }

    static void BuildBackgrounds()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Level/Background_CrystalBurrow.prefab");
        float tileWidth = 50f;
        for (int i = 0; i < 6; i++)
        {
            var bg = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            bg.transform.position = new Vector3(i * tileWidth, 8f, 8f);
            bg.transform.localScale = Vector3.one * 3f;
        }
    }

    static void BuildPlatforms()
    {
        CreatePlatform("Left_Extension", new Vector2(24f, 1f), new Vector3(-12f, 1f, 0f));
        CreatePlatform("Left_Wall", new Vector2(1f, 12f), new Vector3(-24.5f, 6f, 0f));
        CreatePlatform("Ground_Start", new Vector2(32f, 1f), new Vector3(16f, 1f, 0f));
        CreatePlatform("Ground_1", new Vector2(30f, 1f), new Vector3(47f, 1f, 0f));
        CreatePlatform("Ground_2", new Vector2(30f, 1f), new Vector3(68f, 1f, 0f));
        CreatePlatform("Ground_3", new Vector2(30f, 1f), new Vector3(97f, 1f, 0f));
        CreatePlatform("Ground_4", new Vector2(32f, 1f), new Vector3(126f, 1f, 0f));
        CreatePlatform("Ground_5", new Vector2(34f, 1f), new Vector3(158f, 1f, 0f));
        CreatePlatform("Ground_End", new Vector2(34f, 1f), new Vector3(190f, 1f, 0f));
        CreatePlatform("Safety_Floor", new Vector2(240f, 0.8f), new Vector3(96f, -3f, 0f));
        CreatePlatform("Platform_A", new Vector2(7f, 0.6f), new Vector3(32f, 5f, 0f));
        CreatePlatform("Platform_B", new Vector2(8f, 0.6f), new Vector3(52f, 8f, 0f));
        CreatePlatform("Platform_C", new Vector2(7f, 0.6f), new Vector3(72f, 6f, 0f));
        CreatePlatform("Platform_D", new Vector2(8f, 0.6f), new Vector3(125f, 6f, 0f));
        CreatePlatform("Platform_E", new Vector2(8f, 0.6f), new Vector3(142f, 10f, 0f));
        CreatePlatform("Platform_F", new Vector2(8f, 0.6f), new Vector3(160f, 14f, 0f));
    }

    static GameObject CreatePlatform(string name, Vector2 size, Vector3 position)
    {
        var go = new GameObject(name);
        go.layer = LayerMask.NameToLayer("Ground");
        go.transform.position = position;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSolidSprite(name + "_Sprite", new Color(0.08f, 0.22f, 0.28f));
        sr.sortingLayerName = "Ground";
        sr.sortingOrder = 2;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var collider = go.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        return go;
    }

    static void PlaceGameplayObjects()
    {
        PlacePrefab("Assets/Prefabs/Items/Pickup_CrystalShard.prefab", new Vector3(8f, 2.3f, 0f));
        PlacePrefab("Assets/Prefabs/Items/Pickup_CrystalShard.prefab", new Vector3(11f, 2.3f, 0f));
        PlacePrefab("Assets/Prefabs/Items/Pickup_SparkRocket.prefab", new Vector3(52f, 9.4f, 0f));
        PlacePrefab("Assets/Prefabs/Items/Pickup_BigCrystal.prefab", new Vector3(57f, 9.4f, 0f));
        PlacePrefab("Assets/Prefabs/Items/Pickup_RocketAmmo.prefab", new Vector3(74f, 7.4f, 0f));
        PlacePrefab("Assets/Prefabs/Items/Checkpoint.prefab", new Vector3(86f, 2.6f, 0f));
        PlacePrefab("Assets/Prefabs/Items/SpikeCrystalHazard.prefab", new Vector3(103f, 2.3f, 0f));
        PlaceAcidPool(new Vector3(114f, 1.7f, 0f), new Vector2(10f, 0.5f));
        PlacePrefab("Assets/Prefabs/Items/Pickup_HealthCell.prefab", new Vector3(137f, 11.5f, 0f));
        PlacePrefab("Assets/Prefabs/Items/ExitTeleporter.prefab", new Vector3(193f, 3f, 0f));
        PlacePrefab("Assets/Prefabs/Enemies/BeetleBot.prefab", new Vector3(36f, 2.2f, 0f));
        PlacePrefab("Assets/Prefabs/Enemies/BeetleBot.prefab", new Vector3(132f, 2.2f, 0f));
        PlacePrefab("Assets/Prefabs/Enemies/SporeTurret.prefab", new Vector3(153f, 2.2f, 0f));
        PlacePrefab("Assets/Prefabs/Enemies/BeetleBot.prefab", new Vector3(178f, 2.2f, 0f));
        PlacePrefab("Assets/Prefabs/Enemies/SporeTurret.prefab", new Vector3(188f, 2.2f, 0f));
    }

    static void PlaceAcidPool(Vector3 position, Vector2 size)
    {
        var go = new GameObject("AcidPool");
        go.layer = LayerMask.NameToLayer("Hazard");
        go.transform.position = position;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSolidSprite("AcidPool_Sprite", new Color(0.25f, 1f, 0.1f, 0.55f));
        sr.sortingLayerName = "Ground";
        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        col.isTrigger = true;
        go.AddComponent<Hazard>();
    }

    static void CreateHudAndPanels(GameManager manager)
    {
        var canvas = CreateCanvas("GameCanvas");
        var hud = canvas.AddComponent<HUDController>();
        hud.scoreText = CreateText(canvas.transform, "Score 0", 20, new Vector2(-500f, 320f), new Vector2(220f, 34f));
        hud.timerText = CreateText(canvas.transform, "0.0s", 20, new Vector2(0f, 320f), new Vector2(160f, 34f));
        hud.player1Text = CreateText(canvas.transform, "P1", 18, new Vector2(-420f, 280f), new Vector2(420f, 34f));
        hud.player2Text = CreateText(canvas.transform, "P2", 18, new Vector2(420f, 280f), new Vector2(420f, 34f));

        var pausePanel = CreatePanel(canvas.transform, "PausePanel", new Vector2(0f, 0f), new Vector2(360f, 260f));
        var pause = canvas.AddComponent<PauseMenu>();
        pause.panel = pausePanel;
        CreateText(pausePanel.transform, "PAUSED", 30, new Vector2(0f, 80f), new Vector2(260f, 48f));
        CreateButton(pausePanel.transform, "Resume", new Vector2(0f, 20f), pause.Resume);
        CreateButton(pausePanel.transform, "Restart Level", new Vector2(0f, -40f), pause.RestartLevel);
        CreateButton(pausePanel.transform, "Main Menu", new Vector2(0f, -100f), pause.MainMenu);
        pausePanel.SetActive(false);

        var completePanel = CreatePanel(canvas.transform, "LevelCompletePanel", new Vector2(0f, 0f), new Vector2(460f, 360f));
        var complete = canvas.AddComponent<LevelCompleteUI>();
        complete.panel = completePanel;
        complete.summaryText = CreateText(completePanel.transform, "", 22, new Vector2(0f, 60f), new Vector2(380f, 180f));
        CreateButton(completePanel.transform, "Restart", new Vector2(-90f, -120f), complete.Restart);
        CreateButton(completePanel.transform, "Main Menu", new Vector2(90f, -120f), complete.MainMenu);
        completePanel.SetActive(false);

        var so = new SerializedObject(manager);
        so.FindProperty("levelCompleteUI").objectReferenceValue = complete;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static GameObject CreateCanvas(string name)
    {
        var canvasGo = new GameObject(name);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.AddComponent<GraphicRaycaster>();
        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        return canvasGo;
    }

    static Text CreateText(Transform parent, string value, int fontSize, Vector2 anchoredPosition, Vector2 size)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
        var text = go.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        return text;
    }

    static void CreateButton(Transform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject(label);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(220f, 48f);
        rect.anchoredPosition = anchoredPosition;
        var image = go.AddComponent<Image>();
        image.color = new Color(0.05f, 0.25f, 0.32f, 0.95f);
        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        UnityEventTools.AddPersistentListener(button.onClick, action);
        EditorUtility.SetDirty(button);
        var text = CreateText(go.transform, label, 20, Vector2.zero, rect.sizeDelta);
        text.raycastTarget = false;
    }

    static GameObject CreatePanel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
        var image = go.AddComponent<Image>();
        image.color = new Color(0.02f, 0.05f, 0.09f, 0.92f);
        return go;
    }

    static GameObject PlacePrefab(string path, Vector3 position)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
            return null;
        var go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        go.transform.position = position;
        return go;
    }

    static GameObject CreateMarker(string name, Vector3 position)
    {
        var go = new GameObject(name);
        go.transform.position = position;
        return go;
    }

    static void CreateDirectionalLight()
    {
        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    static GameObject SavePrefab(GameObject go, string path)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        UnityEngine.Object.DestroyImmediate(go);
        return prefab;
    }

    static Sprite CreateSolidSprite(string name, Color color)
    {
        string path = $"Assets/Art/Processed/{name}.png";
        var tex = new Texture2D(100, 100, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[100 * 100];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        SaveTexture(tex, path);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        ConfigureTextureImport(path, SpriteImportMode.Single);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Material GetGeneratedMaterial(string name, Color color)
    {
        string path = $"Assets/Materials/{name}.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    static void CreateLayersAndSortingLayers()
    {
        AddLayer("Ground");
        AddLayer("Hazard");
        AddLayer("Pickup");
        AddLayer("Enemy");
        AddLayer("Player");
        AddSortingLayer("Background");
        AddSortingLayer("Ground");
        AddSortingLayer("Pickups");
        AddSortingLayer("Enemies");
        AddSortingLayer("Player");
        AddSortingLayer("Projectiles");
        AddSortingLayer("UI");
    }

    static void AddLayer(string layerName)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers = tagManager.FindProperty("layers");
        for (int i = 0; i < layers.arraySize; i++)
        {
            if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
                return;
        }
        for (int i = 8; i < layers.arraySize; i++)
        {
            var element = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(element.stringValue))
            {
                element.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return;
            }
        }
    }

    static void AddSortingLayer(string layerName)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers = tagManager.FindProperty("m_SortingLayers");
        for (int i = 0; i < layers.arraySize; i++)
        {
            if (layers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == layerName)
                return;
        }
        layers.InsertArrayElementAtIndex(layers.arraySize);
        var layer = layers.GetArrayElementAtIndex(layers.arraySize - 1);
        layer.FindPropertyRelative("name").stringValue = layerName;
        layer.FindPropertyRelative("uniqueID").intValue = UnityEngine.Random.Range(1000, int.MaxValue);
        layer.FindPropertyRelative("locked").boolValue = false;
        tagManager.ApplyModifiedProperties();
    }

    static Texture2D LoadTexture(string assetPath)
    {
        string fullPath = ProjectPath(assetPath);
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"Missing texture: {assetPath}");
            return null;
        }
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(File.ReadAllBytes(fullPath));
        return texture;
    }

    static RectInt NormalizedCell(int width, int height, int cols, int rows, int col, int row)
    {
        int x0 = Mathf.RoundToInt(col * width / (float)cols);
        int x1 = Mathf.RoundToInt((col + 1) * width / (float)cols);
        int yTop = Mathf.RoundToInt(row * height / (float)rows);
        int yBottom = Mathf.RoundToInt((row + 1) * height / (float)rows);
        int unityY = height - yBottom;
        return new RectInt(x0, unityY, x1 - x0, yBottom - yTop);
    }

    static Texture2D TransparentTexture(int width, int height)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static void SaveTexture(Texture2D texture, string assetPath)
    {
        string full = ProjectPath(assetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(full));
        File.WriteAllBytes(full, texture.EncodeToPNG());
    }

    static void ExpandBounds(ref RectInt bounds, int x, int y)
    {
        if (bounds.width <= 0 || bounds.height <= 0)
        {
            bounds = new RectInt(x, y, 1, 1);
            return;
        }
        int minX = Mathf.Min(bounds.xMin, x);
        int minY = Mathf.Min(bounds.yMin, y);
        int maxX = Mathf.Max(bounds.xMax, x + 1);
        int maxY = Mathf.Max(bounds.yMax, y + 1);
        bounds = FromMinMax(minX, minY, maxX, maxY);
    }

    static RectInt PadBounds(RectInt bounds, int padding, int maxWidth, int maxHeight)
    {
        int xMin = Mathf.Max(0, bounds.xMin - padding);
        int yMin = Mathf.Max(0, bounds.yMin - padding);
        int xMax = Mathf.Min(maxWidth, bounds.xMax + padding);
        int yMax = Mathf.Min(maxHeight, bounds.yMax + padding);
        return FromMinMax(xMin, yMin, xMax, yMax);
    }

    static void AddTransition(AnimatorState from, AnimatorState to, string parameter, AnimatorConditionMode mode, float threshold)
    {
        var transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.05f;
        transition.AddCondition(mode, threshold, parameter);
    }

    static void AddExitTransition(AnimatorState from, AnimatorState to)
    {
        var transition = from.AddTransition(to);
        transition.hasExitTime = true;
        transition.exitTime = 0.95f;
        transition.duration = 0.03f;
    }

    static void EnsureParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
    {
        foreach (var parameter in controller.parameters)
        {
            if (parameter.name == name)
                return;
        }
        controller.AddParameter(name, type);
    }

    static RectInt FromMinMax(int minX, int minY, int maxX, int maxY)
    {
        return new RectInt(minX, minY, Mathf.Max(0, maxX - minX), Mathf.Max(0, maxY - minY));
    }

    static string ProjectPath(string assetPath)
    {
        return Path.Combine(Directory.GetParent(Application.dataPath).FullName, assetPath).Replace('\\', Path.DirectorySeparatorChar);
    }

    static string ToAssetPath(string fullPath)
    {
        string root = Directory.GetParent(Application.dataPath).FullName.Replace('\\', '/');
        return fullPath.Replace('\\', '/').Replace(root + "/", "");
    }
}
