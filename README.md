# SitecoreItemWebAPI-CoreSync1.0
I enhanced the Sitecore Item WebAPI 1.2 to include few new features because Item Web API does not support create the versioned (Language or Numbered) item. The media item support is also not great with Item Web API. The biggest support I am looking was, what If, if I want to create an Item while retaining the Item ID on the new Sitecore System. Such can only happen through two ways in Sitecore one is Plain Serialization or it’s another popular derivative which is Sitecore packaging.
Now to overcome these challenges I decide to extend the Item Web API in such a manner so that it can do lot more than what it made for. I hope community like this extention.

What is Pre-Requisite?

 You need to installed Sitecore Item Web API 1.2 if you are using Sitecore version prior to 7.2. If you are using Sitecore 7.2 then Item Web API is part and parcel of Sitecore Package.
 If your .net runtime version is 4.0 or above please make sure that you have done following setting in your web.config, without it your default Sitecore update, new version create and simple create feature will not work.
By Default Sitecore nowdays comes with
<pages validateRequest="false"/>
but it is not effective until or unless we do following
<httpRuntime requestValidationMode="2.0"/>
or read this answer http://stackoverflow.com/questions/26555626/sitecore-7-2-item-web-api-unable-to-put-html-text
 
What is specific pre-requisite for using this tool from powershell remoting ?
Please make sure the machine where your powershell script is stored, you have latest Powershell extention module in action, for example it is required to have version 3.0 & above.

How to Use from this GitHub?

 Download the whole project to your local and build it. Once successfully build you need to copy basically two .dll file "Mindtree.Sitecore.ItemWebApi.Pipelines.dll" and "Mindtree.Sitecore.WebApi.Client.dll" to your target Sitecore instance's Bin directory. There is new config file which also you need to copy to your include folder 'ZMindtree.ItemWebApi.config', this file patches the existing Sitecore.ItemWebApi.config file at runtime. Remember this file was based on Sitecore 7.2 and than extended further. (Explained in blog).

Can't I Simply have the Sitecore package which I can instal straight away ?

 Yes you can do so, please visit Sitecore marketplace and search for CoreSync, Under download you will find the zip based package which just had the service and config included. But remember if you directly install this on production server it will trigger the AppPool Refresh which may not be desired.
 
What are these .dll files?

Mindtree.Sitecore.ItemWebApi.Pipelines.dll : This library contains the enhanced code for extended Sitecore Item Web API.
Mindtree.Sitecore.WebApi.Client.dll : This library contains the .net wrapper which allow you to call the JSON/XML based service from any .net based application.

What is the role of this 'Mindtree.Sitecore.WebApiClient.Demo' ?

This project is a console application which provide possible implementations of the service using the .net wrapper stated above.

How to use this tool ?
Please find videos listed on my blog or sitecore market place

To read more about this tool and extended webservice please follow my blog http://insitecore.blogspot.com 
Please also point out issues and suggestion in order to optmize this thread.
