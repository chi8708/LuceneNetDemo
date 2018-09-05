using Lucene.Net.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace PanGuLucene
{
    public class PanGuConfig 
    {
        public static   void Init() 
        {

            //定义盘古分词的xml引用路径
            PanGu.Segment.Init(PanGuXmlPath);
        }


        /// <summary>
        /// 盘古分词的配置文件
        /// </summary>
        public static string PanGuXmlPath
        {
            get
            {
                return HttpContext.Current.Server.MapPath("/PanGu/PanGu.xml");
            }
        }

        /// <summary>
        /// 盘古分词器
        /// </summary>
        public static Analyzer PanGuAnalyzer
        {
            get { return new PanGuAnalyzer(); }
        }
    }
}