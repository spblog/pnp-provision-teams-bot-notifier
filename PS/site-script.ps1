$site_script = @'
{
    "$schema": "schema.json", 
    "actions": [
		{
				"verb": "triggerFlow",
				"url": "<logic app url>",
				"name": "Apply PnP Template",
				"parameters": {
					"event":"",
					"product":""
				}
		}
    ],
    "bindata": {},
    "version": 1
}
'@

Add-PnPSiteScript -Title "Trigger Contoso landing provisioning" -Content $site_script -Description "Applies Contoso landing site template via azure web job"