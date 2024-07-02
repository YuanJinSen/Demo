using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public abstract class EntityStateManager : MonoBehaviour 
    { 
        public EntityStateManagerEvents events; 
    }

    public abstract class EntityStateManager<T> : EntityStateManager where T : Entity<T>
    {
        public EntityState<T> currentState { get; protected set; }
        public EntityState<T> lastState { get; protected set; }
        public int lastStateIndex => _list.IndexOf(lastState);
        public int currentStateIndex => _list.IndexOf(currentState);
        public T entity { get; protected set; }

        protected List<EntityState<T>> _list;
        protected Dictionary<Type, EntityState<T>> _states;

        #region Unity

        protected void Start()
        {
            Init();
        }

        #endregion

        #region Protected

        protected abstract List<EntityState<T>> GetStateList();

        protected virtual void Init()
        {
            entity = GetComponent<T>();

            _list = GetStateList();
            _states = new Dictionary<Type, EntityState<T>>();
            foreach (var state in _list)
            {
                Type type = state.GetType();
                if (!_states.ContainsKey(type))
                {
                    _states.Add(type, state);
                }
            }

            if (_list.Count > 0)
            {
                currentState = _list[0];
            }
        }

        #endregion

        #region Public

        public virtual void Step()
        {
            if (currentState != null && Time.timeScale > 0)
            {
                currentState.Step(entity);
            }
        }

        public virtual void Change<TState>()where TState:EntityState<T>
        {
            Type type = typeof(TState);
            if (_states.ContainsKey(type))
            {
                Change(_states[type]);
            }
        }

        public virtual void Change(EntityState<T> to)
        {
            if (to != null && Time.timeScale > 0)
            {
                if (currentState != null)
                {
                    currentState.Exit(entity);
                    lastState = currentState;
                }
                to.Enter(entity);
                currentState = to;
                events.onEnter.Invoke(currentState.GetType());
                events.onChange?.Invoke();
            }
        }

        public virtual void OnContact(Collider other)
        {
            if (currentState != null && Time.timeScale > 0)
            {
                currentState.OnContact(entity, other);
            }
        }

        public virtual bool IsCurrentOfType(Type type)
        {
            return currentState != null && currentState.GetType() == type;
        }

        public virtual bool ContainsStateOfType(Type type)
        {
            return _states.ContainsKey(type);
        }

        #endregion
    }
}