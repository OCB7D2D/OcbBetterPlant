using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OcbBetterPlantsPatch
{

    public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

    public static TypeReference LoadType(ModuleDefinition module, string ns, string name)
    {
        return new TypeReference("UnityEngine", "Quaternion", null, null);
    }
    public static MethodDefinition InsertDynamicScaleMethod(ModuleDefinition module)
    {

        TypeReference v3 = module.ImportReference(typeof(Vector3));

        TypeDefinition klass = module.Types.First(d => d.Name == "BlockShapeNew");

        MethodDefinition method = new MethodDefinition("DynamicScale",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
            module.ImportReference(typeof(float)));

        method.Parameters.Add(new ParameterDefinition("_drawPos", ParameterAttributes.None, module.ImportReference(typeof(Vector3))));
        method.Parameters.Add(new ParameterDefinition("_blockValue", ParameterAttributes.None, module.GetType("BlockValue")));

        ILProcessor worker = method.Body.GetILProcessor();

        method.Body.Instructions.Add(worker.Create(OpCodes.Ldc_R4, 1f));
        method.Body.Instructions.Add(worker.Create(OpCodes.Ret));

        klass.Methods.Add(method);
        return method;
    }

    public static MethodDefinition InsertDynamicRotationMethod(ModuleDefinition module)
    {

        TypeReference v3 = module.ImportReference(typeof(Vector3));

        TypeDefinition klass = module.Types.First(d => d.Name == "BlockShapeNew");

        MethodDefinition method = new MethodDefinition("DynamicRotation",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
            module.ImportReference(typeof(Quaternion)));

        method.Parameters.Add(new ParameterDefinition("_drawPos", ParameterAttributes.None, module.ImportReference(typeof(Vector3))));
        method.Parameters.Add(new ParameterDefinition("_blockValue", ParameterAttributes.None, module.GetType("BlockValue")));

        ILProcessor worker = method.Body.GetILProcessor();

        var rot = module.GetType("BlockShapeNew").Methods.First(d => d.Name == "GetRotationStatic");
        
        method.Body.Instructions.Add(worker.Create(OpCodes.Ldarga_S, method.Parameters[1])); // blockValue
        method.Body.Instructions.Add(worker.Create(OpCodes.Call, module.GetType(
            "BlockValue").Methods.First(d => d.Name == "get_rotation")));
        method.Body.Instructions.Add(worker.Create(OpCodes.Call, rot));

        method.Body.Instructions.Add(worker.Create(OpCodes.Ret));

        klass.Methods.Add(method);
        return method;
    }

    public static void PatchBlockShapeRotation(
        ILProcessor worker, MethodReference cb,
        Instruction start, Instruction end)
    {

        worker.Remove(start.Next.Next.Next);
        worker.Remove(start.Next.Next);
        worker.Remove(start.Next);
        worker.Remove(start);

        worker.InsertBefore(end, Instruction.Create(OpCodes.Ldarg_3)); // drawPos
        worker.InsertBefore(end, Instruction.Create(OpCodes.Ldarg_2)); // blockValue
        worker.InsertBefore(end, Instruction.Create(OpCodes.Call, cb));

    }

    public static void PatchRenderFace(ModuleDefinition module, MethodReference cb)
    {

        TypeDefinition type = module.Types.First(d => d.Name == "BlockShapeNew");
        MethodDefinition method = type.Methods.First(d => d.Name == "renderFace");

        ILProcessor worker = method.Body.GetILProcessor();

        for (Instruction il = method.Body.Instructions[0]; il != null; il = il.Next)
        {
            Instruction start = il;
            if (il.OpCode != OpCodes.Ldsfld) continue;
            if ((il = il.Next) == null) break;
            if (il.OpCode != OpCodes.Ldarga_S) continue;
            if (!(il.Operand is ParameterDefinition param)) continue;
            if (param.Name != "_blockValue") continue;
            if ((il = il.Next) == null) break;
            if (il.OpCode != OpCodes.Call) continue;
            if (!(il.Operand is MethodDefinition met)) continue;
            if (met.Name != "get_rotation") continue;
            if ((il = il.Next) == null) break;
            if (il.OpCode != OpCodes.Ldelem_Any) continue;
            if (!(il.Operand is TypeReference trf)) continue;
            if (trf.FullName != "UnityEngine.Quaternion") continue;
            if ((il = il.Next) == null) break;
            if (il.OpCode != OpCodes.Stloc_S) continue;
            if (!(il.Operand is VariableDefinition vdf)) continue;
            if (vdf.Index != 28) continue;
            PatchBlockShapeRotation(worker, cb, start, il);
        }

    }



    public static void PatchRenderFaceScaling(ModuleDefinition module, MethodDefinition scale)
    {

        TypeDefinition type = module.Types.First(d => d.Name == "BlockShapeNew");
        MethodDefinition method = type.Methods.First(d => d.Name == "renderFace");

        ILProcessor worker = method.Body.GetILProcessor();

        for (Instruction il = method.Body.Instructions[0]; il != null; il = il.Next)
        {

            if (il.OpCode != OpCodes.Ldfld) continue;
            if (!il.ToString().EndsWith("ldfld System.Collections.Generic.List`1<UnityEngine.Vector3> BlockShapeNew/MySimpleMesh::Vertices")) continue;
            if ((il = il.Next) == null) break;
            if ((il = il.Next) == null) break;
            if (!il.ToString().EndsWith("callvirt !0 System.Collections.Generic.List`1<UnityEngine.Vector3>::get_Item(System.Int32)")) continue;
            if ((il = il.Next) == null) break;
            if ((il = il.Next) == null) break;
            if (!il.ToString().EndsWith("call UnityEngine.Vector3 UnityEngine.Vector3::op_Addition(UnityEngine.Vector3,UnityEngine.Vector3)")) continue;

            worker.InsertBefore(il.Previous, Instruction.Create(OpCodes.Ldarg_3)); // drawPos
            worker.InsertBefore(il.Previous, Instruction.Create(OpCodes.Ldarg_2)); // blockValue
            worker.InsertBefore(il.Previous, worker.Create(OpCodes.Call, scale));

            worker.InsertBefore(il.Previous, worker.Create(OpCodes.Call, module
                .ImportReference(typeof(Vector3).GetMethod("op_Multiply",
                new Type[] { typeof(Vector3), typeof(float) }))));

            break;

        }

    }

    public static void Patch(AssemblyDefinition assembly)
    {

        ModuleDefinition module = assembly.MainModule;

        MethodDefinition rotation = InsertDynamicRotationMethod(module);

        MethodDefinition scale = InsertDynamicScaleMethod(module);

        PatchRenderFace(module, rotation);

        PatchRenderFaceScaling(module, scale);

    }

    // Called after the patching process and after scripts are compiled.
    // Used to link references between both assemblies
    // Return true if successful
    public static bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
        return true;
    }


    // Helper functions to allow us to access and change variables that are otherwise unavailable.
    private static void SetMethodToVirtual(MethodDefinition method)
    {
        method.IsVirtual = true;
    }

    private static TypeDefinition MakeTypePublic(TypeDefinition type)
    {
        foreach (var myField in type.Fields)
        {
            SetFieldToPublic(myField);
        }
        foreach (var myMethod in type.Methods)
        {
            SetMethodToPublic(myMethod);
        }

        return type;
    }

    private static void SetFieldToPublic(FieldDefinition field)
    {
        field.IsFamily = false;
        field.IsPrivate = false;
        field.IsPublic = true;

    }
    private static void SetMethodToPublic(MethodDefinition field, bool force = false)
    {
        // Leave protected virtual methods alone to avoid
        // issues with others inheriting from it, as it gives
        // a compile error when protection level mismatches.
        // Unsure if this changes anything on runtime though?
        if (!field.IsFamily || !field.IsVirtual || force) {
            field.IsFamily = false;
            field.IsPrivate = false;
            field.IsPublic = true;
        }
    }

}
