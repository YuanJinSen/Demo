using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Odyssey
{
    public abstract class EntityState<T> where T : Entity<T>
    {
        public UnityEvent onEnter;
        public UnityEvent onExit;

        public float timeSinceEntered { get; protected set; }

        #region Unity

        private void Awake()
        {

        }

        private void Start()
        {

        }

        private void Update()
        {

        }

        #endregion

        #region Private



        #endregion

        #region Protected

        protected abstract void OnEnter(T entity);

        protected abstract void OnStep(T entity);

        protected abstract void OnExit(T entity);

        #endregion

        #region Public

        public void Enter(T entity)
        {
            timeSinceEntered = 0;
            onEnter?.Invoke();
            OnEnter(entity);
        }

        public void Step(T entity)
        {
            OnStep(entity);
            timeSinceEntered += Time.deltaTime;
        }

        public void Exit(T entity)
        {
            onExit?.Invoke();
            OnExit(entity);
        }

        public abstract void OnContact(T entity, Collider other);

        public static EntityState<T> CreateStateFromString(string state)
        {
            Type type = Type.GetType(state);
            object obj = Activator.CreateInstance(type);
            return obj as EntityState<T>;
        }

        public static List<EntityState<T>> CreateStatesFromStringArray(string[] states)
        {
            var list = new List<EntityState<T>>();

            foreach (string state in states)
            {
                list.Add(CreateStateFromString(state));
            }

            return list;
        }

        #endregion
    }
}