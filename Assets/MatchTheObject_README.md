# Match the Object

Open `Assets/Scenes/BasicScene.unity` and press Play with your Quest connected through Quest Link. Point a controller at a button and press the trigger. For a standalone Quest build, use the Android build profile with OpenXR and BasicScene enabled.

The left object is the target. Change the right object using the shape, color, size and X/Y/Z buttons. Rotation buttons move 15 degrees at a time. Reset restores a red, medium cube at zero rotation. Confirm gives success or a general retry message without revealing which property is wrong.

## Where to look

- **Match the Object** in the Hierarchy has the game script and target settings.
- **Controls Canvas / Panel** contains normal Unity UI Buttons and Text. Each Button's On Click event is connected in the Inspector.
- **Target Object** and **Your Object** each contain three primitive meshes; one is active at a time.
- `Scripts/MatchTheObject.cs` contains the game logic. Public methods correspond to the buttons.
- `Editor/MatchTheObjectSetup.cs` is an editor-only scene creation helper, not needed while playing. The menu **Match the Object > Build BasicScene Interface** rebuilds this activity and replaces any edits inside its root object.

Change the target shape, color, size and rotation on the game component before Play to create a different challenge. Multiples of 15 degrees are easiest to reach with the buttons. The default target is fixed so you can practice repeatedly. Target settings are applied when Play begins.

Validation allows 15 degrees of angular difference. Sphere rotation is ignored because it cannot be seen. Cylinder spin and flipped directions are accepted; cube rotations with the same visible faces are accepted too. There are no hidden orientation markings to guess.

The existing XR rig and EventSystem are reused. The world-space Canvas has a Tracked Device Graphic Raycaster for controller input and a Graphic Raycaster for mouse input in the editor.

## Quick manual check

1. Confirm immediately: expect a retry message.
2. Try all nine shape/color/size buttons and all six rotation buttons.
3. Reset: expect a medium red cube and X/Y/Z all zero.
4. Match the target and confirm: expect a green success message.
5. Change one property after success: feedback should return to the neutral prompt.

Unity's setup checks cover button wiring, correct/incorrect answers, reset rotation, rotation tolerance and symmetry. Controller pointing and comfort should also be tested in the actual headset.
