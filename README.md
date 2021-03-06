<h1 align="center"> HoloHelp: HoloLens Object Detection for a Guided Interaction </h1>

<p align="center">
  <img alt="HoloLogo" src="https://user-images.githubusercontent.com/55694345/144706173-1c2131ac-d5e5-4e53-9263-99c84b656ecb.png" width=40% height=40%>
</p><br>

My bachelor's degree thesis project @ University of Catania, 2021.

The thesis has the primary aim of helping, assisting and supporting people in the usage of a specific object observed or interacted with.

Starting from Microsoft HoloLens 2, the idea was successfully implemented with a customized Object Detector trained in cloud via Microsoft Azure. To do that, we have used 9 specific classes of the COCO dataset to upload over 1000 images.

<p align="center">
  <img alt="usage" src="https://user-images.githubusercontent.com/55694345/144706320-d010add7-666b-44ae-83d4-0edb81d1f3b2.jpg" width=60% height=60%>
</p><br>

The goal is to make the tool as much manageable and easy to use as possible. Therefore, the idea is to use a single, simple voice command, HoloHelp. Nothing more. Once pronounced, HoloLens will immediately take a picture, saving the eye gaze coordinates of the object that the user is looking at. Once saved the picture with those information, an API request will be sent to Microsoft Azure Custom Vision, and a small audio and AR video guide that explains the usage of the object detected will appear.

[![HoloHelp](https://user-images.githubusercontent.com/55694345/149018993-d159b8bc-6ac9-47ac-9665-d5d77d4b7e74.png)](https://youtu.be/ZugR1hTd4bY)

## Credits
The thesis has been supervised by the professor [Antonino Furnari](http://www.antoninofurnari.it), in collaboration with [NEXT VISION](https://www.nextvisionlab.it/).
