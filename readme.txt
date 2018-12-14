----------Important-----------
This prototype, importantly, uses ResonanceAudio to handle spatial sound.
Many parameters in the current project should be modified when using new
game objects and audio sources. This can be done in the audio source and
in the ResonanceAudio source components.

-------Enabling Resonance-----
In order to make Resonance work properly, it has to downloaded (can be
found on Github) and enabled in edit --> project settings --> audio.
Select it as the spatializer plugin and ambisonic decoder. Make sure that
'virtualize effects' is enabled.

---------Using Resonance-------
Using Resonance audio sources are as simple as standard 3D sound sources
in Unity, but with extra parameters and features. These will need to be
set on a per-item basis and be changed by whatever controls the
environment in order to sound accurate. I would recommend skimming the
documentation for the API. It isn't super complicated and can be working
in a matter of minutes.

Make sure that the camera/ listening source has an audio listener / a
Resonance audio listener. Otherwise the sound will not be generated
properly. Similarly, make sure the sound source (currently the trees)
have 3D sound and spatialization enabled. I'd recommend a custom rolloff.

--------General Project--------
For the basics of this prototype, much of the code (ie. the plane gen.)
is using the basic ARCore code or slightly modified versions of it. Only
the MainController.cs script should be of special interest when adapting
the code to another project. It is the controller which handles raycasts,
tracking prefabs, and other fundamental functions. It contains thorough
comments and should be pretty readable. The SpawnModel() function in
particular should be useful if adapting this script to another program.


-------That should be enough to get started!--------
StageAR made by Rikesh Subedi using the Google ARCore and ResonanceAudio APIs
