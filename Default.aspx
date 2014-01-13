<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="FanfouDisney.cloud" %><!DOCTYPE html>
<html>
  <head>
    <title>饭团儿-没节操的饭否工具ˊ_>ˋ</title>
    <link rel="stylesheet" type="text/css" href="Scripts/jqcloud/jqcloud.css" />
    <link rel="Stylesheet" type="text/css" href="CSS/global.css" />
    <script type="text/javascript" src="http://code.jquery.com/jquery-latest.js"></script>
    <script type="text/javascript" src="Scripts/jqcloud/jqcloud-1.0.4.js"></script>
  </head>
  <body>
    <div class="query">输入用户ID：<input type="text" id="ipt_userid" value='<%=Session["queryId"] %>' /> <button id="btn_changeUser">查　询</button></div>
    <div id="my_favorite_latin_words" style="width: 90% ;height:600px;font-size:16px; margin:0 auto;font-family:'Microsoft Yahei'"></div>
    <script type="text/javascript">
        $("#btn_changeUser").click(function () {
            location.href = 'Default.aspx?id=' + $('#ipt_userid').val();
        });
        var word_list = <%=string.IsNullOrEmpty(tag) ? "[]":tag %>;
        $(function () {
            $("#my_favorite_latin_words").jQCloud(word_list);
        });
    </script>
  </body>
</html>
