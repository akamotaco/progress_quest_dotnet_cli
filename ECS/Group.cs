using System.Collections.Generic;
using System;

namespace ECS
{
    public class Group
    {
        public System System { get; private set; }
        private List<List<Component>> _group;

        public Group(System system) {
            System = system;
        }
        private List<Component> filter(Entity entity, Type[] types) {
            List<Component> _filter = new List<Component>();

            for(int i=0;i<types.Length;++i) {
                var comp = entity.GetComponent(types[i]);
                if(comp != null)
                    _filter.Add(comp);
            }

            if(_filter.Count != types.Length) return null;

            return _filter;
        }

        public int Update(List<Entity> entities) {
            _group = new List<List<Component>>();
            foreach(var entity in entities) {
                var comps = filter(entity,System.Filter);
                if(comps == null)
                    continue;
                _group.Add(comps);
            }
            return _group.Count;
        }

        public List<List<Component>> FilteredComponents() {
            return _group;
        }
    }
}