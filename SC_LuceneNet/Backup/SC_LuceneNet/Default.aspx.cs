using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Lucene.Net;
using PanGu;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.PanGu;
using System.IO;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using SC_LuceneNet.Model;
using System.Text;
using PanGu.Match;
using PanGu.HighLight;
using System.Diagnostics;

namespace SC_LuceneNet
{
    public partial class _Default : System.Web.UI.Page
    {
        private string strIndexPath =string.Empty;
        protected string txtTitle = string.Empty;
        protected string txtContent = string.Empty;
        protected long lSearchTime = 0;
        protected IList<Article> list=new List<Article>();
        protected string txtPageFoot = string.Empty;
        protected void Page_Load(object sender, EventArgs e)
        {
            //定义盘古分词的xml引用路径
            PanGu.Segment.Init(PanGuXmlPath);
            switch (Action)
            {
                case "CreateIndex": CreateIndex(Cover); break;
                case "SearchIndex": SearchIndex(); break;
            }
            //Term t = new Term("content", "刘备");
            //Query query = new TermQuery(t);

            //TermQuery termQuery1 = new TermQuery(new Term("content", "刘备"));
            //TermQuery termQuery2 = new TermQuery(new Term("title", "三国"));
            //BooleanQuery booleanQuery = new BooleanQuery();
            //booleanQuery.Add(termQuery1, BooleanClause.Occur.SHOULD);
            //booleanQuery.Add(termQuery2, BooleanClause.Occur.SHOULD);

            //Query query = new WildcardQuery(new Term("content", "三国*"));

          RangeQuery query = new RangeQuery(new Term("time","20060101"), new Term("time","20060130"), true);
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        private void CreateIndex(bool isCreate)
        {
            //创建索引目录
            if (!Directory.Exists(IndexDic))
            {
                Directory.CreateDirectory(IndexDic);
            }
            //IndexWriter第三个参数:true指重新创建索引,false指从当前索引追加....此处为新建索引所以为true
            IndexWriter writer = new IndexWriter(IndexDic, PanGuAnalyzer, isCreate, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
            for (int i = 1; i < 150; i++)
            {
                AddIndex(writer, "我的标题" + i, i+"标题内容是飞大师傅是地方十大飞啊的飞是 安抚爱上地方 爱上地方" + i,DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"));
                AddIndex(writer, "射雕英雄传作者金庸" + i, i + "我是欧阳锋" + i, DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"));
                AddIndex(writer, "天龙八部2" + i, i + "慕容废墟,上官静儿,打撒飞艾丝凡爱上,虚竹" + i, DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"));
                AddIndex(writer, "倚天屠龙记2" + i, i + "张无忌机" + i, DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"));
                AddIndex(writer, "三国演义" + i, i + "刘备,张飞,关羽" + i, DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"));
            }
            writer.Optimize();
            writer.Close();
            Response.Write("<script type='text/javascript'>alert('创建索引成功');window.location=window.location;</script>");
            Response.End();
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="analyzer"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        private void AddIndex(IndexWriter writer, string title, string content,string date)
        {
            try
            {
                Document doc = new Document();
                doc.Add(new Field("Title", title, Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                doc.Add(new Field("Content", content, Field.Store.YES, Field.Index.ANALYZED));//存储且索引
                doc.Add(new Field("AddTime", date, Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                writer.AddDocument(doc);
            }
            catch (FileNotFoundException fnfe)
            {
                throw fnfe;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 从索引搜索结果
        /// </summary>
        private void SearchIndex()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            BooleanQuery bQuery = new BooleanQuery();
            string title = string.Empty;
            string content = string.Empty;
            if (Request.Form["title"] != null && Request.Form["title"].ToString()!="")
            {
                title =GetKeyWordsSplitBySpace( Request.Form["title"].ToString());
                QueryParser parse = new QueryParser("Title", PanGuAnalyzer);
                Query query = parse.Parse(title);
                parse.SetDefaultOperator(QueryParser.Operator.AND);
                bQuery.Add(query, BooleanClause.Occur.MUST);
                dic.Add("title",Request.Form["title"].ToString());
                txtTitle = Request.Form["title"].ToString();
            }
            if (Request.Form["content"] != null && Request.Form["content"].ToString() != "")
            {
                content = GetKeyWordsSplitBySpace(Request.Form["content"].ToString());
                QueryParser parse = new QueryParser("Content", PanGuAnalyzer);
                Query query = parse.Parse(content);
                parse.SetDefaultOperator(QueryParser.Operator.AND);
                bQuery.Add(query, BooleanClause.Occur.MUST);
                dic.Add("content",Request.Form["content"].ToString());
                txtContent = Request.Form["content"].ToString();
            }
            if (bQuery != null && bQuery.GetClauses().Length>0)
            {
                GetSearchResult(bQuery, dic);
            }
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="bQuery"></param>
        private void GetSearchResult(BooleanQuery bQuery,Dictionary<string,string> dicKeywords)
        {          
            IndexSearcher search = new IndexSearcher(IndexDic,true);
            Stopwatch stopwatch = Stopwatch.StartNew();
            //SortField构造函数第三个字段true为降序,false为升序
            Sort sort = new Sort(new SortField("AddTime", SortField.DOC, true));
            TopDocs docs = search.Search(bQuery, (Filter)null, PageSize * PageIndex, sort);
            stopwatch.Stop();
            if (docs != null && docs.totalHits > 0)
            {
                lSearchTime = stopwatch.ElapsedMilliseconds;
                txtPageFoot = GetPageFoot(PageIndex, PageSize, docs.totalHits, "sabrosus");
                for (int i = 0; i < docs.totalHits; i++)
                {
                    if (i >= (PageIndex - 1) * PageSize && i < PageIndex * PageSize)
                    {
                        Document doc = search.Doc(docs.scoreDocs[i].doc);
                        Article model = new Article()
                        {
                            Title = doc.Get("Title").ToString(),
                            Content = doc.Get("Content").ToString(),
                            AddTime = doc.Get("AddTime").ToString()
                        };
                        list.Add(SetHighlighter(dicKeywords, model));
                    }
                }
            }
        }

        /// <summary>
        /// 按钮事件
        /// </summary>
        protected string Action
        {
            get
            {
                if (Request.Form["action"] != null)
                {
                    return Request.Form["action"].ToString();
                }
                else
                {
                    return "";
                }
            }
        }
        /// <summary>
        /// 索引存放目录
        /// </summary>
        protected string IndexDic
        {
            get
            {
                return Server.MapPath("/IndexDic");
            }
        }

        /// <summary>
        /// 盘古分词的配置文件
        /// </summary>
        protected string PanGuXmlPath
        {
            get {
                return Server.MapPath("/PanGu/PanGu.xml");
            }
        }

        /// <summary>
        /// 盘古分词器
        /// </summary>
        protected Analyzer PanGuAnalyzer
        {
            get { return new PanGuAnalyzer(); }
        }

        /// <summary>
        /// 处理关键字为索引格式
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        private string GetKeyWordsSplitBySpace(string keywords)
        {
            PanGuTokenizer ktTokenizer=new PanGuTokenizer ();
            StringBuilder result = new StringBuilder();
            ICollection<WordInfo> words = ktTokenizer.SegmentToWordInfos(keywords);
            foreach (WordInfo word in words)
            {
                if (word == null)
                {
                    continue;
                }
                result.AppendFormat("{0}^{1}.0 ", word.Word, (int)Math.Pow(3, word.Rank));
            }
            return result.ToString().Trim();
        }

        /// <summary>
        /// 设置关键字高亮
        /// </summary>
        /// <param name="dicKeywords">关键字列表</param>
        /// <param name="model">返回的数据模型</param>
        /// <returns></returns>
        private Article SetHighlighter(Dictionary<string, string> dicKeywords, Article model)
        {
            SimpleHTMLFormatter simpleHTMLFormatter = new PanGu.HighLight.SimpleHTMLFormatter("<font color=\"green\">", "</font>");
            Highlighter highlighter = new PanGu.HighLight.Highlighter(simpleHTMLFormatter, new Segment());
            highlighter.FragmentSize = 50;
            string strTitle = string.Empty;
            string strContent = string.Empty;
            dicKeywords.TryGetValue("title", out strTitle);
            dicKeywords.TryGetValue("content", out strContent);
            if (!string.IsNullOrEmpty(strTitle))
            {
                model.Title = highlighter.GetBestFragment(strTitle, model.Title);
            }
            if (!string.IsNullOrEmpty(strContent))
            {
                model.Content = highlighter.GetBestFragment(strContent, model.Content);
            }
            return model;
        }

        /// <summary>
        /// 页大小
        /// </summary>
        private int PageSize
        {
            get {
                if (Request.Form["pageSize"] != null)
                {
                    return Convert.ToInt32(Request.Form["pageSize"]);
                }
                else
                {
                    return 10;
                }
            }
        }

        /// <summary>
        /// 页码
        /// </summary>
        private int PageIndex
        {
            get
            {
                if (Request.Form["pageIndex"] != null)
                {
                    return Convert.ToInt32(Request.Form["pageIndex"]);
                }
                else
                {
                    return 1;
                }
            }
        }

        /// <summary>
        /// 是否覆盖索引
        /// </summary>
        private bool Cover
        {
            get
            {
                if (Request.Form["cover"] != null)
                {
                    if (Request.Form["cover"] == "1")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else return true;
            }
        }

        /// <summary>
        /// 分页页脚
        /// </summary>
        /// <param name="currentPageIndex">当前页</param>
        /// <param name="pageSize">记录条数</param>
        /// <param name="total">记录总数</param>
        /// <param name="cssName">css样式名称</param>
        /// <returns></returns>
        private string GetPageFoot(int currentPageIndex, int pageSize, int total, string cssName)
        {
            currentPageIndex = currentPageIndex <= 0 ? 1 : currentPageIndex;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            string options = string.Empty;
            int pageCount = 0;//总页数
            int pageVisibleCount = 10; // 显示数量
            if (total % pageSize == 0)
            {
                pageCount = total / pageSize;
            }
            else
            {
                pageCount = total / pageSize + 1;
            }
            //如果是整除的话,退后一页
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<div class=\"page_left\">一页显示<select id=\"pageSize\" name=\"pageSize\" onchange =\"SC.Page.ChangeSize();\">{0}</select>&nbsp;条&nbsp;&nbsp;&nbsp;总共{1}条</div>", SetOption(pageSize), total);
            sb.AppendFormat("<div class=\"page_right\">跳转到第<input type=\"text\" id=\"pageIndex\" name=\"pageIndex\" value=\"{0}\" />页<a href=\"#\" class=\"easyui-linkbutton\" plain=\"true\" iconCls=\"icon-redo\" onclick=\"SC.Page.GotoPage();\">Go</a>共<span id=\"pageCount\">" + pageCount + "</span>&nbsp;页</div><input type=\"hidden\" id=\"isSearch\" name=\"isSearch\" value=\"1\" />", currentPageIndex);

            sb.Append("<div class='" + cssName + "'>");// sbrosus分页样式，需要自己添加哇


            if (currentPageIndex == 1 || total < 1)
            {
                sb.Append("<span ><a href='javascript:void(0)'>首页</a></span>");
                sb.Append("<span ><a href='javascript:void(0)'>上一页</a></span>");
            }
            else
            {
                sb.Append("<span><a onclick=\"SC.Page.GetPage(1)\">首页</a></span>");
                sb.Append("<span><a onclick=\"SC.Page.GetPage(" + (currentPageIndex - 1).ToString() + ")\">上一页</a></span>");
            }
            int i = 1;
            int k = pageVisibleCount > pageCount ? pageCount : pageVisibleCount;
            if (currentPageIndex > pageVisibleCount)
            {
                i = currentPageIndex / pageVisibleCount * pageVisibleCount;
                k = (i + pageVisibleCount) > pageCount ? pageCount : (i + pageVisibleCount);
            }
            for (; i <= k; i++)//k*10防止k为负数
            {
                if (i == currentPageIndex)
                {
                    sb.AppendFormat("<span class='current' ><a href='javascript:void(0)'>{0}</a></span>&nbsp;", i);
                }
                else
                {
                    sb.AppendFormat("<span><a onclick=\"SC.Page.GetPage(" + i + ")\" >{0}</a></span>&nbsp;", i);
                }
            }
            if (currentPageIndex == pageCount || total < 1)
            {
                sb.Append("<span ><a href='javascript:void(0)'>下一页</a></span>");
                sb.Append("<span ><a href='javascript:void(0)'>尾页</a></span>");
            }
            else
            {
                sb.Append("<span><a onclick=\"SC.Page.GetPage(" + (currentPageIndex + 1).ToString() + ")\">下一页</a></span>");
                sb.Append("<span><a onclick=\"SC.Page.GetPage(" + pageCount + ")\">尾页</a></span></div>");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 根据pagesize获取select标签
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        private string SetOption(int pageSize)
        {
            StringBuilder sb_options = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                if (pageSize / 10 == i + 1)
                {
                    sb_options.AppendFormat("<option selected=\"selected\">{0}0</option>", (i + 1).ToString());
                }
                else
                {
                    sb_options.AppendFormat("<option>{0}0</option>", (i + 1).ToString());
                }
            }
            if (pageSize == 1000)
            {
                sb_options.Append("<option selected=\"selected\">1000</option>");
            }
            else
            {
                sb_options.Append("<option >1000</option>");
            }

            return sb_options.ToString();
        }


    }
}