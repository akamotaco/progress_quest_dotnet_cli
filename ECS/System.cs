using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECS
{
    public class System
    {
        public SystemHub Hub {get; private set;}
        private Queue<HubMessage> _msgQ = new Queue<HubMessage>();

        public Type[] Filter { get; private set; }
        public System(params Type[] types) {
            Filter = new Type[types.Length];
            if(Filter.Length > 0)
                types.CopyTo(Filter,0);
        }

        public bool Step(int step,Group group, bool systemDependence=true) {
            MsgProc();
            
            if(systemDependence && group.System != this) return false;

            var allComponents = group.FilteredComponents();
            foreach(var comps in allComponents) {
#region only debug
                if(checkType(comps) == false)
                    throw new Exception();
#endregion
            }

            Proc(step, allComponents);
            return true;
        }

        public virtual void Destroy()
        {
        }

        protected virtual void Proc(int step, List<List<Component>> allComponents) {}
        protected virtual void ApplyMessage(HubMessage message) {throw new ArgumentException("no apply process",this.ToString()+":"+message.ToString());}

        public delegate void SystemCallback(HubMessage message);
        private static void NullCallback(HubMessage message){}
        private SystemCallback _callback = NullCallback;
        public SystemCallback Callback {
            set {_callback = value == null ? NullCallback : value;}
            get {return _callback;}
        }

        protected virtual void MsgProc() {
            while(this._msgQ.Count>0) {
                var msg = _msgQ.Dequeue();
                ApplyMessage(msg);
                _callback(msg);
            }
        }

        private bool checkType(List<Component> comps)
        {
            if(comps.Count != Filter.Length) return false;

            for(var i=0;i<comps.Count;++i)
                if(comps[i].GetType() != Filter[i]) return false;

            return true;
        }

        internal void SetSystemHub(SystemHub hub)
        {
            Hub = hub;
        }
        
        public void EnQueue(HubMessage msg) {
            this._msgQ.Enqueue(msg);
        }
    }
}
