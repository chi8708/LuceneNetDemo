<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SC_LuceneNet._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SC--LuceneNet+盘古分词器</title>
    <script src="Scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
    <script src="Scripts/SC.js" type="text/javascript"></script>
    <link href="Style/index.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript">
        function doclick(action) {
            $("#action").val(action);
            $("#form1").submit();
        }
    </script>
</head>
<body>
    <form id="form1" runat="server" action="Default.aspx">
    <div>
        <h1>LuceneNet+盘古分词器+关键字高亮</h1>
        <hr />
    </div>
    <div id="SearchIndex">
            标题：<input type="text" name="title" class="input_text" value="<%=txtTitle %>" />
            内容：<input type="text" name="content" class="input_text" value="<%=txtContent %>" />
            <input type="submit" value="搜索"  onclick="doclick('SearchIndex')"/><input type="button" value="创建索引" onclick="doclick('CreateIndex')" />
            是否覆盖索引：<input type="radio" checked="checked" id="isCover" name="cover" value="1" /><label style="margin-right:2px;" for="isCover">是</label><input type="radio" id="notCover" name="Cover" value="0" /><label for="notCover">否</label>
    </div>
    <div>
    <table class="table_list cellpadding="0" cellspacing="0">
    <tbody>
    <%
        if (list != null && list.Count > 0)
        {
            %>
         <tr>
            <td>标题</td>
            <td>内容</td>
            <td>添加时间</td>
        </tr>
            <%
            foreach (SC_LuceneNet.Model.Article obj in list)
            {
            %>
            <tr>
                <td><%=obj.Title%></td>
                <td><%=obj.Content%></td>
                <td><%=obj.AddTime%></td>
            </tr>
            <%
            }
            %>
                <tr><td colspan="3" style="text-align:left;">一共找到<strong><%=list.Count%></strong>条数据，共耗费<strong><%=lSearchTime%></strong>毫秒</td></tr>
            <%
        }
         %>
         </tboyd>
         <%
              if (list != null && list.Count > 0)
        {
              %>
         <tfoot>
            <tr><td colspan="3"><%=txtPageFoot %></td></tr>
         </tfoot>
         <%} %>
    </table>
        
    </div>
     <input type="hidden" name="action" id="action" value="default" />
    </form>
</body>
</html>
