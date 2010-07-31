<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<h1>Hello world from view</h1>
<form action="/action6" method="post">
	<input name="somefield" value="<%= Model.firstName %> <%= Model.lastName %>"/>
	<input type="submit" value="POST"/>
</form>
<hr/>
<form action="/action6" method="get">
	<input name="firstname" value="<%= Model.firstName %>"/>
	<input name="lastname" value="<%= Model.lastName %>"/>
	<input name="age" value="<%= Model.age %>"/>
	<input type="submit" value="GET"/>
</form>
