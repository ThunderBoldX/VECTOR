# Controls & presets

[Overview](../README.md) · [Install](INSTALL.md) · [Встановлення українською](INSTALL_UK.md)

Configure profiles on the PC in **Редактор** (Editor). The phone receives the active profile, pauses for changes and keeps its calibration. Press **Грати** (Play) after applying.

## Car / Автомобіль

| Motion | Default Xbox output |
| --- | --- |
| Rotate phone left/right like a steering wheel | Left stick X |
| Tilt forward from neutral | RT / throttle |
| Tilt backward from neutral | LT / brake |

RT and LT are separate analog triggers. In a game, bind them to throttle/brake and check which controller is selected. LT does not automatically select reverse gear; use the game's gearbox or assign a screen button to its reverse command.

The starting profile has no extra screen buttons. You can add handbrake, camera, gear or other controls in the editor.

### Steering at small angles

**Мертва зона керма** is the steering dead zone. **Початок керма** is a separate minimum activation angle; the default is 1°. With zero dead zone and a 1° start, both +1° and −1° activate steering.

**Мінімальний сигнал / компенсація гри** is the initial signal level sent to overcome a game's own controller dead zone. The default 0.25 is a starting point, not a universal game setting. Reduce it toward 0 when the game has no controller dead zone and you want a smoother center. VECTOR's centered output remains zero.

Sensitivity, response curves and smoothing affect how motion feels after activation. If steering still starts late, inspect the game's dead zone too.

## Drone / Дрон · Brick Rigs

| Control | Default assignment |
| --- | --- |
| Left/right phone tilt | Relative mouse X |
| Forward/backward phone tilt | Relative mouse Y |
| **Вгору** (Up) button | Xbox RT |
| **Вниз** (Down) button | Xbox LT |
| **Режим миші (Alt)** | Alt key, tap behavior |
| **Ліворуч / Праворуч** | D-pad left/right |

This preset combines mouse input with Xbox altitude buttons. Enable the game's mouse-control mode and check its actual bindings; the Alt screen button can be reassigned. The preset does not detect the vehicle or edit Brick Rigs settings.

Height output can also use configured keyboard keys. In that mode, bind the same keys in the game. Phone tilt controls the mouse; the altitude buttons are independent.

### Mouse settings

- **Нахил → швидкість миші** (velocity): holding a tilt keeps moving the mouse. Centering stops it. Uses **Максимальна швидкість**, up to 64,000 counts/s.
- **Нахил → зміщення миші** (position): changes in angle generate relative mouse displacement. Uses **Зміщення миші**, up to 1,280 counts/degree.
- **X / ліворуч–праворуч** and **Y / уперед–назад** have independent 0–200% multipliers. 100% is base scale, 50% halves it, 200% doubles it; 0% disables that axis.
- Mouse dead zone and X/Y inversion are separate from car steering parameters. The standard drone preset uses a 1° mouse dead zone and inverted Y.

These are software mouse counts, not physical DPI. The displacement value does not change velocity-mode speed. Adjust the active mode's parameter, the X/Y multipliers and the game's sensitivity together. High limits are available; you do not need to use the maximum.

## Universal / Універсальний

Use this as a starting layout for your own game. Change the name and output type, then add, move or resize controls on the phone-sized canvas. Use **З підключеного телефона** to copy the connected phone's screen size.

A profile supports up to **12** screen buttons. Each has a label, position, size, assignment and behavior:

| Behavior | Action |
| --- | --- |
| Hold | Press while the finger is down, release when lifted |
| Tap | Short press; useful for a discrete action |
| Toggle | Switch on/off with each tap; pause clears held input |

Use the output type appropriate for the game. Xbox has standard controller buttons and axes; keyboard mode sends digital key presses; vJoy provides a separate DirectInput path with up to 32 buttons and requires its own configured driver.

## Save, switch and share

**Зберегти й передати** saves to PC and sends the profile over the existing authenticated connection. Save errors leave the draft in the editor. **Скасувати зміни** restores the stored version.

Changing parameters in the same active profile retains the Host's enabled state. Switching profile, output type or drone height-output type disables input until it is explicitly enabled again after synchronization.

Import/export uses JSON profiles. Profiles contain mappings and layout settings, not remembered-device authentication keys.
