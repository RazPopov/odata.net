//---------------------------------------------------------------------
// <copyright file="CsdlSemanticsNavigationSource.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.Edm.Csdl.CsdlSemantics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.OData.Edm;
    using Microsoft.OData.Edm.Annotations;
    using Microsoft.OData.Edm.Csdl.Parsing.Ast;
    using Microsoft.OData.Edm.Expressions;
    using Microsoft.OData.Edm.Library;
    using Microsoft.OData.Edm.Library.Expressions;

    /// <summary>
    /// Provides semantics for CsdlAbstractNavigationSource.
    /// </summary>
    internal abstract class CsdlSemanticsNavigationSource : CsdlSemanticsElement, IEdmNavigationSource
    {
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1304:NonPrivateReadonlyFieldsMustBeginWithUpperCaseLetter", Justification = "protected field in internal class.")]
        protected readonly CsdlAbstractNavigationSource navigationSource;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1304:NonPrivateReadonlyFieldsMustBeginWithUpperCaseLetter", Justification = "protected field in internal class.")]
        protected readonly CsdlSemanticsEntityContainer container;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1304:NonPrivateReadonlyFieldsMustBeginWithUpperCaseLetter", Justification = "protected field in internal class.")]
        protected readonly IEdmPathExpression path;

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1304:NonPrivateReadonlyFieldsMustBeginWithUpperCaseLetter", Justification = "protected field in internal class.")]
        protected readonly Cache<CsdlSemanticsNavigationSource, IEdmEntityType> typeCache = new Cache<CsdlSemanticsNavigationSource, IEdmEntityType>();
        protected static readonly Func<CsdlSemanticsNavigationSource, IEdmEntityType> ComputeElementTypeFunc = (me) => me.ComputeElementType();

        private readonly Cache<CsdlSemanticsNavigationSource, IEnumerable<IEdmNavigationPropertyBinding>> navigationTargetsCache = new Cache<CsdlSemanticsNavigationSource, IEnumerable<IEdmNavigationPropertyBinding>>();
        private static readonly Func<CsdlSemanticsNavigationSource, IEnumerable<IEdmNavigationPropertyBinding>> ComputeNavigationTargetsFunc = (me) => me.ComputeNavigationTargets();

        private readonly Dictionary<IEdmNavigationProperty, IEdmContainedEntitySet> containedNavigationPropertyCache =
            new Dictionary<IEdmNavigationProperty, IEdmContainedEntitySet>();

        private readonly Dictionary<IEdmNavigationProperty, IEdmUnknownEntitySet> unknownNavigationPropertyCache =
            new Dictionary<IEdmNavigationProperty, IEdmUnknownEntitySet>();

        public CsdlSemanticsNavigationSource(CsdlSemanticsEntityContainer container, CsdlAbstractNavigationSource navigationSource)
            : base(navigationSource)
        {
            this.container = container;
            this.navigationSource = navigationSource;
            this.path = new EdmPathExpression(this.navigationSource.Name);
        }

        public override CsdlSemanticsModel Model
        {
            get { return this.container.Model; }
        }

        public IEdmEntityContainer Container
        {
            get { return this.container; }
        }

        public override CsdlElement Element
        {
            get { return this.navigationSource; }
        }

        public string Name
        {
            get { return this.navigationSource.Name; }
        }

        public Expressions.IEdmPathExpression Path
        {
            get { return this.path; }
        }

        public abstract IEdmType Type { get; }

        public abstract EdmContainerElementKind ContainerElementKind { get; }

        public IEnumerable<IEdmNavigationPropertyBinding> NavigationPropertyBindings
        {
            get { return this.navigationTargetsCache.GetValue(this, ComputeNavigationTargetsFunc, null); }
        }

        public IEdmNavigationSource FindNavigationTarget(IEdmNavigationProperty property)
        {
            EdmUtil.CheckArgumentNull(property, "property");

            if (!property.ContainsTarget)
            {
                foreach (IEdmNavigationPropertyBinding targetMapping in this.NavigationPropertyBindings)
                {
                    if (targetMapping.NavigationProperty == property)
                    {
                        return targetMapping.Target;
                    }
                }
            }
            else
            {
                return EdmUtil.DictionaryGetOrUpdate(
                    this.containedNavigationPropertyCache,
                    property,
                    navProperty => new EdmContainedEntitySet(this, navProperty));
            }

            return EdmUtil.DictionaryGetOrUpdate(
                    this.unknownNavigationPropertyCache,
                    property,
                    navProperty => new EdmUnknownEntitySet(this, navProperty));
        }

        protected override IEnumerable<IEdmVocabularyAnnotation> ComputeInlineVocabularyAnnotations()
        {
            return this.Model.WrapInlineVocabularyAnnotations(this, this.container.Context);
        }

        protected abstract IEdmEntityType ComputeElementType();

        private IEnumerable<IEdmNavigationPropertyBinding> ComputeNavigationTargets()
        {
            return this.navigationSource.NavigationPropertyBindings.Select(this.CreateSemanticMappingForBinding).ToList();
        }

        private IEdmNavigationPropertyBinding CreateSemanticMappingForBinding(CsdlNavigationPropertyBinding binding)
        {
            IEdmNavigationProperty navigationProperty = this.ResolveNavigationPropertyPathForBinding(binding);
            
            IEdmNavigationSource targetNavigationSource = this.Container.FindEntitySetExtended(binding.Target);
            if (targetNavigationSource == null)
            {
                targetNavigationSource = this.Container.FindSingletonExtended(binding.Target);
                if (targetNavigationSource == null)
                {
                    targetNavigationSource = new UnresolvedEntitySet(binding.Target, this.Container, binding.Location);
                }
            }

            return new EdmNavigationPropertyBinding(navigationProperty, targetNavigationSource);
        }

        private IEdmNavigationProperty ResolveNavigationPropertyPathForBinding(CsdlNavigationPropertyBinding binding)
        {
            Debug.Assert(binding != null, "binding != null");

            string bindingPath = binding.Path;
            Debug.Assert(bindingPath != null, "bindingPath != null");

            IEdmEntityType definingType = this.typeCache.GetValue(this, ComputeElementTypeFunc, null);

            const char Slash = '/';
            if (bindingPath.Length == 0 || bindingPath[bindingPath.Length - 1] == Slash)
            {
                // if the path did not actually contain a navigation property, then treat it as an unresolved path.
                // TODO: improve the error given in this case?
                return new UnresolvedNavigationPropertyPath(definingType, bindingPath, binding.Location);
            }

            IEdmNavigationProperty navigationProperty;
            string[] pathSegements = bindingPath.Split(Slash);
            for (int index = 0; index < pathSegements.Length - 1; index++)
            {
                string pathSegement = pathSegements[index];
                if (pathSegement.Length == 0)
                {
                    // TODO: improve the error given in this case?
                    return new UnresolvedNavigationPropertyPath(definingType, bindingPath, binding.Location);
                }

                IEdmEntityType derivedType = this.container.Context.FindType(pathSegement) as IEdmEntityType;
                if (derivedType == null)
                {
                    IEdmProperty property = definingType.FindProperty(pathSegement);
                    navigationProperty = property as IEdmNavigationProperty;
                    if (navigationProperty == null)
                    {
                        return new UnresolvedNavigationPropertyPath(definingType, bindingPath, binding.Location);
                    }

                    definingType = navigationProperty.ToEntityType();
                }
                else
                {
                    definingType = derivedType;
                }
            }

            navigationProperty = definingType.FindProperty(pathSegements.Last()) as IEdmNavigationProperty;
            if (navigationProperty == null)
            {
                // TODO: improve the error given in this case?
                navigationProperty = new UnresolvedNavigationPropertyPath(definingType, bindingPath, binding.Location);
            }

            return navigationProperty;
        }
    }
}