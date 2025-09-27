# Combat:Redux Source Code Repository
This Repository is for the Distrobution And Contribution of the Source Code for Combat:Redux.

## How to use?
Inside `/Scripts/` You will find a somewhat organized list of Source Files. These files are what drives the Current Builds of Combat:Redux.
To Read, Edit, Contribute, or Criticise them, Please Clone the repository, and Open it in your Favorite IDE.

If you want to be able to Read thru Dependency Classes such as "`Debug`" or, "`PlayerRef`", Paste the `/Scripts/` folder into a Unity Project, with the following Dependencies Installed: 
<details>
<summary>Dependencies</summary>
  
- "photon fusion 2": "latest"
- "textmeshpro": "latest"
- "com.unity.2d.sprite": "1.0.0"
- "com.unity.collab-proxy": "2.8.2"
- "com.unity.feature.development": "1.0.1"
- "com.unity.nuget.mono-cecil": "1.10.2"
- "com.unity.probuilder": "5.2.4"
- "com.unity.splines": "2.5.2"
- "com.unity.textmeshpro": "3.0.9"
- "com.unity.timeline": "1.7.6"
- "com.unity.ugui": "1.0.0"
- "com.unity.visualscripting": "1.9.2"
- "com.unity.xr.interaction.toolkit": "2.5.4"
- "com.unity.xr.oculus": "4.2.0"
- "com.unity.xr.openxr": "1.10.0"
- "com.unity.modules.ai": "1.0.0"
- "com.unity.modules.androidjni": "1.0.0"
- "com.unity.modules.animation": "1.0.0"
- "com.unity.modules.assetbundle": "1.0.0"
- "com.unity.modules.audio": "1.0.0"
- "com.unity.modules.cloth": "1.0.0"
- "com.unity.modules.director": "1.0.0"
- "com.unity.modules.imageconversion": "1.0.0"
- "com.unity.modules.imgui": "1.0.0"
- "com.unity.modules.jsonserialize": "1.0.0"
- "com.unity.modules.particlesystem": "1.0.0"
- "com.unity.modules.physics": "1.0.0"
- "com.unity.modules.physics2d": "1.0.0"
- "com.unity.modules.screencapture": "1.0.0"
- "com.unity.modules.terrain": "1.0.0"
- "com.unity.modules.terrainphysics": "1.0.0"
- "com.unity.modules.tilemap": "1.0.0"
- "com.unity.modules.ui": "1.0.0"
- "com.unity.modules.uielements": "1.0.0"
- "com.unity.modules.umbra": "1.0.0"
- "com.unity.modules.unityanalytics": "1.0.0"
- "com.unity.modules.unitywebrequest": "1.0.0"
- "com.unity.modules.unitywebrequestassetbundle": "1.0.0"
- "com.unity.modules.unitywebrequestaudio": "1.0.0"
- "com.unity.modules.unitywebrequesttexture": "1.0.0"
- "com.unity.modules.unitywebrequestwww": "1.0.0"
- "com.unity.modules.vehicles": "1.0.0"
- "com.unity.modules.video": "1.0.0"
- "com.unity.modules.vr": "1.0.0"
- "com.unity.modules.wind": "1.0.0"
- "com.unity.modules.xr": "1.0.0"
  
</details>

## Any Questions, Concearns or Desire for a Word?
You can at all times reach me under these sources:
<details>
<summary>Contacts</summary>
  
- Discord : `@furofloof`
- Telegram : `@furofloof`
- Reddit : `u/furofloof`
- [Bluesky](https://bsky.app/profile/furofloof.com)
- [Twitter](https://x.com/furofloof) 
- [YouTube](https://youtube.com/@furofloof) 
- [GitHub](https://github.com/FuroFloof) 
- [WebSite](https://furofloof.com) 
- Email : `admin@furofloof.com`
  
</details>

Alternatively, you should consider joining the [Monosodium Discord](https://discord.gg/QYczNyKkse).

## How to Contribute?
If you want to receive the proper credit, please add a comment close to the changed content,
including your name, the date, and the change in no more than 2 lines.

<details>

<summary>Example:</summary>

```javascript
[...]
singletonManager.ToggleComponents(isLocal);
if (isLocal) {

    SavePublicToken(cr_account.GetLocalTokenForSelf());
    foreach(var mr in BodyRenderers)
    {
        // FuroFloof @ 29.4.2025 - shadowCastingMode is applied in cr_singletonManager, so this is obsolete.
        //mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
    }
} else {
     
}
[...]
```

</details>

## Can i currently play Combat:Redux anywhere?
Combat:Redux is currently still in Early Development, and is only available on Steam, via a **Beta Steam-Key**.
You may request a Steam-Key in the [Discord Server](https://discord.gg/QYczNyKkse), or DM me for one, however, by accepting a Steam-Key, you should also
try to find the time to take part in Semi-Regular **Group-Playtests** when they are being held. If not, that is fine, but remember that the purpose of
Beta-Tests is to... well... test!

## Want to use Code from Combat:Redux in your own Projects?
I believe people should freely be able to take inspiration (steal) from other public sources, and use them for personal or commercial projects, however
it is so also important to credit apropriately. 

If you use code from here, or use code from here as a reference, i would kindly request that you credit me or this repository if applicable.
