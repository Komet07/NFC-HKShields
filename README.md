# NFC-HKShields
Shields mod for Nebulous: Fleet Command. Mainly created for the Star Wars: Armada Command mod, but ideally should be modular and available for everyone to implement into their own mods

# What does this mod contain?
- Shield Components - a special type of Hull Component with set-up options for various shield health, damage resistance & damage thresholds, etc.
- UI that reflects the current shield health value
- "Ion Shells" (named after their Star Wars equivalents) - special type of explosive shell that does a lot more damage to shields (configurable)
- Networking for UI & VFX in the shield
- VFX for shield impacts (& fragility)

**It is possible to make components that can modify shield stats via modifiers, which have the following codes:**
- shield-maxInteg : Maximum Integrity of the shield
- shield-rechargeRate : Recharge Rate for the shield
- shield-rechargeDelay : Delay until shield starts recharging
- shield-resistance : Percentage of damage that doesn't get applied to shield
- shield-threshold : Damage Threshold (DT) after which shield takes damage
- shield-fragilityBase : Base Fragility Chance
- shield-fragilityThreshold: Threshold after which shield becomes fragile.

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
Download the latest version of the StarWarsShields .dll and include it in your Unity project (the lib folder with the rest of the Neb dlls)

## Setting up a basic shield

**Step 1: Required Scripts**
To set up a shield, you basically need two scripts:
- ShieldHull.cs, a modified version of a HullComponent which includes the shield's stats
- ShieldSW.cs, a script which actually runs the shield and contains some extra options needed to get the shield to work.

**Step 2: Setting up ShieldHull.cs**
Set up your ShieldHull's stats the same you would for any other HullComponent. You can then find the Shield's Stats in the Bottom of the Inspector for the script. 

**Step 3: Setting up ShieldSW.cs**
Once you've added ShieldSW, you'll also see a MeshComponent which'll automatically be added. This is where you put the mesh for your shield, which'll automatically later be stretched to fit the ship it's on. We recommend a low-vertex sphere (NOTE: Should ideally be <256 tris to prevent Unity from complaining about it). 

Now, you also have some options that you need to set here:
- Scale Factor (under Collider Stats) should be pre-set to 1.25 - this is a value we've found works well to envelop most ships pretty well.
- Shield Icon: This is required for the UI - a basic template Icon has been provided in the #mod-showcase post, but any icon will do. We recommend a small resolution - we use a 72x72px sprite.
- VFX: This is optional, but the VFX is what'll play when your shield gets hit (the basic _VFX stat) or when the fragility stuff happens (_fragileVFX)

You can pretty much ignore everything else in ShieldSW.cs!

## Adding your shield to the mod
To add the shield to the mod, add it to your manifest and asset bundle the same as you would with any HullComponent.

## Adding HKShields as a dependency
To properly work and avoid any unwanted errors / compatability issues, please do not package the StarWarsShields dll into your own mod - instead, add it as a dependency!

# Credit
Coded by me (Komet07), tested by and co-designed with HelloThere

Many thanks to AGM-114, PuppyFromHell, someusername6 and ShadowLotus for their very valuable and much-appreciated help in getting this to properly work!
