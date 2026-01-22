using System;
using System.IO;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace VaporDeobfuscator
{
    class Program
    {
        static string[] attrib =
        {
            "YanoAttribute",
            "Xenocode.Client.Attributes.AssemblyAttributes.ProcessedByXenocode",
            "PoweredByAttribute",
            "ObfuscatedByGoliath",
            "NineRays.Obfuscator.Evaluation",
            "NetGuard",
            "dotNetProtector",
            "DotNetPatcherPackerAttribute",
            "DotNetPatcherObfuscatorAttribute",
            "DotfuscatorAttribute",
            "CryptoObfuscator.ProtectedWithCryptoObfuscatorAttribute",
            "BabelObfuscatorAttribute",
            "BabelAttribute",
            "AssemblyInfoAttribute"
        };

        static void Main(string[] args)
        {
            if (args.Length == 0) return;

            var module = ModuleDefMD.Load(args[0]);

            FixModuleName(module, args[0]);
            RemoveFakeTypes(module);
            RemoveAntiDe4Dot(module);
            DecryptStrings(module);

            var outPath = Path.Combine(
                Path.GetDirectoryName(args[0]),
                Path.GetFileNameWithoutExtension(args[0]) + ".unpacked" + Path.GetExtension(args[0])
            );

            module.Write(outPath);
        }

        static void FixModuleName(ModuleDefMD module, string path)
        {
            if (module.Name == "ObfuscatedByVapor")
                module.Name = Path.GetFileName(path);
        }

        static void RemoveFakeTypes(ModuleDefMD module)
        {
            var list = module.Types.Where(t =>
                attrib.Any(a => t.Name.String == a) ||
                t.Name.String.Contains("俺ム仮") ||
                t.Namespace.String.Contains("俺ム仮")
            ).ToList();

            foreach (var t in list)
                module.Types.Remove(t);
        }

        static void RemoveAntiDe4Dot(ModuleDefMD module)
        {
            var list = module.Types.Where(t =>
                t.Name.String.StartsWith("Form") &&
                t.Interfaces.Count > 0 &&
                t.BaseType != null &&
                t.BaseType.FullName == "System.Attribute"
            ).ToList();

            foreach (var t in list)
                module.Types.Remove(t);
        }

        static void DecryptStrings(ModuleDefMD module)
        {
            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody) continue;

                    var body = method.Body;
                    var ins = body.Instructions;

                    for (int i = 0; i < ins.Count - 5; i++)
                    {
                        if (ins[i].OpCode == OpCodes.Nop &&
                            ins[i + 1].OpCode == OpCodes.Call &&
                            ins[i + 2].OpCode == OpCodes.Ldstr &&
                            ins[i + 3].OpCode == OpCodes.Call &&
                            ins[i + 4].OpCode == OpCodes.Callvirt)
                        {
                            try
                            {
                                string b64 = ins[i + 2].Operand.ToString();
                                string s = Encoding.UTF8.GetString(Convert.FromBase64String(b64));

                                ins[i].OpCode = OpCodes.Ldstr;
                                ins[i].Operand = s;

                                for (int j = 1; j <= 4; j++)
                                {
                                    ins[i + j].OpCode = OpCodes.Nop;
                                    ins[i + j].Operand = null;
                                }
                            }
                            catch { }
                        }
                    }

                    body.OptimizeBranches();
                    body.OptimizeMacros();
                }
            }
        }
    }
}
