using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Odyssey
{
    [Serializable]
    public class EntityEvents
    {
        public UnityEvent onGroundEnter;
        public UnityEvent onGroundExit;
        public UnityEvent onRailsEnter;
        public UnityEvent onRailsExit;
    }
}