using System;
using System.Collections.Generic;

namespace ECS
{
    public class Entity
    {
        protected List<Component> _collection;

        public Entity() {
            _collection = new List<Component>();
        }

        public Component GetComponent<T>() {
            Type type = typeof(T);
            foreach(var comp in _collection)
                if(comp.GetType() == type) return comp;
            return null;
        }

        internal Component GetComponent(Type type) {
            foreach(var comp in _collection)
                if(comp.GetType() == type) return comp;
            return null;
        }

        virtual internal void OnDestory() {
            foreach(var component in _collection)
                component.Destroy();
        }
        
        virtual internal void OnCreate() {
            foreach(var component in _collection)
                component.Create();
        }
    }

    public class eUnit : Entity
    {
        public eUnit() : base() {
            _collection.Add(new Position(this));
            _collection.Add(new HitPoint(this));
        }
    }
}
