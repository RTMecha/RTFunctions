using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

namespace RTFunctions.Patchers
{
    public static class Patcher
    {
        public enum PatchType
        {
            Prefix,
            Postfix,
            Transpiler,
            Finalizer,
            ILManipulator
        }

        public static void PatchPropertySetter(Type type, string property, BindingFlags bindingFlags, bool nonPublic,
            Type patchType, string patchMethod, BindingFlags patchBindingFlags, PatchType fix = PatchType.Prefix)
        {
            var propertySetter = type.GetProperty(property, bindingFlags).GetSetMethod(nonPublic);

            var methodPrefix = patchType.GetMethod(patchMethod, patchBindingFlags);

            HarmonyMethod methodPatch = new HarmonyMethod(methodPrefix);

            switch (fix)
            {
                case PatchType.Prefix:
                    {
                        FunctionsPlugin.harmony.Patch(propertySetter, prefix: methodPatch);
                        break;
                    }
                case PatchType.Postfix:
                    {
                        FunctionsPlugin.harmony.Patch(propertySetter, postfix: methodPatch);
                        break;
                    }
                case PatchType.Transpiler:
                    {
                        FunctionsPlugin.harmony.Patch(propertySetter, transpiler: methodPatch);
                        break;
                    }
                case PatchType.Finalizer:
                    {
                        FunctionsPlugin.harmony.Patch(propertySetter, finalizer: methodPatch);
                        break;
                    }
                case PatchType.ILManipulator:
                    {
                        FunctionsPlugin.harmony.Patch(propertySetter, ilmanipulator: methodPatch);
                        break;
                    }
            }
        }

        public static void PatchMethod(Type type, string method, BindingFlags bindingFlags,
            Type patchType, string patchMethod, BindingFlags patchBindingFlags, PatchType fix = PatchType.Prefix)
        {
            var methodToPatch = type.GetMethod(method, bindingFlags);

            var methodPrefix = patchType.GetMethod(patchMethod, patchBindingFlags);

            var methodPatch = new HarmonyMethod(methodPrefix);

            switch (fix)
            {
                case PatchType.Prefix:
                    {
                        FunctionsPlugin.harmony.Patch(methodToPatch, prefix: methodPatch);
                        break;
                    }
                case PatchType.Postfix:
                    {
                        FunctionsPlugin.harmony.Patch(methodToPatch, postfix: methodPatch);
                        break;
                    }
                case PatchType.Transpiler:
                    {
                        FunctionsPlugin.harmony.Patch(methodToPatch, transpiler: methodPatch);
                        break;
                    }
                case PatchType.Finalizer:
                    {
                        FunctionsPlugin.harmony.Patch(methodToPatch, finalizer: methodPatch);
                        break;
                    }
                case PatchType.ILManipulator:
                    {
                        FunctionsPlugin.harmony.Patch(methodToPatch, ilmanipulator: methodPatch);
                        break;
                    }
            }
        }
    }
}
