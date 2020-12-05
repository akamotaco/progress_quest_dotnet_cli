using System;

namespace ECS
{
    public class Component
    {
        public Entity Entity {get; private set;}
        public Component(Entity parent) {
            Entity = parent;
        }

        virtual public void Create() {}
        virtual public void Destroy() {}
    }

    public class Position : Component
    {
        public Position(Entity entity) : base(entity) {}
        public float x = 0.5f;
    }

    public class HitPoint : Component
    {
        public HitPoint(Entity entity) : base(entity) {}
        public int hp = 12;
    }
}
