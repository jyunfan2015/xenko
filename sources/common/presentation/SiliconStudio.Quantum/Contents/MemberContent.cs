﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using System.Reflection;

using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// An implementation of <see cref="IContent"/> that gives access to a member of an object.
    /// </summary>
    public class MemberContent : ContentBase, IUpdatableContent
    {
        protected IContent Container;
        private readonly ModelContainer modelContainer;
        private IModelNode modelNode;

        public MemberContent(INodeBuilder nodeBuilder, IContent container, IMemberDescriptor member, bool isPrimitive, IReference reference)
            : base(nodeBuilder.TypeDescriptorFactory.Find(member.Type), isPrimitive, reference)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            Member = member;
            Container = container;
            modelContainer = nodeBuilder.ModelContainer;
        }

        /// <summary>
        /// The <see cref="IMemberDescriptor"/> used to access the member of the container represented by this content.
        /// </summary>
        public IMemberDescriptor Member { get; protected set; }

        /// <inheritdoc/>
        public sealed override object Value { get { if (Container.Value == null) throw new InvalidOperationException("Container's value is null"); return Member.Get(Container.Value); } }

        /// <inheritdoc/>
        public override void Update(object newValue, object index)
        {
            if (index != null)
            {
                var collectionDescriptor = Descriptor as CollectionDescriptor;
                var dictionaryDescriptor = Descriptor as DictionaryDescriptor;
                if (collectionDescriptor != null)
                {
                    collectionDescriptor.SetValue(Value, (int)index, newValue);
                }
                else if (dictionaryDescriptor != null)
                {
                    dictionaryDescriptor.SetValue(Value, index, newValue);
                }
                else
                    throw new NotSupportedException("Unable to set the node value, the collection is unsupported");

                UpdateReferences();
            }
            else
            {
                var oldValue = Value;
                if (Container.Value == null) throw new InvalidOperationException("Container's value is null");
                var containerValue = Container.Value;
                Member.Set(containerValue, newValue);

                if (Container.Value.GetType().GetTypeInfo().IsValueType)
                    Container.Update(containerValue);

                UpdateReferences();
                NotifyContentChanged(oldValue, Value);
            }
        }

        private void UpdateReferences()
        {
            if (modelContainer != null && modelNode != null)
            {
                modelContainer.UpdateReferences(modelNode);
            }
        }

        void IUpdatableContent.RegisterOwner(IModelNode node)
        {
            modelNode = node;
        }
    }
}
