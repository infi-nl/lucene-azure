using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.Windsor;
using Castle.Windsor.Installer;

namespace Infi.IoC {
    public static class DependencyInjectionSetup {
        public static WindsorContainer SetupDI(string namespacePrefix) {
            var windsorContainer = new WindsorContainer();

            var allAssemblies = GetAssemblies(namespacePrefix);

            foreach (var assemblyName in allAssemblies) {
                windsorContainer.Install(FromAssembly.Named(assemblyName));
            }

            return windsorContainer;
        }

        private static IEnumerable<string> GetAssemblies(string assemblyPrefix) {
            var done = new List<string>();
            var todo = new Stack<Assembly>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                todo.Push(assembly);
            }

            do {
                var todoItem = todo.Pop();
                var referencedAssemblies =
                    todoItem.GetReferencedAssemblies()
                        .Where(reference =>
                                reference.Name.StartsWith(assemblyPrefix) &&
                                !done.Contains(reference.FullName));

                foreach (var reference in referencedAssemblies) {
                    todo.Push(Assembly.Load(reference));
                    done.Add(reference.FullName);
                }
            } while (todo.Any());

            return done;
        }
    }
}
