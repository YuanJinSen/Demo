using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class ClassTypeName : PropertyAttribute
    {
        public Type type;

        public ClassTypeName(Type type)
        {
            this.type = type;
        }
    }
}