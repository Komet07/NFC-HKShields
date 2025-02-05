# NFC-HKShields
Shields mod for Nebulous: Fleet Command. Mainly created for the Star Wars: Armada Command mod, but ideally should be modular and available for everyone to implement into their own mods

# What does this mod contain?
- Shield Components - a special type of Hull Component with set-up options for various shield health, damage resistance & damage thresholds, etc.
- UI that reflects the current shield health value
- "Ion Shells" (named after their Star Wars equivalents) - special type of explosive shell that does a lot more damage to shields (configurable)
- Networking for UI & VFX in the shield (Currently WIP & in testing)
- VFX for shield impacts (& fragility)

It *should* be possible to make components that can modify shield stats via modifiers, although afaik this hasn't been tested yet. 

## Complete list of shield stats & options
These stats have to be set by the mod-maker in Unity when configuring the shield.
- Max Shield Integrity: How much health (HP) your shield can have. Damage is calculated with the component damage a munition or missile does.
- Shield Recharge Rate: How quick your shield recharges integrity (HP)
- Shield Recharge Delay: How long you have to wait until the shield starts recharging again after damage is taken. Optionally, taking hull damage can also trigger this delay (off by default).
- Shield Damage Resistance: Percentage of damage that doesn't get applied. I.e. a resistance of 30% means only 70% of all damage gets applied.
- Shield Damage Threshold: Minimum damage threshold where shield starts taking damage. Also includes option for whether this threshold should subtract from the given damage or not.
- Fragility: The chance for, above a certain damage threshold, the shield to completely fail (off by default - failure chance and damage threshold can be adjusted).

# How to set up (WIP)
## Installation
Download the latest version of the StarWarsShields .dll and set it up in your unity project (WIP)

## Setting up a basic shield
(WIP)

## Adding your shield to the mod
To add the shield to the mod, add it to your manifest the same as you would with any HullComponent. (WIP)

## Adding HKShields as a dependency
To properly work and avoid any unwanted errors / compatability issues, please do not package the StarWarsShields dll into your own mod - instead, add it as a dependency. 
(WIP)

# Credit
Coded by me (Komet07), tested by and co-designed with HelloThere

Many thanks to AGM-114, PuppyFromHell, someusername6, ShadowLotus for their very valuable and much-appreciated help in getting this to properly work!
