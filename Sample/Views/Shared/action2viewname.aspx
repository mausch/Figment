<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<h1>Hello world from view</h1>
<form action="/action6" method="post">
	<input name="somefield"/>
	<input type="submit" value="POST"/>
</form>
<hr/>
<form action="/action6" method="get">
	<input name="firstname"/>
	<input name="lastname"/>
	<input type="submit" value="GET"/>
</form>
