using DATN.Server.Data;
using DATN.Shared;
using System.Collections.Generic;
using System.Linq;

namespace DATN.Server.Service
{
    public class MessageService
    {
        private AppDBContext _context;
        public MessageService(AppDBContext context)
        {
            _context = context;
        }
        public List<Message> GetMessage()
        {
            return _context.Message.ToList();
        }
        public Message AddMessage(Message Message)
        {
            _context.Add(Message);
            _context.SaveChanges();
            return Message;
        }
        public Message DeleteMessage(int id)
        {
            var existing = _context.Message.Find(id);
            if (existing == null)
            {
                return null;
            }
            else
            {
                _context.Remove(existing);
                _context.SaveChanges();
                return existing;
            }
        }
        public Message GetIdMessage(int id)
        {
            var Message = _context.Message.Find(id);
            if (Message == null)
            {
                return null;
            }
            return Message;
        }
        public Message UpdateMessage(int id, Message update)
        {
            var existing = _context.Message.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.Note = update.Note;
            existing.CreateDate = update.CreateDate;
            existing.UpdateDate = update.UpdateDate;
            existing.AccountId = update.AccountId;
            existing.TableId = update.TableId;
            existing.MessageText = update.MessageText;

            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
