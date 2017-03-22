﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NetPrints.Core;

namespace NetPrintsEditor.Reflection
{
    public class ReflectionProvider : IReflectionProvider
    {
        public List<Assembly> Assemblies
        {
            get;
            set;
        }
        
        public ReflectionProvider(List<Assembly> assemblies)
        {
            Assemblies = assemblies;
        }

        #region IReflectionProvider
        public IEnumerable<TypeSpecifier> GetNonStaticTypes()
        {
            return Assemblies.SelectMany(a => a.GetTypes().Where(
                t => t.IsPublic && !(t.IsAbstract && t.IsSealed)).
                Select(t => (TypeSpecifier)t));
        }

        public IEnumerable<MethodSpecifier> GetPublicMethodsForType(TypeSpecifier typeSpecifier)
        {
            Type type = GetTypeFromSpecifier(typeSpecifier);

            if (type != null)
            {
                // Get all public instance methods, ignore special ones (properties / events)

                return type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(m => !m.IsSpecialName).Select(m => (MethodSpecifier)m);
            }
            else
            {
                return new MethodSpecifier[] { };
            }
        }

        public IEnumerable<PropertySpecifier> GetPublicPropertiesForType(TypeSpecifier typeSpecifier)
        {
            return GetTypeFromSpecifier(typeSpecifier)?.GetProperties().Select(p => (PropertySpecifier)p);
        }

        public IEnumerable<MethodSpecifier> GetStaticFunctions()
        {
            return Assemblies.SelectMany(a =>
                a.GetTypes().Where(t => t.IsPublic).SelectMany(t =>
                    t.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Where(m => !m.IsSpecialName).Select(m => (MethodSpecifier)m)));
        }

        public IEnumerable<MethodSpecifier> GetStaticFunctionsWithReturnType(TypeSpecifier returnTypeSpecifier)
        {
            return Assemblies.SelectMany(a =>
                a.GetTypes().Where(t => t.IsPublic).SelectMany(t =>
                    t.GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .Where(m => m.ReturnType == returnTypeSpecifier && !m.IsSpecialName).
                        Select(m => (MethodSpecifier)m)));
        }

        public IEnumerable<ConstructorSpecifier> GetConstructors(TypeSpecifier typeSpecifier)
        {
            return GetTypeFromSpecifier(typeSpecifier)?.GetConstructors().Select(c => (ConstructorSpecifier)c);
        }

        public IEnumerable<string> GetEnumNames(TypeSpecifier typeSpecifier)
        {
            return GetTypeFromSpecifier(typeSpecifier)?.GetEnumNames();
        }
        
        public bool TypeSpecifierIsSubclassOf(TypeSpecifier a, TypeSpecifier b)
        {
            Type typeA = GetTypeFromSpecifier(a);
            Type typeB = GetTypeFromSpecifier(b);

            return typeA != null && typeB != null && typeA.IsSubclassOf(typeB);
        }
        #endregion

        private Type GetTypeFromSpecifier(TypeSpecifier specifier)
        {
            foreach (Assembly assembly in Assemblies)
            {
                Type t = assembly.GetType(specifier);
                if (t != null)
                {
                    return t;
                }
            }

            return null;
        }
    }
}
