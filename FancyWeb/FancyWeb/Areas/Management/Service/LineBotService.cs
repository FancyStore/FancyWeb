﻿using FancyWeb.Areas.Management.ViewModels;
using FancyWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FancyWeb.Areas.Management.Service
{
    public class LineBotService
    {
        private FancyStoreEntities db = new FancyStoreEntities();

        public string Getpupp(string url)
        {
            var pupp = db.OrderDetails.AsEnumerable().GroupBy(n => n.ProductID).Select(n => new { n.Key, sum = n.Sum(m => m.OrderQTY) }).OrderByDescending(n => n.sum).Take(3).ToList();
            string LineMessage = "";
            string[] i = { "1⃣", "2⃣", "3⃣" };
            int j = 0;
            foreach (var item in pupp)
            {
                var product = db.Products.Find(item.Key);
                LineMessage += $"🔶{i[j]} {product.ProductName}\n【價格】:{product.UnitPrice}\n[商品連接]➡️{url}/{product.ProductID}\n";
                j++;
            }
            return LineMessage;
        }

        public string GetActityP(string url)
        {
            var Activity = db.ActivityProducts.OrderBy(n => Guid.NewGuid()).Take(3).Select(n => new
            {
                Pid = n.ProductID,
                pname = n.Product.ProductName,
                Avname = n.Activity.ActivityName,
                oup = n.Product.UnitPrice,
                nup = n.Product.UnitPrice * n.Activity.DiscountMethod.Discount
            }).ToList();
            string LineMessage = "🔥活動商品! 還不快出手🔥\n\n";
            foreach (var item in Activity)
            {
                LineMessage += $"🔸[{item.Avname}] = >    {item.pname}\n原價: NT$ {item.oup} 優惠價: {item.nup.ToString("C0")}\n商品連接  → {url}/{item.Pid} \n\n";
            }
            LineMessage += "活動期間好康大放送，加緊你的腳步❗❗\n";
            return LineMessage;
        }

        //推薦
        public List<Line_Template> Line_Templates(string url)
        {
            var romd = db.Products.OrderBy(n => Guid.NewGuid()).Take(3).Select(n => new
            {
                pname = n.ProductName,
                pup = n.UnitPrice,
                pid = n.ProductID
            }).ToList();
            List<Columns> columns = new List<Columns>();
            foreach (var item in romd)
            {
                List<ViewModels.Action> actions = new List<ViewModels.Action>();
                actions.Add(new ViewModels.Action
                {
                    type = "uri",
                    label = "前往商品頁面",
                    uri = "https://msit12201.azurewebsites.net/ProductDisplay/Product/GetProductDetail/" + item.pid,

                });
                columns.Add(new Columns
                {
                    thumbnailImageUrl = "https://msit12201.azurewebsites.net/ProductDisplay/Product/ByteImage/" + item.pid,
                    title = item.pname,
                    text = "NT$ " + item.pup.ToString(),
                    actions = actions
                });
            }
            Line_Template ddd = new Line_Template()
            {
                template = new Template
                {
                    type = "carousel",
                    columns = columns
                }
            };
            List<Line_Template> _Templates = new List<Line_Template>();
            _Templates.Add(ddd);

            return _Templates;
        }
        //取消訂單
        public string CancelOrder(string uname, string ordernum)
        {
            var haveorder = db.OrderHeaders.Where(n => n.OrderNum == ordernum && n.User.UserName == uname).FirstOrDefault();
            if (haveorder == null)
            {
                return "無此訂單";
            }
            else if (haveorder.OrderStatusID != 1)
            {
                return "訂單狀態無法取消";
            }
            else
            {
                haveorder.OrderStatusID = 3;
                db.SaveChanges();
                return "成功取消訂單";
            }
        }
        //查詢訂單
        public List<LineOrderHeader> SearchOrder(string uname, string count)
        {
            List<LineOrderHeader> data = new List<LineOrderHeader>();
            data = db.OrderHeaders.Where(n=>n.User.UserName == uname).OrderByDescending(n=>n.CreateDate).Take(Convert.ToInt32(count)).Select(n => new LineOrderHeader
            {
                ordernum = n.OrderNum,
                orderstatus = n.OrderStatusList.OrderStatusName,
                amount = n.OrderAmount,
                orderdetail = n.OrderDetails.Select(m => new LineOrderDetail
                {
                    pname = m.Product.ProductName,
                    pQTY = m.OrderQTY,
                    pUP = m.UnitPrice
                }).ToList()
            }).ToList();
            return data;
        }
        public class LineOrderHeader
        {
            public string ordernum { get; set; }
            public string orderstatus { get; set; }
            public int amount { get; set; }
            public List<LineOrderDetail> orderdetail { get; set; }
        }
        public class LineOrderDetail
        {
            public string pname { get; set; }
            public int pQTY { get; set; }
            public int pUP { get; set; }
        }
    }
}