using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.Targets
{
    class TargetFactory
    {
        private static List<Type> FindAllDerivedTypes<T>()
        {
            return FindAllDerivedTypes<T>(System.Reflection.Assembly.GetAssembly(typeof(T)));
        }

        private static List<Type> FindAllDerivedTypes<T>(System.Reflection.Assembly assembly)
        {
            var derivedType = typeof(T);
            return assembly.GetTypes().Where(t => t != derivedType && derivedType.IsAssignableFrom(t)).ToList();

        }

        public static List<Tuple<string, string>> GetTargetNames()
        {
            List<Tuple<string, string>> names = new List<Tuple<string, string>>();
            var targets = FindAllDerivedTypes<Target>();

            foreach (var target in targets)
            {
                if (!target.IsAbstract)
                {
                    names.Add( new Tuple<string,string>(target.Name, target.Namespace));
                }
            }

            return names;
        }

        public static Target Create(string targetName, string targetNamespace)
        {
            var newTargetName = $"{targetNamespace}.{targetName}";
            return (Target)System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(newTargetName);
        }
    }
}
