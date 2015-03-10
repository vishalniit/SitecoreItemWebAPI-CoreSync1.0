Hi Folks,

This is Sitecore Package, which allow you to install the CoreSync Module.

This package is applicable to Sitecore Version 7.0 and prior Only.

For details on functionality of this tool please refer the online blog http://insitecore.blogspot.com or Git Repository https://github.com/vishalniit/SitecoreItemWebAPI-CoreSync1.0.

Following are the per-requisite before you install this package:
a) Your Sitecore Website instance folder should have appropriate permissions so that the installation wizard is able to override any config files if required in the include folder.
b) Your Server where this Sitecore website instance is installed, the PowerShell should be above 3.0 and installed.
c) Your Sitecore Website instance should have PowerShell extensions pre-installed.

After successful installation you should see the CoreSync as context menu option within the content editor while right clicking on any item or as context menu while clicking on any item.
In order to make this tool work you should also have another Sitecore instance where the same tool with all above mentioned same configuration is present.

You also need to have atleast one target setting item to be defined under following path /sitecore/system/Modules/CoreSync, without this there will be no target to select, Please also make sure that rules are defined properly at setting item as per your security standard defined.


After successful installation you should see the CoreSync as context menu option within the content editor while right clicking on any item or as context menu while clicking on any item.
In order to make this tool work you should also have another Sitecore instance where the same tool with all above mentioned same configuration is present.

You also need to have atleast one target setting item to be defined under following path /sitecore/system/Modules/CoreSync, without this there will be no target to select, Please also make sure that rules are defined properly at setting item as per your security standard defined.

Note:
If your .net runtime version is 4.0 or above please make sure that you have done following setting in your web.config, without it your default Sitecore update, new version create and simple create feature will not work.
By Default Sitecore nowdays comes with
<pages validateRequest="false"/>
but it is not effective until or unless we do following
<httpRuntime requestValidationMode="2.0"/>
or read this answer http://stackoverflow.com/questions/26555626/sitecore-7-2-item-web-api-unable-to-put-html-text

Please leave your feedback & comment for this tool.

Regards
Vishal Gupta
Mindtree