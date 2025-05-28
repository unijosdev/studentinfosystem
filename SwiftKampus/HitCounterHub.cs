using Microsoft.AspNet.SignalR.Hubs;
using System.Threading.Tasks;

namespace SwiftKampus
{
    [HubName("hitCounter")]
    public class HitCounterHub : Microsoft.AspNet.SignalR.Hub
    {
        // don't want to use static in web app

        static int _hitCount;
        public void RecordHit()
        {
            //Clients.All.hello();
            _hitCount += 1;
            Clients.All.OnRecordHit(_hitCount);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _hitCount -= 1;
            Clients.All.OnRecordHit(_hitCount);
            //Clients.Users()
            return base.OnDisconnected(stopCalled);
        }
    }
}