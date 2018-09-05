(function () {
    if (!window.SC) { window.SC = {} }
    SC.Page = {
        //提交
        Submit: function () {
            $("#action").val("SearchIndex");
            var form = document.getElementById("form1");
            var pathname = location.pathname;
            var url = pathname.substr(pathname.lastIndexOf("/") + 1);
            form.action = url + location.search;
            form.method = "post";
            form.submit();
            //$("#isSearch").val("0");
        },
        //获取页码
        GetPage: function (pageIndex) {
            $("#pageIndex").val(pageIndex);
            this.Submit();
        },
        GotoPage: function () {
            var pageIndex = $("#pageIndex").val();
            if (!/^[0-9]+\.?[0-9]*$/.test(pageIndex)) {
                alert("页码必须为整数");
                return false;
            }
            else {
                if (pageIndex > $("#pageCount").html()) {
                    alert("页码必须小于总页数");
                    return false;
                }
                else
                    this.GetPage(pageIndex);
            }
        },
        //改变现实条数
        ChangeSize: function () {
            $("#pageIndex").val(1);
            this.Submit();
        },
        //删除后刷新
        Del: function () {
            var id = [];
            $.each($("input[name='chk']:checked"), function (i, n) {
                id.push(n.id);
            });
            if (id.length <= 0) {
                alert("至少选择一个进行删除,请重新选择");
                return false;
            }
            else {
                var ids = id.join(",");
                $("#ids").val(ids);
                if (confirm("确定要删除吗?"))
                    this.Submit();
            }
        }
    };

} ());
 