using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using SwiftKampus.Models;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SwiftKampus
{
    public class ChatHub : Hub
    {
        private readonly SchoolDbContext _db = new SchoolDbContext();
        private static int _hitCount = 2;

        [Authorize]
        public void Send(string name, string message)
        {
            var userCount = _db.Users.Count();
            var list = _db.Users.ToList();
            Clients.All.addNewMessageToPage(name, message, userCount, list);
        }

        public async Task GetUser()
        {
            var list = await _db.Users.AsNoTracking().Where(x => x.IsLogin.Equals(true)).ToListAsync();
            string userimg = "/images/DP/dummy.png";
            var userId = HttpContext.Current.User.Identity.GetUserId();
            list = list.Where(x => x.Id != userId).ToList();
            var loginDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            Clients.Caller.onGetUser(list, loginDate, userimg);

            var gm = new List<GroupMessageVm>()
            {
                new GroupMessageVm()
                {
                    Name ="Joe", Message = "First message"
                },
                new GroupMessageVm()
                {
                    Name ="Davido", Message = "second message"
                }
            };
            Clients.Caller.getGroupMessages(gm);
        }

        public async Task loadPrivateMessage(string senderId, string receiverId)
        {
            var chatHistories = await _db.PrivateMessages.AsNoTracking()
                              .Where(x => (x.FromUser.Equals(receiverId) && x.ToUser.Equals(senderId))
                              || (x.FromUser.Equals(senderId) && x.ToUser.Equals(receiverId)))
                              .OrderByDescending(o => o.PrivateMessageId)
                              .Take(20).ToListAsync();
            var pm = new List<PrivateMsgVm>();
            foreach (var item in chatHistories)
            {
                var dateMsg = "";
                if (item.MessageDate.AddDays(1) > DateTime.Now)
                {
                    dateMsg = $"{item.MessageDate.Month.ToString()}-{item.MessageDate.Day.ToString()} {item.MessageDate.ToShortTimeString()}";
                }
                else if (item.MessageDate.AddDays(1) > DateTime.Now)
                {
                    dateMsg = "Yesterday";
                }
                else if (item.MessageDate.AddDays(1) > DateTime.Now)
                {
                    dateMsg = "Two days ago";
                }
                else
                {
                }
                var privateMessage = new PrivateMsgVm()
                {
                    PrivateMessageId = item.PrivateMessageId,
                    Message = item.Message,
                    MessageDate = dateMsg,
                    RecieverId = item.ToUser,
                    SenderId = item.FromUser,
                    RecieverUserName = _db.Users.AsNoTracking().Where(x => x.Id.Equals(item.FromUser))
                                        .Select(s => s.UserName).FirstOrDefault()
                };
                pm.Add(privateMessage);
            }
            //var pm = new List<PrivateMsgVm>()
            //{
            //    new PrivateMsgVm()
            //    {
            //        RecieverUserName = "Admin@swiftkampus.com", MessageDate= DateTime.Now, Message = "First Private message",
            //         RecieverId = "c3cddd9f-4af4-492a-b36a-4869080de3c6", SenderId = "UNIBEN1"
            //    },

            //     new PrivateMsgVm()
            //    {
            //        RecieverUserName = "Lanre Mattew Joseph", MessageDate= DateTime.Now, Message = "Second Private message",
            //         SenderId= "UNIBEN1", RecieverId = "c3cddd9f-4af4-492a-b36a-4869080de3c6"
            //    },
            //      new PrivateMsgVm()
            //    {
            //        RecieverUserName = "Admin@swiftkampus.com", MessageDate= DateTime.Now, Message = "Third Private message",
            //         RecieverId = "c3cddd9f-4af4-492a-b36a-4869080de3c6", SenderId = "UNIBEN1"
            //    }
            //};
            Clients.Caller.FetchPrivateMessages(pm);
        }

        public async Task RegisterUser(string userId, string connectionId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id.Equals(userId));
            if (user != null)
            {
                var chatConnection = new ChatConnection
                {
                    UserId = user.Id,
                    ConnectionId = connectionId
                };
                _db.Entry(chatConnection).State = EntityState.Added;
                _db.SaveChanges();
            }
            Clients.Caller.onRegisterUser(user);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var connectionId = Context.ConnectionId;
            var chatConnection = _db.ChatConnections.FirstOrDefault(x => x.ConnectionId.Equals(connectionId));
            if (chatConnection != null)
            {
                _db.Entry(chatConnection).State = EntityState.Deleted;
                _db.SaveChanges();
            }
            _hitCount -= 1;
            Clients.All.OnRecordHit(_hitCount);
            //Clients.Users()
            return base.OnDisconnected(stopCalled);
        }

        public async Task SendPrivateMessage(string toUserId, string message, string sender)
        {
            //string fromUserId = sender;

            //var toUser = await _db.Users.FirstOrDefaultAsync(x => x.Id == toUserId);
            var fromUser = await _db.Users.FirstOrDefaultAsync(x => x.Id.Equals(sender));

            //if (toUser != null && fromUser != null)
            //{
            var userChatConnectionIds = await _db.ChatConnections.AsNoTracking().Where(x => x.UserId.Equals(toUserId))
                                        .Select(x => x.ConnectionId).ToListAsync();
            string CurrentDateTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            var pMessage = new PrivateMessage()
            {
                Message = message,
                ToUser = toUserId,
                FromUser = sender,
                MessageDate = Convert.ToDateTime(CurrentDateTime)
            };
            _db.PrivateMessages.Add(pMessage);
            await _db.SaveChangesAsync();
            string userimg = "/images/DP/dummy.png";
            var windowId = sender;
            var fromUserName = fromUser?.UserName;
            // send to
            if (!string.IsNullOrEmpty(message))
            {
                Clients.Clients(userChatConnectionIds)
                    .sendPrivateMessage(windowId, fromUserName, message, userimg, CurrentDateTime);
            }
            else
            {
                var messages = await _db.PrivateMessages.Where(x => x.FromUser.Equals(sender)
                                    && x.ToUser.Equals(toUserId)).ToListAsync();
                foreach (var dbMessage in messages)
                {
                    Clients.Clients(userChatConnectionIds)
                        .sendPrivateMessage(windowId, fromUserName, dbMessage, userimg, CurrentDateTime);
                }
            }
            //windowId = toUserId;
            // send to caller user
            //Clients.Caller.sendPrivateMessage(windowId, fromUserName, message, userimg, CurrentDateTime);
            //}
        }

        #region Old project Code

        //private List<ApplicationUser> _connectedUsers;
        //private static List<Message> _currentMessage;
        //private ApplicationDbContext _db;

        //public ChatHub()
        //{
        //    _db = new ApplicationDbContext();
        //    _connectedUsers = _db.Users.ToList();
        //    _currentMessage = _db.Messages.ToList();
        //}
        //public void Connect(string userName)
        //{
        //    var id = Context.User.Identity.GetUserId();

        // if (_connectedUsers.Count(x => x.Id == id) == 0) { string UserImg =
        // GetUserImage(userName); string logintime =
        // DateTime.Now.ToString(CultureInfo.InvariantCulture); _connectedUsers.Add(new
        // ApplicationUser { Id = id, UserName = userName, UserImage = UserImg, LoginTime = logintime });

        // // send to caller Clients.Caller.onConnected(id, userName, _connectedUsers, _currentMessage);

        //        // send to all except caller client
        //        Clients.AllExcept(id).onNewUserConnected(id, userName, UserImg, logintime);
        //    }
        //}

        //public void SendMessageToAll(string userName, string message, string time)
        //{
        //    string UserImg = GetUserImage(userName);
        //    // store last 100 messages in cache
        //    AddMessageinCache(userName, message, time, UserImg);

        // // Broad cast message Clients.All.messageReceived(userName, message, time, UserImg);

        //}

        //private void AddMessageinCache(string userName, string message, string time, string UserImg)
        //{
        //    _currentMessage.Add(new Message { UserName = userName, MessageText = message, Time = time, UserImage = UserImg });

        // if (_currentMessage.Count > 100) _currentMessage.RemoveAt(0);

        //    // Refresh();
        //}

        //public string GetUserImage(string username)
        //{
        //    string RetimgName = "images/dummy.png";
        //    //try
        //    //{
        //    //    string query = "select Photo from tbl_Users where UserName='" + username + "'";
        //    //    //string ImageName = ConnC.GetColumnVal(query, "Photo");

        //    //    if (ImageName != "")
        //    //        RetimgName = "images/DP/" + ImageName;
        //    //}
        //    //catch (Exception ex)
        //    //{ }
        //    return RetimgName;
        //}

        //public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        //{
        //    var item = _connectedUsers.FirstOrDefault(x => x.Id == Context.ConnectionId);
        //    if (item != null)
        //    {
        //        _connectedUsers.Remove(item);

        // var id = Context.ConnectionId; Clients.All.onUserDisconnected(id, item.UserName);

        //    }
        //    return base.OnDisconnected(stopCalled);
        //}

        #endregion Old project Code
    }

    public class PrivateMsgVm
    {
        public int PrivateMessageId { get; set; }
        public string RecieverId { get; set; }
        public string RecieverUserName { get; set; }
        public string SenderId { get; set; }
        public string Message { get; set; }
        public string MessageDate { get; set; }
    }
}