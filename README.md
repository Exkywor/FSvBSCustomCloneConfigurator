[![Download latest release](https://github.com/Exkywor/FSvBSCustomCloneConfigurator/blob/master/FSvBSCustomCloneUtility/resources/images/splashimage_banner.png)](https://github.com/Exkywor/FSvBSCustomCloneUtility/releases/latest)

Tool to configure a custom appearance of the Citadel clone for _Mass Effect 3_ and _Mass Effect 3 Legendary Edition_.  

In order to use this tool, you must first install the FemShep v Broshep mod.  
[Mass Effect 3](https://www.nexusmods.com/masseffect3/mods/975)  
[Mass Effect 3 Legendary Edition](https://www.nexusmods.com/masseffectlegendaryedition/mods/850)

## How to use?
Follow these steps to apply a custom head morph to the Citadel clone:  
1. Download the [latest release](https://github.com/Exkywor/FSvBSCustomCloneUtility/releases/latest) of the tool.  
2. Extract it and launch the **FSvBSC3.exe** file. You'll get the following window:  

![2](https://i.imgur.com/rvXskWS.png)  

3. In the **GAME TARGET** section, select for which game you want to apply the morphs. You need to install either the _normal_ or _vanilla vs_ versions of the mod beforehand.  

![3](https://i.imgur.com/GltvqzP.png)  

If the tool cannot automatically find your game installation, it will prompt you to browse and select the **MassEffect3.exe** file.  

4. In the **CLONE'S APPEARANCE** section you can control the appearance of the male or female clones. Selecting **Default** will set the clone to use the iconic default Femshep/Broshep appearance. Selecting **Custom** will prompt you to select the **.ron** file containing the head morph to apply. Read how to get a **.ron** file at the end of this section.  

![4](https://i.imgur.com/OZVDmvy.png[/img)  

Do not apply a male morph to the female clone or vice-versa. Due to differences in the meshes, it will lead to bad results or outright throw an error.  

If your morph uses any modded resources, like hairs or textures, make sure to have said resources installed before applying the morphs. In the case that the tool can't find a resource, you'll receive a message with the list of problematic resources.  
If you don't have access to those resources anymore, you can remove those lines from the **.ron** file in any text editor.  

5. Click **Apply**. The tool will run its magic, and you'll receive a message once the operations are complete.  

![5](https://i.imgur.com/aXKODKi.png)  

If you want to have one of the clones go back to the default appearance, simply launch the tool, select **Default**, and click **Apply**. Do note that applying removes any previously used morph, so make sure to select any morphs you want to keep using.  

### How to get a head morph into a **.ron** file:
1. Download, launch and install the [Trilogy Save Editor](https://www.nexusmods.com/masseffectlegendaryedition/mods/20).
2. Open the save that has the head morph you want to use.
3. Go to the **Head Morph** tab, select **Export**, and save the **.ron** file.

## License
FSvBSCustomCloneConfigurator is licensed under GPLv3.
