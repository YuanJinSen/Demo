using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public interface IEntityContact
    {
        public void OnEntityContact(Entity entity);
    }
}