using System;
using System.Collections.Generic;

namespace ECS
{
    public class SystemHub
    {
        protected List<ECS.Entity> _entities = new List<ECS.Entity>();
        protected List<ECS.Group> _groups = new List<ECS.Group>();
        private List<System> _systems = new List<System>();

        private Queue<HubMessage> _queue = new Queue<HubMessage>();
        private Dictionary<string, System> _named = new Dictionary<string, System>();
        
        public SystemHub(){}
        ~SystemHub() {
            Destroy();
        }

        public int CountEnt() { return _entities.Count; }
        public int CountGrp() { return _groups.Count; }
        public int CountSys() { return _systems.Count; }

        internal ECS.System AddSystem(ECS.System system, string name=null)
        {
            if(system == null)
                throw new ArgumentException("Parameter cannot be null", "original");

            var group = new ECS.Group(system);
            group.Update(_entities);
            
            _systems.Add(system);
            _groups.Add(group);

            if(name != null)
                _named.Add(name, system);
            system.SetSystemHub(this);

            return system;
        }

        internal void RemoveEntAll()
        {
            foreach(var entity in _entities)
                entity.OnDestory();
            _entities.Clear();
            
            foreach(var group in _groups)
                group.Update(_entities);
        }

        internal void Destroy()
        {
            foreach(var system in _systems)
            {
                system.Destroy();
            }
        }

        internal List<object> FindEntAll<T>()
        {
            return _entities.FindAll(x=>x is T).ConvertAll(x=>(object)x);
        }

        public bool SendMessage(string name, HubMessage msg) {
            if(name == null)
                throw new ArgumentException("Parameter cannot be null", "original");
            
            System sys;
            if(_named.TryGetValue(name, out sys)== false)
                return false;
            
            sys.EnQueue(msg);
            return true;
        }

        internal bool Add(ECS.Entity entity) {
            if(_entities.Contains(entity))
                return false;

            _entities.Add(entity);
            entity.OnCreate();
            
            for(var i=0;i<_groups.Count;++i)
                _groups[i].Update(_entities);

            return true;
        }

        internal bool Remove(ECS.Entity entity) {
            if(!_entities.Contains(entity))
                return false;
            
            entity.OnDestory();
            if(_entities.Remove(entity) == false)
                return false;
            
            for(var i=0;i<_groups.Count;++i)
                _groups[i].Update(_entities);
            
            return true;
        }

        internal System FindSystem(string name)
        {
            System sys;
            if(_named.TryGetValue(name, out sys) == false)
                return null;
            return sys;
        }

        internal void Step(int step)
        {
            for(var i=0;i<_systems.Count;++i) {
                _systems[i].Step(step, _groups[i]);
            }
        }

        // internal void RemoveAll()
        // {
        //     foreach(var entity in _entities)
        //         entity.Destory();
        //     _entities.Clear();
        //     foreach(var group in _groups)
        //         group.Update(_entities);

        // }
    }
}
