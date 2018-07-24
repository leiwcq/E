using System;

namespace E.Interface
{
    public class AttributeBase : Attribute
    {
        public AttributeBase()
        {
            typeId = Guid.NewGuid();
        }

        protected readonly Guid typeId; //Hack required to give Attributes unique identity
    }
}