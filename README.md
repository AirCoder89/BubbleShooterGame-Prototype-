# BubbleShooterGame

<table>
  <tr>
    <td><img src="https://github.com/user-attachments/assets/628d98c6-0224-48a2-b3e3-321b5f48e681" alt="InspectMe Logo" width="100"></td>
    <td>
      🛠️ Boost your Unity workflows with <a href="https://divinitycodes.de/">InspectMe</a>! Our tool simplifies debugging with an intuitive tree view. Check it out! 👉 
      <a href="https://assetstore.unity.com/packages/tools/utilities/inspectme-lite-advanced-debugging-code-clarity-283366">InspectMe Lite</a> - 
      <a href="https://assetstore.unity.com/packages/tools/utilities/inspectme-pro-advanced-debugging-code-clarity-256329">InspectMe Pro</a>
    </td>
  </tr>
</table>

---

Dive into the vibrant world of Bubble Shooter Game, where strategy meets fun! This game challenges you to match and clear bubbles on a dynamic grid using precision and quick thinking.

## Watch the Demo
Catch a glimpse of the gameplay and mechanics in action:
**[Watch the Video Demo on YouTube](https://youtu.be/BJlwRPlcW1I)**

## Source Code Guide

### GameController
The `GameController` script is where the magic begins. It allows for custom configurations of bubble types, colors, and grid generation limits.

#### Merging Behaviors
You can define how bubbles will merge based on various criteria:
1. Highest Bubble
2. Biggest Neighbors Count
3. Highest Bubble Then Biggest Neighbors Count
4. Biggest Neighbors Count Then Highest Bubble

#### Aim Settings
Choose your aiming method:
- **Aim on Bubble**: Directly target a bubble.
- **Aim on Available Neighbor**: Target available neighboring spaces.

![GameController_1](https://user-images.githubusercontent.com/62396712/79257582-1f10c200-7e8a-11ea-81fa-d258afe3369b.PNG)
![GameController_2](https://user-images.githubusercontent.com/62396712/79257699-4c5d7000-7e8a-11ea-891a-5faf232e23a4.PNG)

### BubbleGrid
This script sets up the grid dimensions, initial rows, bubble sizes, and alignment. It also controls the animations of bubbles within the grid.

![Grid_1](https://user-images.githubusercontent.com/62396712/79258171-06ed7280-7e8b-11ea-8021-d4ea16b6edce.PNG)
![Grid_2](https://user-images.githubusercontent.com/62396712/79257787-6e56f280-7e8a-11ea-8139-5132f6fed9bf.PNG)
![Grid_3](https://user-images.githubusercontent.com/62396712/79257799-7151e300-7e8a-11ea-9740-6d86d2a939ee.PNG)

### BubbleShooter
Finally, the `BubbleShooter` script allows for the setup of the aim and animations for shooting bubbles, providing you with all the tools needed for precise gameplay.

![Shooter_1](https://user-images.githubusercontent.com/62396712/79257969-b7a74200-7e8a-11ea-9846-8cf3ea867fa3.PNG)
![Shooter_2](https://user-images.githubusercontent.com/62396712/79257973-baa23280-7e8a-11ea-9117-2df388f399f3.PNG)

### Conclusion
This brief overview covers the core functionalities and scripts of the Bubble Shooter Game. The source code is designed for easy navigation and modification, offering insights into effective game mechanics and UI handling in Unity.

---

<table>
  <tr>
    <td><img src="https://github.com/user-attachments/assets/628d98c6-0224-48a2-b3e3-321b5f48e681" alt="InspectMe Logo" width="100"></td>
    <td>
      🛠️ Boost your Unity workflows with <a href="https://divinitycodes.de/">InspectMe</a>! Our tool simplifies debugging with an intuitive tree view. Check it out! 👉 
      <a href="https://assetstore.unity.com/packages/tools/utilities/inspectme-lite-advanced-debugging-code-clarity-283366">InspectMe Lite</a> - 
      <a href="https://assetstore.unity.com/packages/tools/utilities/inspectme-pro-advanced-debugging-code-clarity-256329">InspectMe Pro</a>
    </td>
  </tr>
</table>
