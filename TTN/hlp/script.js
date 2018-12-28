function SetDefaultPage()
{
	if (parent == null) return;

	var data = {};
	
    if(parent.window.location.search) 
    {
       var pair = (parent.location.search.substr(1)).split('&');
        for(var i = 0; i < pair.length; i ++) 
        {
			var param = pair[i].split('=');
			data[param[0]] = param[1];
        }
    }
    if (data["page"]!=null)
    {  
    	if (document.all(data["page"]))
    	{
			document.all(data["page"]).click();
			document.all(data["page"]).focus();
		}			
	}
}

function OpenPageInHelp(pageId)
{
	OpenPageInHelp(pageId, undefined);
}

function OpenPageInHelp(pageId, anchor)
{
	if (!parent || !parent.index || !parent.index.document.all(pageId))
	{
		alert("Ссылка на страницу с id='"  + pageId + "' не найдена или у документа неверная структура!");
		return;
	}

	var obj = parent.index.document.all(pageId);
	
	var oldHref = obj.href;

	if( obj!==undefined && anchor!==undefined )
		obj.href += '#' + anchor;

	obj.click();
	obj.href = oldHref;
	obj.focus();
}

