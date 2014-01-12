﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="FanfouDisney.cloud" %><!DOCTYPE html>
<html>
  <head>
    <title>饭团儿-没节操的饭否工具ˊ_>ˋ</title>
    <link rel="stylesheet" type="text/css" href="Scripts/jqcloud/jqcloud.css" />
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.4.4/jquery.js"></script>
    <script type="text/javascript" src="Scripts/jqcloud/jqcloud-1.0.4.js"></script>
    <script type="text/javascript">
        var word_list = <%=string.IsNullOrEmpty(tag) ? "[]":tag %>;
        $(function () {
            $("#my_favorite_latin_words").jQCloud(word_list);
        });
    </script>
  </head>
  <body>
    <p>当前用户：<input type="text" id="ipt_userid" value='<%=Session["queryId"] %>' /> <button id="btn_changeUser">切换用户</button></p>
    <div id="my_favorite_latin_words" style="width: 90% ;height:600px;font-size:16px; margin:0 auto;font-family:'Microsoft Yahei'"></div>
    <script>
        $("#btn_changeUser").click(function () {
            location.href = 'Default.aspx?id=' + $('#ipt_userid').val();
        });
    </script>
  </body>
</html>