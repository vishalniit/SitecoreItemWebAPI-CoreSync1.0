<# 
  Sitecore PowerShell Remoting.

  Sample Execute Script Block on a Sitecore Server:
    Set-SitecoreConfiguration 'http://hostname' 'admin' 'b'
    Invoke-SitecoreScript { get-item master:\content\ } @{name = "value"}

  Sample upload local C:\image.png to a Sitecore Server to "/sitecore/ media library/Path/an_image.png" path:
    Set-SitecoreConfiguration 'http://hostname' 'admin' 'b'
    Get-Item C:\image.png | Upload-SitecoreFile -remotePath "Path\an_image.png"
    
  Sample upload remote "/sitecore/ media library/Path/an_image.png" from local C:\image.png:
    Set-SitecoreConfiguration 'http://hostname' 'admin' 'b'
    Upload-SitecoreFile -remotePath "Path\an_image" -File C:\image.jpg

Naturally update the host name, credentials and parameters with the ones meeting your use case.
    
#>

function Set-SitecoreConfiguration {
    [CmdletBinding()]
    param(
        [Parameter(Position=0, Mandatory=$true, ValueFromPipeline=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$SitecoreHost,

        [Parameter(Position=1, Mandatory=$true, ValueFromPipeline=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$User,
        
        [Parameter(Position=2, Mandatory=$true, ValueFromPipeline=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$Password
    )
    $URI = $SitecoreHost + "/Console/Services/RemoteAutomation.asmx";
    $GLOBAL:SpeRemoteProxy = New-WebServiceProxy -Uri $URI
    $GLOBAL:SpeRemoteUser = $User;
    $GLOBAL:SpeRemotePassword = $Password;
}

function Invoke-SitecoreScript {
    [CmdletBinding()]
    param(
        [Parameter(Position=0, Mandatory=$true, ValueFromPipeline=$true)]
        [ValidateNotNullOrEmpty()]
        [ScriptBlock]$Command,
        
        [Parameter(Position=1, ValueFromPipeline=$true)]
        [Hashtable]$Params = @{noParameters=$true}
    )
	
    $sb = New-Object System.Text.StringBuilder;
    $settings = New-Object System.Xml.XmlWriterSettings -Property @{CloseOutput = $true; Encoding = [System.Text.Encoding]::UTF8; Indent = $false; OmitXmlDeclaration = $true};
    $xw = [System.Xml.XmlWriter]::Create($sb, $settings);
    $type = $type = [PSObject].Assembly.GetType("System.Management.Automation.Serializer");
    $ctor = $type.GetConstructor("instance,nonpublic", $null, @([Xml.XmlWriter]), $null);
    $serializer = $ctor.invoke($xw);
    $method = $type.GetMethod("Serialize", "invokemethod,nonpublic,instance", $null, @([object]), @());
    $done = $type.GetMethod("Done", [System.Reflection.BindingFlags]"nonpublic,instance");
    $method.Invoke($serializer, @($Params)) | Out-Null;
    $done.Invoke($serializer, @()) | Out-Null;
    $cliXmlArgs = $sb.ToString();
    $xw.Close();

    $reply = $GLOBAL:SpeRemoteProxy.ExecuteScriptBlock($GLOBAL:SpeRemoteUser, $GLOBAL:SpeRemotePassword, $Command, $cliXmlArgs);

    $xmlString = $reply -replace "\n", "" -replace "\r",""
    $sr = New-Object System.IO.StringReader $xmlString
    $xr = New-Object System.Xml.XmlTextReader $sr
    $type = $type = [PSObject].Assembly.GetType("System.Management.Automation.Deserializer")
    $ctor = $type.GetConstructor("instance,nonpublic", $null, @([Xml.XmlReader]), $null)
    $deserializer = $ctor.Invoke($xr)
    $method = $type.GetMethod("Deserialize", "nonpublic,instance", $null, @(), @())
    $done = $type.GetMethod("Done", [System.Reflection.BindingFlags]"nonpublic,instance")
    while (!$done.Invoke($deserializer, @()))
    {
        try {
            $value = $method.Invoke($deserializer, @())
       	    return $value
        } catch [Exception] {
            write-warning "Error while de-serializing string: $($error[0])"
            break;
        }
    }
    $xr.Close()
    $sr.Dispose()
}

function Upload-SitecoreFile {
    [CmdletBinding()]
    param(
        [Parameter(Position=0, Mandatory=$true, ValueFromPipeline=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$File,
        
        [Parameter(Position=1)]
		[ValidateNotNullOrEmpty()]
        [String]$RemotePath,

        [Parameter(Position=2)]
        [String]$Database = "master",

        [Parameter(Position=3)]
        [String]$Language = "en"
    )
	
    $bytes = [System.IO.File]::ReadAllBytes($file);
    $reply = $GLOBAL:SpeRemoteProxy.UploadFile($GLOBAL:SpeRemoteUser, $GLOBAL:SpeRemotePassword, $RemotePath, $bytes, $Database, $Language);
}

function Download-SitecoreFile {
    [CmdletBinding()]
    param(
        [Parameter(Position=0, Mandatory=$true, ValueFromPipeline=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$File,
        
        [Parameter(Position=1)]
		[ValidateNotNullOrEmpty()]
        [String]$RemotePath,

        [Parameter(Position=2)]
        [String]$Database = "master",

        [Parameter(Position=3)]
        [String]$Language = "en"
    )
	
    $bytes = $GLOBAL:SpeRemoteProxy.DownloadFile($GLOBAL:SpeRemoteUser, $GLOBAL:SpeRemotePassword, $RemotePath, $Database, $Language);
    if(-not(test-path $file))
    {
        New-Item -ItemType File -Path $File
    }

    $FileName = Convert-Path -path $File
    [System.IO.File]::WriteAllBytes($FileName, $bytes);
}
# CoreSync - 1.0
# Pre-Requisite - PowerShell 3.0 to be installed in the Source machine and "sitecore/system/Modules/PowerShell/Script Library/CoreSync/SyncorBackupFunc" item should exist in master DB.
#Parameters and actual Calling Line - Need to Configure here, you can create copy of this script to create different flavour of same work.

#Sitecore Source Script Path
$ScriptItemPath="master:/sitecore/system/Modules/PowerShell/Script Library/CoreSync/SyncorBackupFunc/";

#Source Machine or the machine from where you want take your data or from where you want to sync the data - Change the Credentials
$SourcetHostname="http://source.brand.com";
$SourceUsername="sitecore\admin";
$SourcePassword="b";

#Target machine or the machine where you want to put the backup or where you want to sync the data - Change the Credentials
$TargetHostname="http://target.brand.com";
$TargetUsername="sitecore\admin";
$TargetPassword="b";

#Following is the sample data filled, please change the data way you prefer
$data=@{
#Source item
Item="{30F62584-2808-4DA6-BD4D-1886CDEE670D}";
#Exclusion List - This will be used only if IncludeChildren property is true and it is optional.
ItemList="";
#Target machine Sitecore username
UserName=$TargetUsername;
#Target Machine Sitecore Password
Password=$TargetPassword;
#Target Machine Sitecore Host URL
HostURL=$TargetHostname;
#Target Machine Database, if not provided "master" will be taken as default.
DBName="master";
#Target Machine Language, if not provided "en" will be taken as default.
LangName="en";
#It will look recursively for all of the children of Item provided as first argument, This is only applicable if 'LookupMode' is false which means it will look item from the content tree.
IncludeChildren="true";
#Here BackupMode=True means it will always create the version while updating data into target system, but if false than it will just updated the target item.
BackupMode="false";
# It is only useful if this data is targeted towards single item and purpose is to update/create/version single item, Bydefault always latest version will be picked. 
VersionNumber="0";
#Here LookupMode=False means that it will look from the item in source system using content tree search, but if it is false it will look using History engine for last updated, version added, created events.
LookupMode="false";
#This property is only usefull and necessary if LookupMode=true as then it requires to know from since when server needs to look into history engine.
DateTime="";
}

#Set-SitecoreConfiguration '<yourhost>' '<SitecoreUserName>' '<password>'
Set-SitecoreConfiguration $SourcetHostname $SourceUsername $SourcePassword
Invoke-SitecoreScript {Get-Item "master:/sitecore/system/Modules/PowerShell/Script Library/CoreSync/SyncorBackupFunc/" | Execute-Script; Execute-Auto-Site-Backup $params.Item $params.ItemList $params.UserName $params.Password $params.HostURL $params.DBName $params.LangName $params.IncludeChildren $params.BackupMode $params.VersionNumber $params.LookupMode $params.DateTime} $data
