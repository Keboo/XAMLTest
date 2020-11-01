﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XamlTest.Utility
{
    internal static class AppDomainMixins
    {
        public static void IncludeAssembliesIn(this AppDomain appDomain, string directory)
        {
            var resolver = new AssemblyResolver();
            resolver.IncludeFiles(Directory.EnumerateFiles(directory));

            appDomain.AssemblyResolve += AppDomainAssemblyResolve;

            Assembly? AppDomainAssemblyResolve(object? sender, ResolveEventArgs e)
            {
                if (e.Name is { } name && resolver.Resolve(name) is { } assembly)
                {
                    return assembly;
                }
                return null;
            }
        }

        private class AssemblyResolver
        {
            private List<(AssemblyName Name, Lazy<Assembly> Assembly)> Assemblies { get; }
                = new List<(AssemblyName Name, Lazy<Assembly> Assembly)>();

            public void IncludeFiles(IEnumerable<string> assemblyFilePaths)
            {
                if (assemblyFilePaths is null)
                {
                    throw new ArgumentNullException(nameof(assemblyFilePaths));
                }

                foreach (var file in assemblyFilePaths)
                {
                    try
                    {
                        Assemblies.Add((AssemblyName.GetAssemblyName(file), new Lazy<Assembly>(() => Assembly.LoadFrom(file))));
                    }
                    catch (BadImageFormatException)
                    {
                        continue;
                    }
                }
            }

            public Assembly? Resolve(string assemblyName)
            {
                var name = new AssemblyName(assemblyName);

                var possible = Assemblies.Where(x => x.Name.Name == name.Name);
                if (name.Version != null)
                {
                    possible = possible.Where(x => x.Name.Version == name.Version);
                }
                if (name.KeyPair != null)
                {
                    possible = possible.Where(x => name.KeyPair.PublicKey.SequenceEqual(x.Name.KeyPair?.PublicKey ?? Array.Empty<byte>()));
                }
                var found = possible.ToList();
                if (found.Count == 1)
                {
                    return found[0].Assembly.Value;
                }
                //TODO Handle 0 and multiple errors cases
                return null;
            }
        }
    }
}
