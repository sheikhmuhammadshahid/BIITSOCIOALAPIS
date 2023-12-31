﻿using BiitSocioApis.classes;
using BiitSocioApis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Twilio.TwiML.Messaging;

namespace BiitSocioApis.Controllers
{
    public class ChatController : ApiController
    {
        BIITSOCIOEntities db = new BIITSOCIOEntities();
        [HttpPost]
        public HttpResponseMessage addChat()
        {
            try
            {
                chat chatt = new chat();
                HttpRequest request = HttpContext.Current.Request;
                chatt.dateTime = DateTime.Now.ToShortTimeString();
                chatt.Date = DateTime.Now.ToShortDateString();
                chatt.receivedTime = DateTime.Now.ToString();
                chatt.userid = request["userid"];
                chatt.chat_id = request["chat_id"];
                chatt.text=request["text"];
                chatt.type=request["type"];
                
                if (request["type"] == "image" || request["type"] == "video")
                {
                    HttpPostedFile imagefile = request.Files["image"];
                    chatt.url = new PostController().saveImage(imagefile, chatt.userid, "postImages");
                }
              //  chatt.Date = DateTime.Now.ToShortDateString();
                db.chats.Add(chatt);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Added successfully!");
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }
        [HttpPost]
        public HttpResponseMessage deleteChat(int chatt_id)
        {
            try
            {
                var chat = db.chats.Where(s=>s.id==chatt_id).FirstOrDefault();
                if (chat != null)
                {
                    db.chats.Remove(chat);
                    db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Removed successfully!");
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }

        [HttpGet]
        public HttpResponseMessage checkMessages(string userId)
        {
            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                DateTime dt = DateTime.Now;
                string targetDateTimeString = DateTime.Now.AddSeconds(-55).ToString("yyyy-MM-dd HH:mm:ss");
               
                var data =db.chats.AsEnumerable().Where(s => s.chat_id == userId && DateTime.Parse(s.receivedTime) >= DateTime.Parse(targetDateTimeString) &&
                                DateTime.Parse(s.receivedTime) < DateTime.Now).ToList();
                List<object> list= new List<object>();
                object obj = new object();
                foreach (var item in data)
                {
                    obj= db.Users.AsEnumerable().Where(s => s.CNIC == item.userid).Select(s => new
                    {
                        profileImage = s.profileImage,
                        name = s.name,

                    }).FirstOrDefault();
                    if (obj == null)
                    {
                        obj = db.Groups.AsEnumerable().Where(s => s.id.ToString() == item.userid).Select(s => new
                        {
                            profileImage = s.profile,
                            name = s.name
                        }).FirstOrDefault();
                    }
                    list.Add(new
                    {
                        chat = new {
                            id = item.id,
                            type = item.type,
                            message = item.text,
                            url = item.url,
                            fromFile = false,
                            date = item.Date,
                            dateTime = item.dateTime,
                        },
                        sender =obj,
                    }); 
                }

                return Request.CreateResponse(HttpStatusCode.OK, list);

            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();

            }
        }
        [HttpGet]
        public HttpResponseMessage getChat(string loggedInUserId,string chatwithId)
        {
            try
            {
                /*var res=(from ch in db.chats join us in db.Users on ch.chat_id equals us.CNIC where (ch.chat_id==chatwithId &&
                         ch.userid==loggedInUserId)||(ch.chat_id == loggedInUserId &&
                         ch.userid == chatwithId) select new {id=ch.id, message=ch.text,sender=ch.userid==loggedInUserId?true:false,
                             senderImage=
                         }).ToList();*/
                var res = db.chats.Where(ch => (ch.chat_id == chatwithId &&
                         ch.userid == loggedInUserId) || (ch.chat_id == loggedInUserId &&
                         ch.userid == chatwithId)).Select(s => new {
                             id = s.id,
                             type=s.type,
                             message = s.text,
                             url=s.url,
                             fromFile=false,
                             date =s.Date,
                             dateTime=s.dateTime,
                             sender = s.userid == loggedInUserId ? true : false,
                             senderImage =db.Users.Where(us=>us.CNIC==s.userid.Trim()).Select(us=>us.profileImage).FirstOrDefault() }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }
        [HttpGet]
        public HttpResponseMessage getChatsForHistory(string userCnic, int lastSaveId)
        {
            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                List<chat> posts = new List<chat>();
                var user = db.Users.Where(s => s.CNIC == userCnic).FirstOrDefault();

                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, "something gone wrong!");
            }
        }

    }
}
