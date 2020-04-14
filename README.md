# BubbleShooterGame
![BubbleShooterSpriteSheet](https://user-images.githubusercontent.com/62396712/79257135-7a8e8000-7e89-11ea-9be0-83d2a05591ad.jpg)

### Watch a Demo Video On Youtube
**[Video Link](https://youtu.be/BJlwRPlcW1I)**

### Try it on your phone
**[Apk Link](https://1drv.ms/u/s!Ambq7X4wLes3pXFV2-6n8Xwp62y1?e=mgfalJ)**

### Source Code Guide

I would start talk about the “**GameController**” , from that script we can add bubbles types ,  colors and how what the limit should the grid generate! 

![GameController_1](https://user-images.githubusercontent.com/62396712/79257582-1f10c200-7e8a-11ea-81fa-d258afe3369b.PNG)

Also we can setup the merging behavior of the bubbles:
1.	Highest Bubble
2.	Biggest Neighbors Count
3.	Highest Bubble Then Biggest Neighbors Count
4.	Biggest Neighbors Count Then Highest Bubble
And we can set Aim Settings which gonna be “**Aim on Bubble**” or “**Aim on Available Neighbor**”.

![GameController_2](https://user-images.githubusercontent.com/62396712/79257699-4c5d7000-7e8a-11ea-891a-5faf232e23a4.PNG)


Next is The “**BubbleGrid**” Script, that allow us to set the grid dimension, how many row will be generated at the start of the game, the bubbles size and the grid alignment.

![Grid_1](https://user-images.githubusercontent.com/62396712/79258171-06ed7280-7e8b-11ea-8021-d4ea16b6edce.PNG)
![Grid_2](https://user-images.githubusercontent.com/62396712/79257787-6e56f280-7e8a-11ea-8139-5132f6fed9bf.PNG)

Also the script has parameters of the bubbles animations inside the grid.

![Grid_3](https://user-images.githubusercontent.com/62396712/79257799-7151e300-7e8a-11ea-9740-6d86d2a939ee.PNG)

And finally the “**BubbleShooter**” script, we can setup the aim and animations.

![Shooter_1](https://user-images.githubusercontent.com/62396712/79257969-b7a74200-7e8a-11ea-9846-8cf3ea867fa3.PNG)
![shooter_2](https://user-images.githubusercontent.com/62396712/79257973-baa23280-7e8a-11ea-9117-2df388f399f3.PNG)

So, that’s a short glance about the source code, there is more but I selected the most important scripts.

