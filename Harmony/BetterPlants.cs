using OCB;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

public class OcbBetterPlants : IModApi
{

    public static float RotY = 180f;
    public static float RotX = 8f;
    public static float RotZ = 8f;
    public static float ScaleMin = 0.8f;
    public static float ScaleMax = 1.3f;
    public static bool Enabled = true;

    public static readonly ulong Seed00 = StaticRandom.RandomSeed();
    public static readonly ulong Seed01 = StaticRandom.RandomSeed();
    public static readonly ulong Seed02 = StaticRandom.RandomSeed();
    public static readonly ulong Seed03 = StaticRandom.RandomSeed();
    public static readonly ulong Seed04 = StaticRandom.RandomSeed();
    public static readonly ulong Seed05 = StaticRandom.RandomSeed();
    public static readonly ulong Seed06 = StaticRandom.RandomSeed();

    public void InitMod(Mod mod)
    {
        Debug.Log("Loading OCB Better Grass Patch: " + GetType().ToString());

        // Check if BepInEx was loaded and did its job correctly
        if (AccessTools.Method(typeof(BlockShapeNew), "DynamicScale") == null)
        {
            BepInExAutoInstall.TryToInstallBepInEx(mod);
            Log.Warning("Installed necessary BepInEx files in game folder!".ToUpper());
            Log.Warning("Please restart the game for changes to take effect!".ToUpper());
            Log.Error("Features will not be available until game restart!".ToUpper());
            // Application.Quit();
            return;
        }

        new Harmony(GetType().ToString()).PatchAll(Assembly.GetExecutingAssembly());
        ModEvents.GameStartDone.RegisterHandler(ApplyGamePrefs);
    }

    static public void ApplyGamePrefs()
    {
        Enabled = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTerrainQuality) > 0;
    }

    [HarmonyPatch(typeof(MeshDescription))]
    [HarmonyPatch("SetGrassQuality")]
    public class MeshDescription_SetGrassQuality
    {
        static void Postfix()
        {
            ApplyGamePrefs();
        }
    }

    [HarmonyPatch(typeof(BlockShapeNew))]
    [HarmonyPatch("DynamicScale")]
    public class BlockShapeNew_DynamicScale
    {
        static void Postfix(
            Vector3 _drawPos,
            BlockValue _blockValue,
            ref float __result)
        {
            if (Enabled == false) return;
            if (_blockValue.Block.Properties.Values.TryGetString(
                "DynamicRotation", out string dynamicRotation))
            {
                ulong seed = Seed05;
                StaticRandom.HashSeed(ref seed, _drawPos.x);
                StaticRandom.HashSeed(ref seed, _drawPos.y);
                StaticRandom.HashSeed(ref seed, _drawPos.z);
                __result = StaticRandom.RangeSquare(
                    ScaleMin, ScaleMax, seed);
            }
        }
    }

    [HarmonyPatch(typeof(BlockShapeNew))]
    [HarmonyPatch("DynamicRotation")]
    public class BlockShapeNew_DynamicRotation
    {
        static void Postfix(
            Vector3 _drawPos,
            BlockValue _blockValue,
            ref Quaternion __result)
        {
            if (Enabled == false) return;
            if (_blockValue.Block.Properties.Values.TryGetString(
                "DynamicRotation", out string dynamicRotation))
            {
                ulong seed = Seed00;
                StaticRandom.HashSeed(ref seed, _drawPos.x);
                StaticRandom.HashSeed(ref seed, _drawPos.y);
                StaticRandom.HashSeed(ref seed, _drawPos.z);
                var y = StaticRandom.Range(-RotY, RotY, seed);
                StaticRandom.HashSeed(ref seed, Seed01);
                var x = StaticRandom.RangeSquare(-RotX, RotX, seed);
                StaticRandom.HashSeed(ref seed, Seed02);
                var z = StaticRandom.RangeSquare(-RotZ, RotZ, seed);
                __result = Quaternion.Euler(x, y, z) * __result;
            }
        }
    }

}
