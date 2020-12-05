namespace ECS
{
    public class HubMessage
    {
        public System Sender {get; private set;}

        public HubMessage(System sender) {
            Sender = sender;
        }
    }
}
