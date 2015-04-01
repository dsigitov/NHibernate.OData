﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Metadata;
using NHibernate.Type;

namespace NHibernate.OData
{
    internal class MappedClassMetadata
    {
        private readonly IDictionary<string, DynamicComponentProperty> _caseSensitiveDynamicProperties = new Dictionary<string, DynamicComponentProperty>(StringComparer.Ordinal);
        private readonly IDictionary<string, DynamicComponentProperty> _caseInsensitiveDynamicProperties = new Dictionary<string, DynamicComponentProperty>(StringComparer.OrdinalIgnoreCase);
        private readonly ISet<string> _caseSensitiveComponents = new HashSet<string>(StringComparer.Ordinal);
        private readonly ISet<string> _caseInsensitiveComponents = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public MappedClassMetadata(IClassMetadata classMetadata)
        {
            Require.NotNull(classMetadata, "classMetadata");

            for (int i = 0; i < classMetadata.PropertyNames.Length; i++)
                BuildComponentMetadata(classMetadata.PropertyNames[i], classMetadata.PropertyTypes[i]);
        }

        public DynamicComponentProperty FindDynamicComponentProperty(string fullPath, bool caseSensitive)
        {
            Require.NotNull(fullPath, "fullPath");

            var dictionary = caseSensitive ? _caseSensitiveDynamicProperties : _caseInsensitiveDynamicProperties;
            DynamicComponentProperty dynamicProperty;

            dictionary.TryGetValue(fullPath, out dynamicProperty);

            return dynamicProperty;
        }

        public bool IsComponent(string fullPath, bool caseSensitive)
        {
            var componentNamesSet = caseSensitive ? _caseSensitiveComponents : _caseInsensitiveComponents;
            return componentNamesSet.Contains(fullPath);
        }

        private void BuildComponentMetadata(string name, IType type)
        {
            ComponentType component = type as ComponentType;
            if (component == null)
                return;

            _caseSensitiveComponents.Add(name);
            _caseInsensitiveComponents.Add(name);

            bool isDynamicComponent = component.ReturnedClass == typeof(IDictionary);

            for (int i = 0; i < component.PropertyNames.Length; i++)
            {
                string fullName = name + "." + component.PropertyNames[i];

                if (isDynamicComponent)
                {
                    var dynamicProperty = new DynamicComponentProperty(component.PropertyNames[i], component.Subtypes[i].ReturnedClass);

                    _caseInsensitiveDynamicProperties[fullName] = dynamicProperty;
                    _caseSensitiveDynamicProperties[fullName] = dynamicProperty;
                }

                BuildComponentMetadata(fullName, component.Subtypes[i]);
            }
        }
    }
}
