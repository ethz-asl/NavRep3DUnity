Project Unity version: 2020.1.3f1

# NavRep3DUnity

This is the Unity project used to build the *simple*, *city*, and *office* scenarios for the [NavDreams](https://github.com/danieldugas/NavDreams) reinforcement-learning paper.

(Looking for the *modern*, *cathedral*, *gallery*, and *replica* scenarios? [This way.](https://www.github.com/danieldugas/NavDreamsUnity) )

![titleimage](github_media/city_unity.png)


## Installing Unity on Ubuntu

- Download Unity Hub from [here](https://unity3d.com/get-unity/download), [direct link](https://public-cdn.cloud.unity3d.com/hub/prod/UnityHub.AppImage)
- From the [unity archive](https://unity3d.com/get-unity/download/archive), Find the version you want, right click the green button "Unity Hub", and copy the link. Then, open the link in unity hub like so:
```
chmod +x ~/Downloads/UnityHub.AppImage
~/Downloads/UnityHub.AppImage unityhub://2020.1.3f1/cf5c4788e1d8
```

## Opening The Project

- make sure to clone with --recursive
- open the project in unity (run the `UnityHub.AppImage` executable and select the project folder).
- open `Assets/Scenes/SimpleRL.unity`, and the *simple* scenario should show. Press play, then in another terminal run `python -m navdreams.navrep3dtrainenv --unity-player-dir None` to allow moving the robot with keyboard keys.
