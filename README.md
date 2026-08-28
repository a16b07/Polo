# One Man Rave 🕺🔫

A first-person roguelite shooter built in Unity. You play as **Polo**, a man who drank his way out of his own nightclub — and now he's fighting his way back in, one wave at a time.

Built for **Pixel Borregos' Game Jam 2026**.

## The Story

Polo used to own the club. Then the bottle took it from him. Now he's back, gun in hand, ready to clear the dance floor of everything standing between him and getting his life — and his nightclub — back.

Scattered through the club are three **Consejos** (pieces of advice) waiting to be found. Whether Polo listens to them determines which of the game's two endings he gets.

## Gameplay

- **Wave-based combat** — clear out each wave of enemies pulsing under the strobe lights before the next one drops in.
- **Risk/reward perk system** — after every wave, roll a perk. Most are stat buffs and nerfs (speed, damage, damage reduction, luck, ammo, projectile stats), but rarer rolls grant powerful passive effects — piercing rounds, explosive rounds, double jump, infinite ammo bursts, permanent damage stacking, and more. Some rolls hurt as much as they help.
- **Guns and knives** — pick up, swap, and throw weapons found around the club, or fall back on your knife when you're out of ammo.
- **Collectibles** — track down the three hidden Consejos to unlock the good ending.
- **A club that fights back** — strobing disco lights, flashbang grenades, and a gritty, low-fi visual filter set the mood.

## Controls

| Action | Input |
|---|---|
| Move | `WASD` |
| Look | Mouse |
| Shoot / Attack | Left Click |
| Interact / Pick up | `E` |
| Jump | `Space` |
| Crouch | `C` |
| Sprint | `Left Shift` |
| Switch Weapon | `1` / `2` |

*(Gamepad support is also included.)*

## Built With

- **Engine:** Unity 6 (6000.4.7f1)
- **Render Pipeline:** Universal Render Pipeline (URP), with custom post-processing (bitcrush / VHS-style filter)
- **Input:** Unity's new Input System

## Getting Started

1. Clone the repo:
   ```bash
   git clone https://github.com/max-lopzzz/OneManRave.git
   ```
2. Open the project folder in **Unity Hub** using Unity `6000.4.7f1` (or newer).
3. Open `Assets/Scenes/SampleScene.unity`.
4. Hit Play.

## Project Structure

```
Assets/
├── Scripts/          # Gameplay code (player, enemies, weapons, perks, waves)
├── Scenes/           # Main game scene
├── sprites buffs/    # Perk/buff icon art
├── cutscenes/        # Story cutscenes and ending gifs
├── Sound/            # Audio
└── ...
```

## Roadmap / Ideas

- [ ] Additional weapons and perks
- [ ] More waves / enemy variety
- [ ] Polished menus and settings
- [ ] Full playtest balancing pass on perk drop rates

## License

*Add your license of choice here.*

## Credits

Made by [max-lopzzz](https://github.com/max-lopzzz).
